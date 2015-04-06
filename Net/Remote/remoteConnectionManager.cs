using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using Woodpecker.Core;

namespace Woodpecker.Net.Remote
{
    /// <summary>
    /// Provides various methods for interacting with Woodpecker via a remote connection, eg, for housekeeping purposes.
    /// </summary>
    public partial class remoteConnectionManager
    {
        #region Fields
        /// <summary>
        /// The System.Net.Sockets.Socket object that listens for connections.
        /// </summary>
        private Socket mListener;
        /// <summary>
        /// A List of the type Socket with the Sockets that have connected and haven't send a message yet.
        /// </summary>
        private List<Socket> mClients = new List<Socket>();
        #endregion

        #region Methods
        /// <summary>
        /// Attempts to start listening on a certain port. A boolean that indicates if the operation has succeeded is returned.
        /// </summary>
        /// <param name="Port">The TCP port number to listen on.</param>
        /// <param name="allowExternalHosts">Supply true if the server can accept connections from different hosts than just localhost.</param>
        public bool startListening(int Port, bool allowExternalHosts)
        {
            try
            {
                mListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint EP = null;
                if (allowExternalHosts)
                    EP = new IPEndPoint(IPAddress.Any, Port);
                else
                    EP = new IPEndPoint(IPAddress.Loopback, Port);

                mListener.Bind(EP);
                mListener.Listen(4);

                mListener.BeginAccept(new AsyncCallback(connectionRequest), mListener);
                Logging.Log("Remote connection listener running on port " + Port + ", allow external hosts: " + allowExternalHosts.ToString().ToLower() + ".");
                return true;
            }
            catch { return false; }
        }
        /// <summary>
        /// Stops listening for connections.
        /// </summary>
        public void stopListening()
        {
            try
            {
                mListener.Close();
                mListener = null;
            }
            catch { }
        }
        /// <summary>
        /// Invoked asynchronously when a new client requests connection.
        /// </summary>
        /// <param name="iAr">The IAsyncResult object of the asynchronous BeginAccept operation.</param>
        private void connectionRequest(IAsyncResult iAr)
        {
            try
            {
                Socket Request = mListener.EndAccept(iAr);
                remoteConnection newClient = new remoteConnection(Request);
                newClient.waitForData();
            }
            catch { }
            finally
            {
                if (mListener != null)
                    mListener.BeginAccept(new AsyncCallback(this.connectionRequest), mListener);
            }
        }
        #endregion
    }
}
