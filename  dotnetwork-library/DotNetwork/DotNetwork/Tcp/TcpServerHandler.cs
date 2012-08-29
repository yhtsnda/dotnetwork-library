/*
 * AUTHOR: Utermiko
 * Modified by: <none>
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DotNetwork.Tcp
{
    /// <summary>
    /// Class that represents server
    /// </summary>
    public class TcpServerHandler
    {
        private IPAddress ListeningIP = IPAddress.Any;
        private int ListeningPort = 7000;
        private int BufferSize = 8192;
        private int MaxQueue = 50;
        private Socket ListeningSocket = null;
        private Thread ListeningThread = null;
        private List<ClientInfo> ClientList;

        /// <summary>
        /// Performed when server started listening for new clients
        /// </summary>
        public event EventHandler OnListenStarted;
        /// <summary>
        /// Performed when new client connected to server
        /// </summary>
        public event ClientActionHandler OnClientConnected;
        /// <summary>
        /// Performed when client disconnected from server
        /// </summary>
        public event ClientActionHandler OnClientDisconnected;
        /// <summary>
        /// Performed when message arrives
        /// </summary>
        public event MessageReceivedHandler OnMessageReceived;
        /// <summary>
        /// Performed when error occurred
        /// </summary>
        public event ErrorHandler OnErrorOccurred;

        /// <summary>
        /// Constructor of TcpServerHandler
        /// </summary>
        /// <param name="port">Port on which server will listening</param>
        public TcpServerHandler(int port)
        {
            Constructor(ListeningIP, port, BufferSize, MaxQueue);
        }
        /// <summary>
        /// Constructor of TcpServerHandler
        /// </summary>
        /// <param name="port">Port on which server will listening</param>
        /// <param name="ip">IP on which server will listening</param>
        public TcpServerHandler(int port, string ip)
        {
            try
            {
                IPAddress.Parse(ip);
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Bad IP", Error.BadIP);
                return;
            }
            Constructor(IPAddress.Parse(ip), port, BufferSize, MaxQueue);
        }
        /// <summary>
        /// Constructor of TcpServerHandler
        /// </summary>
        /// <param name="port">Port on which server will listening</param>
        /// <param name="ip">IP on which server will listening</param>
        public TcpServerHandler(int port, IPAddress ip)
        {
            Constructor(ip, port, BufferSize, MaxQueue);
        }
        /// <summary>
        /// Constructor of TcpServerHandler
        /// </summary>
        /// <param name="port">Port on which server will listening</param>
        /// <param name="ip">IP on which server will listening</param>
        /// <param name="bufferSize">Max receive buffer size</param>
        /// <param name="maxQueue">Max queue of connecting clients</param>
        public TcpServerHandler(int port, string ip, int bufferSize, int maxQueue)
        {
            try
            {
                IPAddress.Parse(ip);
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Bad IP", Error.BadIP);
                return;
            }
            Constructor(IPAddress.Parse(ip), port, bufferSize, maxQueue);
        }
        /// <summary>
        /// Constructor of TcpServerHandler
        /// </summary>
        /// <param name="port">Port on which server will listening</param>
        /// <param name="ip">IP on which server will listening</param>
        /// <param name="bufferSize">Max receive buffer size</param>
        /// <param name="maxQueue">Max queue of connecting clients</param>
        public TcpServerHandler(int port, IPAddress ip, int bufferSize, int maxQueue)
        {
            Constructor(ip, port, bufferSize, maxQueue);
        }
        private void Constructor(IPAddress ip, int port, int buffersize, int maxqueue)
        {
            if (port < 0 || port > 65535)
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Bad port", Error.BadPort);
                return;
            }
            if (buffersize <= 0)
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Bad buffer size", Error.BadBufferSize);
                return;
            }
            ListeningIP = ip;
            ListeningPort = port;
            BufferSize = buffersize;
            MaxQueue = maxqueue;
            ClientList = new List<ClientInfo>();
            ListeningSocket = new Socket(ListeningIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                ListeningSocket.Bind(new IPEndPoint(ListeningIP, ListeningPort));
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Probably on this port listens another application", Error.SocketBindError);
                return;
            }
        }

        /// <summary>
        /// Starts listening for new clients
        /// </summary>
        /// <exception cref="SocketException"></exception>
        public void StartListening()
        {
            ListeningThread = new Thread(new ThreadStart(ListeningMethod));
            ListeningThread.IsBackground = true;
            ListeningThread.Start();
        }
        /// <summary>
        /// Stops listening for new clients
        /// </summary>
        public void StopListening()
        {
            if (ListeningThread != null) ListeningThread.Abort();
            if (ListeningSocket != null) ListeningSocket.Close();
        }
        /// <summary>
        /// Stop listening for new clients with specified timeout
        /// </summary>
        /// <param name="timeout">Timeout to close socket</param>
        public void StopListening(int timeout)
        {
            if (ListeningThread != null) ListeningThread.Abort();
            if (ListeningSocket != null) ListeningSocket.Close(timeout);
        }

        /// <summary>
        /// Kick specified client from server
        /// </summary>
        /// <param name="client">Client to kick from server</param>
        /// <exception cref="ArgumentException"></exception>
        public void Kick(ClientInfo client)
        {
            try
            {
                client.ClientResources.Connection.Close();
                //if (client.ClientResources.ConnectionThread != null) client.ClientResources.ConnectionThread.Abort();
                lock (ClientList) ClientList.Remove(client);
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "The specified client doesn't connected with the server", Error.KickFailed);
            }
        }
        /// <summary>
        /// Kick all connected clients from server
        /// </summary>
        public void KickAll()
        {
            foreach (ClientInfo cli in ClientList)
            {
                try
                {
                    cli.ClientResources.Connection.Close();
                    //if (cli.ClientResources.ConnectionThread != null) cli.ClientResources.ConnectionThread.Abort();
                    lock (ClientList) ClientList.Remove(cli);
                }
                catch { }
            }
        }

        /// <summary>
        /// Sends data to specified client
        /// </summary>
        /// <param name="client">Message recipient</param>
        /// <param name="data">Data to sent</param>
        /// <exception cref="ArgumentException"></exception>
        public void Send(ClientInfo client, byte[] data)
        {
            try
            {
                client.ClientResources.Connection.Send(data);
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "The specified client doesn't connected with the server", Error.KickFailed);
                return;
            }
        }
        /// <summary>
        /// Sends data to all connected clients
        /// </summary>
        /// <param name="data">Data to sent</param>
        public void SendToAll(byte[] data)
        {
            lock (ClientList)
            {
                foreach (ClientInfo client in ClientList)
                {
                    try
                    {
                        client.ClientResources.Connection.Send(data);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Get list of connected client
        /// </summary>
        /// <returns>List of connected clients</returns>
        public List<ClientInfo> GetClientList()
        {
            return ClientList;
        }
        /// <summary>
        /// Get string array with connected clients IPs
        /// </summary>
        /// <returns>String array with connected clients IPs</returns>
        public string[] GetClientsStringIP()
        {
            string[] o;
            lock (ClientList)
            {
                o = new string[ClientList.Count];
                for (int i = 0; i < ClientList.Count; ++i)
                {
                    try
                    {
                        o[i] = ClientList[i].IP.ToString().Split(new Char[] { ':' })[0];
                    }
                    catch { }
                }
            }
            return o;
        }
        /// <summary>
        /// Get array with connected clients IPs
        /// </summary>
        /// <returns>Array with connected clients IPs</returns>
        public IPAddress[] GetClientsIP()
        {
            IPAddress[] o;
            lock (ClientList)
            {
                o = new IPAddress[ClientList.Count];
                string os = "";
                for (int i = 0; i < ClientList.Count; ++i)
                {
                    try
                    {
                        os = ClientList[i].IP.ToString().Split(new Char[] { ':' })[0];
                        o[i] = IPAddress.Parse(os);
                    }
                    catch { }
                }
            }
            return o;
        }

        private void ListeningMethod()
        {
            try
            {
                ListeningSocket.Listen(MaxQueue);
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Probably on this port listens another application", Error.ListeningFailed);
                return;
            }
            if (OnListenStarted != null) OnListenStarted(this, EventArgs.Empty);

            while (true)
            {
                ClientInfo ci = new ClientInfo();
                ci.ClientResources.Connection = ListeningSocket.Accept();
                ci.ClientResources.ConnectionThread = new Thread(new ParameterizedThreadStart(ConnectionMethod));
                ci.ClientResources.ConnectionThread.IsBackground = true;
                ci.ClientResources.ConnectionThread.Start(ci);
                ci.IP = ci.ClientResources.Connection.RemoteEndPoint;
                lock (ClientList) ClientList.Add(ci);
                if (OnClientConnected != null) OnClientConnected(this, ci);
            }
        }
        private void ConnectionMethod(object cinf)
        {
            ClientInfo client = (ClientInfo)cinf;
            client.ClientResources.Connection.ReceiveBufferSize = BufferSize;
            byte[] buffer = new byte[BufferSize];
            int bufflen = 0;

            while (true)
            {
                try
                {
                    bufflen = client.ClientResources.Connection.Receive(buffer);
                }
                catch//dc
                {
                    break;
                }
                if (bufflen == 0) break;//dc

                //msg recv
                if (OnMessageReceived != null) OnMessageReceived(this, new PacketEventArgs(buffer, bufflen, client));
            }

            if (OnClientDisconnected != null) OnClientDisconnected(this, client);
            try
            {
                client.ClientResources.Connection.Close();
                lock (ClientList) ClientList.Remove(client);
            }
            catch { }
        }
    }
}
