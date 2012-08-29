/*
 * AUTHOR: Utermiko
 * Modified by: <none>
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace DotNetwork.Http
{
    /// <summary>
    /// Class that represents Http Request
    /// </summary>
    public class HttpRequestHandler : IDisposable
    {
        private WebRequest Request;
        private WebResponse Response;
        private string Agent = "DotNetwork Agent";
        private List<string> GetParametrs;
        private List<string> PostParametrs;
        /// <summary>
        /// The URI that indentifies the Internet resource
        /// </summary>
        public Uri UriToRequest { get; private set; }

        /// <summary>
        /// Performed when error occurred
        /// </summary>
        public event ErrorHandler OnErrorOccurred;

        /// <summary>
        /// Constructor of HttpRequestHandler
        /// </summary>
        /// <param name="uri">The URI that indentifies the Internet resource</param>
        public HttpRequestHandler(string uri)
        {
            try
            {
                new Uri(uri);
            }
            catch
            {
                if (uri.StartsWith("http://") || uri.StartsWith("https://"))
                {
                    if (OnErrorOccurred != null) OnErrorOccurred(this, "Bad URI", Error.BadUri);
                }
                else
                {
                    uri = uri.Insert(0, "http://");
                }
            }
            Constructor(new Uri(uri), Agent);
        }
        /// <summary>
        /// Constructor of HttpRequestHandler
        /// </summary>
        /// <param name="uri">The URI that indentifies the Internet resource</param>
        /// <param name="userAgent">User agent. Default is "DotNetwork Agent"</param>
        public HttpRequestHandler(string uri, string userAgent)
        {
            try
            {
                new Uri(uri);
            }
            catch
            {
                if (uri.StartsWith("http://") || uri.StartsWith("https://"))
                {
                    if (OnErrorOccurred != null) OnErrorOccurred(this, "Bad URI", Error.BadUri);
                }
                else
                {
                    uri = uri.Insert(0, "http://");
                }
            }
            Constructor(new Uri(uri), userAgent);
        }
        /// <summary>
        /// Constructor of HttpRequestHandler
        /// </summary>
        /// <param name="uri">The URI that indentifies the Internet resource</param>
        public HttpRequestHandler(Uri uri)
        {
            Constructor(uri, Agent);
        }
        /// <summary>
        /// Constructor of HttpRequestHandler
        /// </summary>
        /// <param name="uri">The URI that indentifies the Internet resource</param>
        /// <param name="userAgent">User agent. Default is "DotNetwork Agent"</param>
        public HttpRequestHandler(Uri uri, string userAgent)
        {
            Constructor(uri, userAgent);
        }
        /// <summary>
        /// Constructor of HttpRequestHandler
        /// </summary>
        /// <param name="hostName">Host name</param>
        /// <param name="path">Path to file</param>
        /// <param name="port">Port</param>
        /// <param name="userName">User name</param>
        /// <param name="password">Password</param>
        public HttpRequestHandler(string hostName, string path, int port, string userName, string password)
        {
            UriBuilder ub = new UriBuilder();
            try
            {
                ub.Host = hostName;
                ub.UserName = userName;
                ub.Password = password;
                ub.Path = path;
                ub.Port = port;
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "One of parametr is uncorrect", Error.BadParametr);
            }
            Constructor(ub.Uri, Agent);
        }
        /// <summary>
        /// Constructor of HttpRequestHandler
        /// </summary>
        /// <param name="hostName">Host name</param>
        /// <param name="path">Path to file</param>
        /// <param name="port">Port</param>
        /// <param name="userName">User name</param>
        /// <param name="password">Password</param>
        /// <param name="userAgent">User agent. Default is "DotNetwork Agent"</param>
        public HttpRequestHandler(string hostName, string path, int port, string userName, string password, string userAgent)
        {
            UriBuilder ub = new UriBuilder();
            try
            {
                ub.Host = hostName;
                ub.UserName = userName;
                ub.Password = password;
                ub.Path = path;
                ub.Port = port;
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "One of parametr is uncorrect", Error.BadParametr);
            }
            Constructor(ub.Uri, userAgent);
        }
        private void Constructor(Uri uri, string ua)
        {
            UriToRequest = uri;
            Agent = ua;
            GetParametrs = new List<string>();
            PostParametrs = new List<string>();
        }

        /// <summary>
        /// Adds GET parametr
        /// </summary>
        /// <param name="name">Name of parametr</param>
        /// <param name="value">Value of parametr</param>
        public void AddGetParametr(string name, string value)
        {
            GetParametrs.Add(name + '=' + value);
        }
        /// <summary>
        /// Adds POST parametr
        /// </summary>
        /// <param name="name">Name of parametr</param>
        /// <param name="value">Value of parametr</param>
        public void AddPostParametr(string name, string value)
        {
            PostParametrs.Add(name + '=' + value);
        }

        /// <summary>
        /// Sends request
        /// </summary>
        public void SendRequest()
        {
            Send();
        }
        /// <summary>
        /// Gets response
        /// </summary>
        /// <returns>Stream of response data</returns>
        public Stream GetResponse()
        {
            return Get();
        }
        /// <summary>
        /// Sends request and after that get response
        /// </summary>
        /// <returns>Stream of response data</returns>
        public Stream SendRequestAndGetResponse()
        {
            Send();
            return Get();
        }

        /// <summary>
        /// Reads data from stream
        /// </summary>
        /// <param name="stream">Stream from which to read data</param>
        /// <returns>Read string</returns>
        public string ReadFromStream(Stream stream)
        {
            string resp = "";
            try
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    resp = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Stream is null", Error.NullError);
            }
            return resp;
        }

        private void Send()
        {
            string getpar = "?";
            foreach (string parametr in GetParametrs)
            {
                getpar += parametr + '&';
            }
            if (getpar.Length > 0) getpar = getpar.Remove(getpar.Length - 1);
            string postpar = "";
            foreach (string parametr in PostParametrs)
            {
                postpar += parametr + '&';
            }
            if (postpar.Length > 0) postpar = postpar.Remove(postpar.Length - 1);
            byte[] postparb = Encoding.ASCII.GetBytes(postpar);
            try
            {
                Request = WebRequest.Create(UriToRequest + getpar);
                Request.Credentials = CredentialCache.DefaultCredentials;
                Request.ContentType = "application/x-www-form-urlencoded";
                ((HttpWebRequest)Request).UserAgent = Agent;
                Request.Method = "POST";
                Request.ContentLength = postparb.Length;
                using (Stream writer = Request.GetRequestStream())
                {
                    writer.Write(postparb, 0, postparb.Length);
                    writer.Close();
                }
                Response = Request.GetResponse();
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Error while sending request. Probably the remote name could not be resolved", Error.RemoteNotResolved);
            }
        }
        private Stream Get()
        {
            Stream respstream = null;
            try
            {
                respstream = Response.GetResponseStream();
            }
            catch
            {
                if (OnErrorOccurred != null) OnErrorOccurred(this, "Request was not sent", Error.RequestNotSent);
            }
            return respstream;
        }

        /// <summary>
        /// Disposes this object
        /// </summary>
        public void Dispose()
        {
            Request = null;
            Response = null;
            UriToRequest = null;
            Agent = null;
            GetParametrs = null;
            PostParametrs = null;
            GC.SuppressFinalize(this);
        }
    }
}
