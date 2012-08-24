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
    /// Class that represents client
    /// </summary>
    public class TcpClientHandler
    {
        private IPAddress IP = IPAddress.Parse("127.0.0.1");
        private int Port = 7000;
        private int BufferSize = 8192;
        private Socket Communication = null;
        private Thread ReceiveThread = null;

        /// <summary>
        /// Performed when client successful connected to server
        /// </summary>
        public event EventHandler OnConnected;
        /// <summary>
        /// Performed when client disconnected from server
        /// </summary>
        public event EventHandler OnDisconnected;
        /// <summary>
        /// Performed when message arrives
        /// </summary>
        public event MessageReceivedHandler OnMessageReceived;
        /// <summary>
        /// Performed when error occurred
        /// </summary>
        public event ErrorHandler OnErrorOccurred;

        /// <summary>
        /// Constructor of TcpClientHandler
        /// </summary>
        /// <param name="ip">IP to connect</param>
        /// <param name="port">Port to connect</param>
        public TcpClientHandler(string ip, int port)
        {
            try
            {
                IPAddress.Parse(ip);
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Bad IP", Error.BadIP);
            }
            Constructor(IPAddress.Parse(ip), port, BufferSize);
        }
        /// <summary>
        /// Constructor of TcpClientHandler
        /// </summary>
        /// <param name="ip">IP to connect</param>
        /// <param name="port">Port to connect</param>
        public TcpClientHandler(IPAddress ip, int port)
        {
            Constructor(ip, port, BufferSize);
        }
        /// <summary>
        /// Constructor of TcpClientHandler
        /// </summary>
        /// <param name="ip">IP to connect</param>
        /// <param name="port">Port to connect</param>
        /// <param name="bufferSize">Max receive buffer size</param>
        public TcpClientHandler(string ip, int port, int bufferSize)
        {
            try
            {
                IPAddress.Parse(ip);
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Bad IP", Error.BadIP);
            }
            Constructor(IPAddress.Parse(ip), port, bufferSize);
        }
        /// <summary>
        /// Constructor of TcpClientHandler
        /// </summary>
        /// <param name="ip">IP to connect</param>
        /// <param name="port">Port to connect</param>
        /// <param name="bufferSize">Max receive buffer size</param>
        public TcpClientHandler(IPAddress ip, int port, int bufferSize)
        {
            Constructor(ip, port, bufferSize);
        }
        private void Constructor(IPAddress ip, int port, int buffersize)
        {
            if (port < 0 || port > 65535) if (OnErrorOccurred != null) OnErrorOccurred(this, "Bad port", Error.BadPort);
            if (buffersize <= 0) if (OnErrorOccurred != null) OnErrorOccurred(this, "Bad buffer size", Error.BadBufferSize);
            IP = ip;
            Port = port;
            BufferSize = buffersize;
            Communication = new Socket(IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// Try to connect to server specified in constructor
        /// </summary>
        public void Connect()
        {
            //connecting in other thread
            Thread ct = new Thread(new ThreadStart(ConnectingMethod));
            ct.IsBackground = true;
            ct.Start();
        }
        /// <summary>
        /// Disconnects from server
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (Communication != null) Communication.Close(); else if (OnErrorOccurred != null) OnErrorOccurred(this, "Socket has not been created", Error.SocketNotCreated);
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Error while disconnecting", Error.DisconnectingFailed);
            }
        }

        /// <summary>
        /// Sends data to server
        /// </summary>
        /// <param name="data">Data to sent</param>
        public void Send(byte[] data)
        {
            try
            {
                if (Communication != null) Communication.Send(data); else if (OnErrorOccurred != null) OnErrorOccurred(this, "Socket has not been created", Error.SocketNotCreated);
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Error while sending message", Error.SendingMessageFailed);
            }
        }

        private void ConnectingMethod()
        {
            try
            {
                Communication.Connect(IP, Port);
            }
            catch
            {
                if (OnErrorOccurred != null) if (OnErrorOccurred != null) OnErrorOccurred(this, "Connecting failed", Error.ConnectingFailed);
                return;
            }
            if (OnConnected != null) OnConnected(this, EventArgs.Empty);
            ReceiveThread = new Thread(new ThreadStart(ReceiveMethod));
            ReceiveThread.IsBackground = true;
            ReceiveThread.Start();
        }
        private void ReceiveMethod()
        {
            Communication.ReceiveBufferSize = BufferSize;
            byte[] buffer = new byte[BufferSize];
            int bufflen = 0;
            ClientInfo ci = new ClientInfo();
            ci.ClientResources.Connection = Communication;
            ci.ClientResources.ConnectionThread = ReceiveThread;
            ci.IP = ci.ClientResources.Connection.RemoteEndPoint;

            while (true)
            {
                try
                {
                    bufflen = Communication.Receive(buffer);
                }
                catch//dc
                {
                    break;
                }
                if (bufflen == 0) break;//dc

                //msg recv
                if (OnMessageReceived != null) OnMessageReceived(this, new PacketEventArgs(buffer, bufflen, ci));
            }

            if (OnDisconnected != null) OnDisconnected(this, EventArgs.Empty);
            try
            {
                Communication.Close();
            }
            catch { }
        }
    }
}
