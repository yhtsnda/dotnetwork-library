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
    /// Delegate
    /// </summary>
    /// <param name="sender">Sender of event</param>
    /// <param name="e">Event arguments - information about packet</param>
    public delegate void MessageReceivedHandler(object sender, PacketEventArgs e);
    /// <summary>
    /// Delegate
    /// </summary>
    /// <param name="sender">Sender of event</param>
    /// <param name="client">Event arguments - client</param>
    public delegate void ClientActionHandler(object sender, ClientInfo client);
    /// <summary>
    /// Delegate
    /// </summary>
    /// <param name="sender">Sender of event</param>
    /// <param name="message">Error message</param>
    /// <param name="errortype">Type of error</param>
    public delegate void ErrorHandler(object sender, string message, Error errortype);
}
