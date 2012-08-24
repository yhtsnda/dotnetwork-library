/*
 * AUTHOR: Utermiko
 * Modified by: <none>
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace DotNetwork.Udp
{
    /// <summary>
    /// Class that represents udp client
    /// </summary>
    public class UdpHandler
    {
        private IPAddress ReceiveIP = IPAddress.Any;
        private int ReceivePort = 8000;
        private int BufferSize = 8192;
        private Socket Communication = null;
        private Thread ReceiveThread = null;

        /// <summary>
        /// Performed when message arrives
        /// </summary>
        public event MessageReceivedHandler OnMessageReceived;
        /// <summary>
        /// Performed when error occurred
        /// </summary>
        public event ErrorHandler OnErrorOccurred;

        /// <summary>
        /// Constructor of UdpHandler
        /// </summary>
        /// <param name="receivePort">Port on which to listen to new messages</param>
        public UdpHandler(int receivePort)
        {
            Constructor(ReceiveIP, receivePort, BufferSize);
        }
        /// <summary>
        /// Constructor of UdpHandler
        /// </summary>
        /// <param name="receivePort">Port on which to listen to new messages</param>
        /// <param name="receiveIP">IP on which to listen to new messages</param>
        public UdpHandler(int receivePort, string receiveIP)
        {
            try
            {
                IPAddress.Parse(receiveIP);
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Bad IP", Error.BadIP);
                return;
            }
            Constructor(IPAddress.Parse(receiveIP), receivePort, BufferSize);
        }
        /// <summary>
        /// Constructor of UdpHandler
        /// </summary>
        /// <param name="receivePort">Port on which to listen to new messages</param>
        /// <param name="receiveIP">IP on which to listen to new messages</param>
        public UdpHandler(int receivePort, IPAddress receiveIP)
        {
            Constructor(receiveIP, receivePort, BufferSize);
        }
        /// <summary>
        /// Constructor of UdpHandler
        /// </summary>
        /// <param name="receivePort">Port on which to listen to new messages</param>
        /// <param name="receiveIP">IP on which to listen to new messages</param>
        /// <param name="bufferSize">Max receive buffer size</param>
        public UdpHandler(int receivePort, string receiveIP, int bufferSize)
        {
            try
            {
                IPAddress.Parse(receiveIP);
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Bad IP", Error.BadIP);
                return;
            }
            Constructor(IPAddress.Parse(receiveIP), receivePort, bufferSize);
        }
        /// <summary>
        /// Constructor of UdpHandler
        /// </summary>
        /// <param name="receivePort">Port on which to listen to new messages</param>
        /// <param name="receiveIP">IP on which to listen to new messages</param>
        /// <param name="bufferSize">Max receive buffer size</param>
        public UdpHandler(int receivePort, IPAddress receiveIP, int bufferSize)
        {
            Constructor(receiveIP, receivePort, bufferSize);
        }
        private void Constructor(IPAddress recvip, int recvport, int buffsize)
        {
            if (recvport < 0 || recvport > 65535)
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Bad port", Error.BadPort);
                return;
            }
            if (buffsize <= 0)
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Bad buffer size", Error.BadBufferSize);
                return;
            }
            ReceiveIP = recvip;
            ReceivePort = recvport;
            BufferSize = buffsize;
            Communication = new Socket(ReceiveIP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                Communication.Bind(new IPEndPoint(ReceiveIP, ReceivePort));
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Probably on this port listens another application", Error.SocketBindError);
                return;
            }
        }

        /// <summary>
        /// Starts receiving for new messages
        /// </summary>
        public void StartReceiving()
        {
            ReceiveThread = new Thread(new ThreadStart(ReceiveMethod));
            ReceiveThread.IsBackground = true;
            ReceiveThread.Start();
        }
        /// <summary>
        /// Stops receiving for new messages
        /// </summary>
        public void StopReceiving()
        {
            if (ReceiveThread != null) ReceiveThread.Abort();
            if (Communication != null) Communication.Close();
        }
        /// <summary>
        /// Stops receiving for new messages with specified timeout
        /// </summary>
        /// <param name="timeout">Timeout to stop receiving</param>
        public void StopReceiving(int timeout)
        {
            if (ReceiveThread != null) ReceiveThread.Abort();
            if (Communication != null) Communication.Close(timeout);
        }

        /// <summary>
        /// Sends message to specified end point
        /// </summary>
        /// <param name="ep">Recipient end point</param>
        /// <param name="data">Data to sent</param>
        public void Send(EndPoint ep, byte[] data)
        {
            try
            {
                Communication.SendTo(data, ep);
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Error while sending message.", Error.SendingMessageFailed);
                return;
            }
        }
        /// <summary>
        /// Sends message to specified ip and port
        /// </summary>
        /// <param name="ip">Recipient IP</param>
        /// <param name="port">Recipient port</param>
        /// <param name="data">Data to sent</param>
        public void Send(string ip, int port, byte[] data)
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
            if (port < 0 || port > 65535) if (OnErrorOccurred != null) OnErrorOccurred(this, "Bad port", Error.BadPort);
            try
            {
                Communication.SendTo(data, new IPEndPoint(IPAddress.Parse(ip), port));
                return;
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Error while sending message.", Error.SendingMessageFailed);
                return;
            }
        }
        /// <summary>
        /// Sends message to specified ip and port
        /// </summary>
        /// <param name="ip">Recipient IP</param>
        /// <param name="port">Recipient port</param>
        /// <param name="data">Data to sent</param>
        public void Send(IPAddress ip, int port, byte[] data)
        {
            try
            {
                Communication.SendTo(data, new IPEndPoint(ip, port));
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Error while sending message", Error.SendingMessageFailed);
                return;
            }
        }
        /// <summary>
        /// Sends message to specified recipients
        /// </summary>
        /// <param name="ipsOfRecipients">IPs of recipients</param>
        /// <param name="recipientPort">Port to send message</param>
        /// <param name="data">Data to sent</param>
        public void Send(string[] ipsOfRecipients, int recipientPort, byte[] data)
        {
            foreach (string ip in ipsOfRecipients)
            {
                try
                {
                    Communication.SendTo(data, new IPEndPoint(IPAddress.Parse(ip), recipientPort));
                }
                catch { }
            }
        }
        /// <summary>
        /// Sends message to specified recipients
        /// </summary>
        /// <param name="ipsOfRecipients">IPs of recipients</param>
        /// <param name="recipientPort">Port to send message</param>
        /// <param name="data">Data to sent</param>
        public void Send(IPAddress[] ipsOfRecipients, int recipientPort, byte[] data)
        {
            foreach (IPAddress ip in ipsOfRecipients)
            {
                try
                {
                    Communication.SendTo(data, new IPEndPoint(ip, recipientPort));
                }
                catch { }
            }
        }
        /// <summary>
        /// Sends message to specified recipients
        /// </summary>
        /// <param name="recipients">Array of recipients</param>
        /// <param name="data">Data to sent</param>
        public void Send(IPEndPoint[] recipients, byte[] data)
        {
            foreach (IPEndPoint ep in recipients)
            {
                try
                {
                    Communication.SendTo(data, ep);
                }
                catch { }
            }
        }

        private void ReceiveMethod()
        {
            Communication.ReceiveBufferSize = BufferSize;
            byte[] buffer = new byte[BufferSize];
            int bufferlen = 0;
            EndPoint senderep = new IPEndPoint(ReceiveIP, ReceivePort);
            while (true)
            {
                try
                {
                    bufferlen = Communication.ReceiveFrom(buffer, ref senderep);
                }
                catch
                {
                    if (OnErrorOccurred != null) OnErrorOccurred(this, "Error while receiving", Error.ReceivingFailed);
                    return;
                }
                ClientInfo ci = new ClientInfo();
                ci.IP = senderep;
                ci.ClientResources.Connection = Communication;
                if (OnMessageReceived != null) OnMessageReceived(this, new PacketEventArgs(buffer, bufferlen, ci));
            }
        }
    }
}