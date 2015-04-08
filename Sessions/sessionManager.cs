using System;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

using Woodpecker.Core;
using Woodpecker.Game;
using Woodpecker.Net.Game;
using Woodpecker.Net.Game.Messages;

namespace Woodpecker.Sessions
{
    public class sessionManager
    {
        #region Fields
        /// <summary>
        /// A System.Collections.Dictionary (int, Woodpecker.Sessions.Session) containing all the Woodpecker.Sessions.Session objects, with an integer as key.
        /// </summary>
        public static Dictionary<uint, Session> mSessions = new Dictionary<uint, Session>();
        /// <summary>
        /// The amount of sessions in the session manager.
        /// </summary>
        public int sessionCount
        {
            get { return mSessions.Count; }
        }
        /// <summary>
        /// A System.Threading.Thread that runs on an interval of 60 seconds, dropping dead sessions.
        /// </summary>
        private Thread mSessionPingChecker;
        /// <summary>
        /// A System.Threading.Thread that runs on an interval of 1000ms, checking all active user sessions if they have sent more than 500 bytes in the past second. If positive, then the connection is equal to hax and will be dropped, after blacklisting the IP address.
        /// </summary>
        private Thread mSessionMessageSizeChecker;
        /// <summary>
        /// Incremented every time a session is created.
        /// </summary>
        private uint mSessionCounter = 1;
        #endregion

        #region Methods
        #region Core
        /// <summary>
        /// Clears all sessions, disposing all game- and MUS connections with clients, freeing up all resources, room instances etc.
        /// </summary>
        public void Clear()
        {
            lock (mSessions)
            {
                foreach (Session lSession in mSessions.Values)
                {
                    lSession.Destroy();
                }
            }
            mSessions.Clear();
        }
        /// <summary>
        /// Starts the session ping checker.
        /// </summary>
        public void startPingChecker()
        {
            mSessionPingChecker = new Thread(new ThreadStart(this.checkSessionPings));
            try { mSessionPingChecker.Start(); }
            catch { }

            Logging.Log("Session ping checker started.");
        }
        /// <summary>
        /// Stops the session ping checker.
        /// </summary>
        public void stopPingChecker()
        {
            try { mSessionPingChecker.Abort(); }
            catch { }
            Logging.Log("Session ping checker stopped.");
        }
        /// <summary>
        /// Drops sessions which haven't sent 'PONG' (197, "CD") with their game connection in the past minute. Active sessions respond with "CD" on "@r".
        /// </summary>
        private void checkSessionPings()
        {
            List<uint> deadSessions = new List<uint>();
            while (true)
            {
                lock (mSessions)
                {
                    foreach (Session lSession in mSessions.Values)
                    {
                        if (!lSession.pongReceived) // Pong was not received on time
                        {
                            deadSessions.Add(lSession.ID);
                        }
                        else
                            lSession.pongReceived = false;
                    }
                }

                foreach (uint deadSessionID in deadSessions)
                {
                    this.destroySession(deadSessionID);
                }

                lock (mSessions)
                {
                    foreach (Session lSession in mSessions.Values)
                    {
                        if (lSession.isReady)
                            lSession.gameConnection.sendMessage("@r\x01"); // Ping!
                    }
                }

                deadSessions.Clear();

                // Client has to respond with CD in the next minute, or it will be seen as dead and destroyed at the next cycle of this method
                Thread.Sleep(60000); // Wait one minute and repeat
            }
        }

        /// <summary>
        /// Starts the session message size checker.
        /// </summary>
        public void startReceivedBytesChecker()
        {
            mSessionMessageSizeChecker = new Thread(new ThreadStart(this.checkSessionReceivedBytes));
            mSessionMessageSizeChecker.Priority = ThreadPriority.Lowest;
            try { mSessionMessageSizeChecker.Start(); }
            catch { }

            Logging.Log("Session received bytes checker started.");
        }
        /// <summary>
        /// Stops the session message size checker.
        /// </summary>
        public void stopMessageSizeChecker()
        {
            try { mSessionMessageSizeChecker.Abort(); }
            catch { }
            Logging.Log("Session received bytes checker stopped.");
        }
        private void checkSessionReceivedBytes()
        {
            List<Session> dirtySessions = new List<Session>();
            while (true)
            {
                lock (mSessions)
                {
                    foreach (Session lSession in mSessions.Values)
                    {
                        if (lSession.gameConnection.receivedBytes < 750)
                            lSession.gameConnection.resetReceivedBytesCounter();
                        else
                        {
                            dirtySessions.Add(lSession);
                            Logging.Log("Session " + lSession.ID + " sent " + lSession.gameConnection.receivedBytes + " bytes in one second.", Logging.logType.haxEvent);
                        }
                    }
                }

                foreach (Session lDirtySession in dirtySessions)
                {
                    Engine.Net.Game.blackListIpAddress(lDirtySession.ipAddress);
                    this.destroySession(lDirtySession.ID);
                }

                dirtySessions.Clear();
                Thread.Sleep(1000);
            }
        }
        public void listSessions()
        {
            Engine.Program.GUI.logSacredText(null);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("Listing active sessions...\n");

            int sessionCounter = 0;
            int sessionWithUserCounter = 0;

            Dictionary<uint, Session>.Enumerator lSessions = mSessions.GetEnumerator();
            while (lSessions.MoveNext())
            {
                Session lSession = lSessions.Current.Value;
                sb.Append("Session " + lSession.ID + "\n");
                sb.Append("{\n");
                sb.Append("\x09IP address: " + lSession.ipAddress + ";\n");
                if (lSession.User != null)
                {
                    sb.Append("\n");
                    sb.Append("\x09User ID: " + lSession.User.ID + ";\n");
                    sb.Append("\x09Username: " + lSession.User.Username + ";\n");
                    sb.Append("\x09Role: userRole." + lSession.User.Role.ToString() + ";\n");
                    sessionWithUserCounter++;
                }
                sb.Append("}\n\n");
                sessionCounter++;
            }
            Logging.Log(sb.ToString());
            Logging.Log("Done listing active sessions, total session count: " + sessionCounter + ". Sessions with active user count: " + sessionWithUserCounter);
        }
        #endregion

        #region Session creating & destroying
        /// <summary>
        /// Creates a session for a user, assigning the session ID and adding it to the collection of sessions. Afterwards, the normal game packet routine is started.
        /// </summary>
        /// <param name="gameClient">The System.Net.Sockets.Socket object of the game client that has been accepted by the game connection listener.</param>
        public void createSession(Socket gameClient)
        {
            uint sessionID = mSessionCounter++;
            Session Session = new Session(sessionID, gameClient);
            mSessions.Add(sessionID, Session);
            Logging.Log("Created session " + sessionID + " for " + Session.ipAddress, Logging.logType.sessionConnectionEvent);
            Session.Start();
        }
        /// <summary>
        /// Destroys the session of a given session ID (if active) and disposes all resources.
        /// </summary>
        /// <param name="sessionID">The ID of the session to destroy.</param>
        public void destroySession(uint sessionID)
        {
            if (mSessions.ContainsKey(sessionID))
            {
                Session dSession = mSessions[sessionID];
                dSession.Destroy();
                dSession = null;

                mSessions.Remove(sessionID);
                Logging.Log("Session " + sessionID + " has successfully been destroyed.", Logging.logType.sessionConnectionEvent);
            }
        }
        /// <summary>
        /// Destroys all active sessions for a certain user ID.
        /// </summary>
        /// <param name="userID">The database ID of the user to destroy the active sessions of.</param>
        public void destroySessions(int userID)
        {
            uint sessionID = Engine.Game.Users.getUserSessionID(userID);
            if (sessionID > 0)
                this.destroySession(sessionID);
        }
        #endregion

        #region Actions on single sessions
        /// <summary>
        /// Tries to find a Session object for a given ID, and returns the object if found. Otherwise, a sessionException is thrown.
        /// </summary>
        /// <param name="sessionID">The session ID to get the Session object of.</param>
        /// <exception>sessionException</exception>
        public Session getSession(uint sessionID)
        {
            if (mSessions.ContainsKey(sessionID))
                return mSessions[sessionID];
            else
                throw new sessionException();
        }
        public void addSessionsByIpToList(ref List<Session> List, string ipAddress)
        {
            lock (mSessions)
            {
                foreach (Session lSession in mSessions.Values)
                {
                    if (lSession.ipAddress == ipAddress)
                        List.Add(lSession);
                }
            }
        }
        public void destroySessions(string ipAddress)
        {
            List<Session> toRemove = new List<Session>();
            addSessionsByIpToList(ref toRemove, ipAddress);

            foreach (Session lSession in toRemove)
            {
                this.destroySession(lSession.ID);
            }
        }
        /// <summary>
        /// Sends a game message to a session. Tries to find the session for a given session ID. If the session is found and the session is ready, then the message is sent.
        /// </summary>
        /// <param name="sessionID">The ID of the session to send the message to.</param>
        /// <param name="Message">The Woodpecker.Net.Game.Messages.gameMessage to send.</param>
        public void sendGameMessage(uint sessionID, serverMessage Message)
        {
            if (mSessions.ContainsKey(sessionID))
            {
                Session Session = mSessions[sessionID];
                if (Session.isReady)
                    Session.gameConnection.sendMessage(Message);
            }
        }
        #endregion

        #region Other
        /// <summary>
        /// Returns the amount of sessions that use a given IP address.
        /// </summary>
        /// <param name="IP">The IP address as a string.</param>
        public int getSessionCountOfIpAddress(string IP)
        {
            int Count = 0;
            Dictionary<uint, Session>.Enumerator lSessions = mSessions.GetEnumerator();
            while (lSessions.MoveNext())
            {
                Session lSession = lSessions.Current.Value;
                if (lSession.ipAddress == IP)
                    Count++;
            }

            return Count;
        }
        #endregion
        #endregion
    }
}
