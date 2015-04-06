using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using Woodpecker.Core;
using Woodpecker.Storage;
using Woodpecker.Sessions;
using Woodpecker.Net.Game.Messages;

namespace Woodpecker.Net.Game
{
    /// <summary>
    /// Provides management for connected game clients.
    /// </summary>
    public class gameConnectionManager
    {
        #region Fields
        /// <summary>
        /// An integer which represents the max amount of simultaneous connections an IP address can have to the server.
        /// </summary>
        private int mMaxConnectionsPerIP;
        /// <summary>
        /// The System.Net.Sockets.Socket object that listens for incoming connections.
        /// </summary>
        private Socket mListener;
        #endregion

        #region Methods
        /// <summary>
        /// Attempts to start listening on a certain port. A boolean that indicates if the operation has succeeded is returned.
        /// </summary>
        /// <param name="Port">The TCP port number to listen on.</param>
        /// <param name="maxConnectionsPerIP">The maximum amount of simultaneous connections that an IP address can have to the server.</param>
        public bool startListening(int Port, int maxConnectionsPerIP)
        {
            try
            {
                mListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mListener.Bind(new IPEndPoint(IPAddress.Any, Port));
                mListener.Listen(4);

                mMaxConnectionsPerIP = 999999; // maxConnectionsPerIP;

                mListener.BeginAccept(new AsyncCallback(connectionRequest), mListener);
                Logging.Log("Game connection listener running on port " + Port + ", max connections per IP: " + maxConnectionsPerIP + ".");
                return true;
            }
            catch { return false; }
        }
        /// <summary>
        /// Stops listening and disposes all connections and resources.
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
        /// Invoked asynchronously when a new client requests connection. If the connection is not blacklisted and the max simultaenous connections amount isn't reached yet, then the connection will be processed and normal packet routine will start.
        /// </summary>
        /// <param name="iAr">The IAsyncResult object of the asynchronous BeginAccept operation.</param>
        private void connectionRequest(IAsyncResult iAr)
        {
            try
            {
                Socket Request = mListener.EndAccept(iAr);
                string requestIP = Request.RemoteEndPoint.ToString().Split(':')[0];

                if (this.ipIsBlacklisted(requestIP))
                {
                    Request.Close();
                    Logging.Log("Refused connection request from " + requestIP + ", this IP address is blacklisted for whatever reason.", Logging.logType.connectionBlacklistEvent);
                }
                else
                {
                    if (Engine.Sessions.getSessionCountOfIpAddress(requestIP) >= mMaxConnectionsPerIP)
                    {
                        Request.Close();
                        Logging.Log("Refused connection request from " + requestIP + ", this IP already has " + mMaxConnectionsPerIP + " connections to the server, which is the maximum configured.", Logging.logType.sessionConnectionEvent);
                    }
                    else
                    {
                        Engine.Sessions.createSession(Request);
                    }
                }
            }
            catch (ObjectDisposedException) { } // Nothing special
            catch (NullReferenceException) { } // Nothing special
            catch (Exception ex) { Logging.Log("Unhandled error during game connection request: " + ex.Message, Logging.logType.commonError); }
            finally
            {
                if (mListener != null)
                    mListener.BeginAccept(new AsyncCallback(this.connectionRequest), mListener);
            }
        }

        #region Methods
        /// <summary>
        /// Adds a given IP address to the connection blacklist, thus refusing future connections (both game and MUS) from that IP address.
        /// </summary>
        /// <param name="IP">The IP address to add to the blacklist.</param>
        public void blackListIpAddress(string IP)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("ip", IP);
            Database.Open();
            if (Database.Ready)
            {
                Database.runQuery("INSERT INTO connections_blacklist(ip,added) VALUES (@ip, CURDATE())");
                Logging.Log("Blacklisted IP address '" + IP + "' for whatever reason.", Logging.logType.connectionBlacklistEvent);
            }
            else
                Logging.Log("Failed to add IP address '" + IP + "' to the connection blacklist, the database was not contactable.", Logging.logType.commonError);
        }
        /// <summary>
        /// Returns a boolean that indicates if a given IP address is present in the 'connections_blacklist' table of the database.
        /// </summary>
        /// <param name="IP">The IP address to check.</param>
        public bool ipIsBlacklisted(string IP)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("ip", IP);
            Database.Open();
            if (Database.Ready)
                return Database.findsResult("SELECT ip FROM connections_blacklist WHERE ip = @ip");
            else
                return false;
        }
        /// <summary>
        /// Empties the 'connections_blacklist' table in the database.
        /// </summary>
        public void clearBlacklist()
        {
            Database Database = new Database(true, true);
            if (Database.Ready)
                Database.runQuery("DELETE FROM connections_blacklist");
        }
        #endregion
        #endregion
    }
}
