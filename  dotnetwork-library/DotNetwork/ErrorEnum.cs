using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetwork
{
    /// <summary>
    /// Enum that repreents errors
    /// </summary>
    public enum Error
    {
        /// <summary>
        /// Undefinied error
        /// </summary>
        Undefinied,
        /// <summary>
        /// Bad IP
        /// </summary>
        BadIP,
        /// <summary>
        /// Bad port
        /// </summary>
        BadPort,
        /// <summary>
        /// Bad buffer size
        /// </summary>
        BadBufferSize,
        /// <summary>
        /// Error while connecting
        /// </summary>
        ConnectingFailed,
        /// <summary>
        /// Socket is not created
        /// </summary>
        SocketNotCreated,
        /// <summary>
        /// Error while disconnecting
        /// </summary>
        DisconnectingFailed,
        /// <summary>
        /// Error while sending message
        /// </summary>
        SendingMessageFailed,
        /// <summary>
        /// Error while listening
        /// </summary>
        ListeningFailed,
        /// <summary>
        /// Error while kicking client
        /// </summary>
        KickFailed,
        /// <summary>
        /// Error while receiving failed
        /// </summary>
        ReceivingFailed,
        /// <summary>
        /// Error while socket binding
        /// </summary>
        SocketBindError,
        /// <summary>
        /// The remote name could not be resolved
        /// </summary>
        RemoteNotResolved,
        /// <summary>
        /// Request was not sent to remote
        /// </summary>
        RequestNotSent,
        /// <summary>
        /// Null object
        /// </summary>
        NullError,
        /// <summary>
        /// Bad parametr
        /// </summary>
        BadParametr,
        /// <summary>
        /// Bad URI
        /// </summary>
        BadUri,
    }
}
