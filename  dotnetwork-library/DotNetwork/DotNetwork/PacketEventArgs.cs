/*
 * AUTHOR: Utermiko
 * Modified by: <none>
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetwork
{
    /// <summary>
    /// Class that represents arrived packet
    /// </summary>
    public class PacketEventArgs : EventArgs
    {
        /// <summary>
        /// Message content
        /// </summary>
        public byte[] Data;
        /// <summary>
        /// Length of message
        /// </summary>
        public int DataLength;
        /// <summary>
        /// Sender of message
        /// </summary>
        public ClientInfo Sender;

        /// <summary>
        /// Constructor of PacketEventArgs
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="dataLength">Length of data</param>
        /// <param name="sender">Sender of packet</param>
        public PacketEventArgs(byte[] data, int dataLength, ClientInfo sender)
        {
            Data = data;
            DataLength = dataLength;
            Sender = sender;
        }
    }
}
