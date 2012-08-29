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

namespace DotNetwork
{
    /// <summary>
    /// Class that represents client
    /// </summary>
    public class ClientInfo
    {
        /// <summary>
        /// Structure to store technical data about client.
        /// </summary>
        public struct ClientResourcesStruct
        {
            internal Socket Connection;
            internal Thread ConnectionThread;
        }
        /// <summary>
        /// Connection information. Content isn't visible outside.
        /// </summary>
        public ClientResourcesStruct ClientResources;
        /// <summary>
        /// Client IP
        /// </summary>
        public EndPoint IP { get; internal set; }
        /// <summary>
        /// Constructor of ClientInfo
        /// </summary>
        public ClientInfo()
        {
            ClientResources = new ClientResourcesStruct();
        }
    }
}
