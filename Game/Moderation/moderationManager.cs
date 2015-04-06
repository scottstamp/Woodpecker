using System;
using System.Data;
using System.Threading;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

using Woodpecker.Core;
using Woodpecker.Storage;
using Woodpecker.Sessions;

using Woodpecker.Net.Game.Messages;

using Woodpecker.Game.Users;
using Woodpecker.Game.Rooms.Instances;

namespace Woodpecker.Game.Moderation
{
    /// <summary>
    /// Provides all kinds of functions for moderation in the virtual hotel, eg, bans etc.
    /// </summary>
    public class moderationManager
    {
        #region Fields
        private List<chatLog> mChatLogStack;
        private Thread mChatLogWorker;
        /// <summary>
        /// True if the logging of in-game chat should be enabled.
        /// </summary>
        private bool mLogChat;
        public bool logChat
        {
            get { return mLogChat; }
            set
            {
                mLogChat = value;
                if (mLogChat)
                {
                    mChatLogStack = new List<chatLog>();
                    mChatLogWorker = new Thread(new ThreadStart(this.processChatLogStack));
                    mChatLogWorker.Priority = ThreadPriority.BelowNormal;
                    mChatLogWorker.Start();

                    Logging.Log("Logging of in-game chat enabled.");
                }
                else
                {
                    if (mChatLogWorker != null)
                    {
                        mChatLogWorker.Abort();
                        mChatLogStack.Clear();
                        mChatLogWorker = null;
                        mChatLogStack = null;
                    }
                    Logging.Log("Logging of in-game chat disabled.");
                }
            }
        }
        #endregion

        #region Methods
        #region Bans
        /// <summary>
        /// Returns true if the given user is (still) banned. If so, then the ban reason is passed through the out Reason string.
        /// </summary>
        /// <param name="userID">The database ID of the user to check.</param>
        /// <param name="Reason">A string that should be passed with the out parameter and which will contain the banreason if banned.</param>
        public bool isBanned(int userID, out string Reason)
        {
            Reason = "";
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.Open();
            if (Database.Ready)
            {
                Reason = Database.getString("SELECT reason FROM moderation_bans WHERE userid = @userid AND expires > NOW() LIMIT 1");
            }

            return (Reason.Length > 0);
        }
        public bool isBanned(string ipAddress, string machineID, out string Reason)
        {
            Reason = "";
            Database Database = new Database(false, true);
            Database.addParameterWithValue("ip", ipAddress);
            Database.addParameterWithValue("machineid", machineID);
            Database.Open();
            if (Database.Ready)
            {
                Reason = Database.getString("SELECT reason FROM moderation_bans WHERE ip = @ip OR machineid = @machineid AND expires > NOW() LIMIT 1");
            }

            return (Reason.Length > 0);
        }
        /// <summary>
        /// Removes all expired bans from the database.
        /// </summary>
        public void dropExpiredBans()
        {
            Database Database = new Database(true, true);
            if (Database.Ready)
                Database.runQuery("DELETE FROM moderation_bans WHERE expires > NOW()");
        }
        #endregion

        #region Chatlog
        /// <summary>
        /// Adds a chat message to the chat log stack, so it will be written in the database on the next cycle.
        /// </summary>
        /// <param name="sessionID">The ID of the session that chatted this message.</param>
        /// <param name="userID">The database ID of the user that chatted this message.</param>
        /// <param name="roomID">The database ID of the room where this message was chatted in.</param>
        /// <param name="receiverID">If the type of this chat message was a whisper, the database ID of the receiving user should be supplied here.</param>
        /// <param name="Type">A value of the 'chatType' enum, representing the type of this chat message.</param>
        /// <param name="Text">The actual chat message. Pass this argument with the 'ref' keyword please.</param>
        public void addChatMessageToLogStack(uint sessionID, int userID, int roomID, int receiverID, chatType Type, ref string Text)
        {
            if (mLogChat)
            {
                chatLog Log = new chatLog(sessionID, userID, roomID, receiverID, Type, Text);
                mChatLogStack.Add(Log);
            }
        }
        /// <summary>
        /// Processes the current chatlog stack and writes it into the 'moderation_chatlog' table of the database.
        /// </summary>
        private void processChatLogStack()
        {
            while (true)
            {
                Thread.Sleep(15000); // Every 15 seconds
                if (mChatLogStack.Count > 0) // Log chatlogs at min 10 logs @ stack
                {
                    MySqlParameter dtParameter = new MySqlParameter("moment", MySqlDbType.DateTime);
                    MySqlParameter txtParameter = new MySqlParameter("text", MySqlDbType.Text);
                    Database dbClient = new Database(false, false);
                    dbClient.addRawParameter(dtParameter);
                    dbClient.addRawParameter(txtParameter);

                    dbClient.Open();
                    if (dbClient.Ready)
                    {
                        lock (mChatLogStack)
                        {
                            try
                            {
                                foreach (chatLog Log in mChatLogStack)
                                {
                                    dtParameter.Value = Log.Timestamp;
                                    txtParameter.Value = Log.Text;
                                    dbClient.runQuery(Log.ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                // Failed!
                                Logging.Log("Error while inserting chatlogs: " + ex.ToString(), Logging.logType.commonError);
                            }
                        }
                        dbClient.Close();
                    }
                    mChatLogStack.Clear();
                }
            }
        }
        #endregion

        #region Moderating methods
        public bool requestAlert(int userID, string targetUsername, string Message, string extraInfo)
        {
            int targetUserID = Engine.Game.Users.getUserID(targetUsername);
            if (targetUserID > 0)
                return requestAlert(userID, targetUserID, Message, extraInfo);
            else
                return false;
        }
        public bool requestAlert(int userID, int targetUserID, string Message, string extraInfo)
        {
            Session targetSession = Engine.Game.Users.getUserSession(targetUserID);
            if (targetSession != null)
            {
                targetSession.gameConnection.sendLocalizedError("mod_warn/" + Message);
                logModerationAction(userID, "alert", targetUserID, Message, extraInfo);

                return true;
            }

            return false;
        }

        public bool requestKickFromRoom(int userID, string targetUsername, string Message, string extraInfo)
        {
            int targetUserID = Engine.Game.Users.getUserID(targetUsername);
            if (targetUserID > 0)
                return requestKickFromRoom(userID, targetUserID, Message, extraInfo);
            else
                return false;
        }
        public bool requestKickFromRoom(int userID, int targetUserID, string Message, string extraInfo)
        {
            userInformation Caster = Engine.Game.Users.getUserInfo(userID, false);
            if (Caster != null)
            {
                Session targetSession = Engine.Game.Users.getUserSession(targetUserID);
                if (targetSession != null && targetSession.inRoom)
                {
                    if (Caster.Role > targetSession.User.Role)
                    {
                        targetSession.kickFromRoom(Message);
                        logModerationAction(Caster.ID, "kick", targetUserID, Message, extraInfo);
                        
                        return true;
                    }
                }
            }

            return false;
        }

        public bool requestRoomAlert(int userID, int roomID, string Message, string extraInfo)
        {
            if (Engine.Game.Rooms.roomInstanceRunning(roomID))
            {
                serverMessage Alert = new serverMessage(33); // "@a"
                Alert.Append("mod_warn/");
                Alert.Append(Message);

                Engine.Game.Rooms.getRoomInstance(roomID).sendMessage(Alert);

                logModerationAction(userID, "roomalert", roomID, Message, extraInfo);

                return true;
            }

            return false;
        }

        public bool requestRoomKick(int userID, int roomID, string Message, string extraInfo)
        {
            userInformation Caster = Engine.Game.Users.getUserInfo(userID, false);
            if (Caster != null)
            {
                if (Engine.Game.Rooms.roomInstanceRunning(roomID))
                {
                    Engine.Game.Rooms.getRoomInstance(roomID).castRoomKick(Caster.Role, Message);
                    logModerationAction(userID, "roomkick", roomID, Message, extraInfo);

                    return true;
                }
            }

            return false;
        }

        public bool requestBan(int userID, string targetUsername, int banHours, bool banIP, bool banMachine, string Message, string extraInfo)
        {
            int targetUserID = Engine.Game.Users.getUserID(targetUsername);
            if (targetUserID > 0)
                return requestBan(userID, targetUserID, banHours, banIP, banMachine, Message, extraInfo);
            else
                return false;
        }
        public bool requestBan(int userID, int targetUserID, int banHours, bool banIP, bool banMachine, string Message, string extraInfo)
        {
            userInformation Caster = Engine.Game.Users.getUserInfo(userID, false);
            userInformation Target = Engine.Game.Users.getUserInfo(targetUserID, false);
            if (Caster != null && Target != null)
            {
                if (Caster.Role > Target.Role) // Able to ban this user
                {
                    if (banHours >= 2
                        && banHours <= 100000
                        && !(banHours >= 168 && !Caster.hasFuseRight("fuse_superban"))) // Ban condition valid
                    {
                        userAccessInformation lastAccess = null;
                        Session targetSession = Engine.Game.Users.getUserSession(targetUserID);

                        #region Determine last access
                        if (targetSession == null)
                            lastAccess = Engine.Game.Users.getLastAccess(targetUserID);
                        else
                            lastAccess = targetSession.Access;

                        if ((banIP || banMachine) && lastAccess == null)
                            return false; // Can't ban IP and/or machine with no access log
                        #endregion

                        if (this.requestUnban(userID, targetUserID, lastAccess.IP, lastAccess.machineID)) // Removed old bans succeeded
                        {
                            #region Write ban entry
                            Database dbClient = new Database(false, false);
                            dbClient.addParameterWithValue("userid", targetUserID);
                            dbClient.addParameterWithValue("appliedby", userID);
                            dbClient.addParameterWithValue("expires", DateTime.Now.AddHours(banHours));
                            dbClient.addParameterWithValue("reason", Message);

                            // Handle access parameters
                            if (banIP)
                                dbClient.addParameterWithValue("ip", lastAccess.IP);
                            else
                                dbClient.addParameterWithValue("ip", DBNull.Value);
                            if (banMachine)
                                dbClient.addParameterWithValue("machineid", lastAccess.machineID);
                            else
                                dbClient.addParameterWithValue("machineid", DBNull.Value);

                            dbClient.Open();
                            if (!dbClient.Ready)
                                return false; // Failed to write entry

                            dbClient.runQuery("INSERT INTO moderation_bans VALUES (@userid,@ip,@machineid,NOW(),@appliedby,@expires,@reason)");
                            dbClient.Close();
                            dbClient = null;
                            #endregion

                            #region Notify & disconnect affected users
                            List<Session> toNotify = new List<Session>();
                            if (targetSession != null)
                                toNotify.Add(targetSession);

                            if (banIP)
                                Engine.Sessions.addSessionsByIpToList(ref toNotify, lastAccess.IP);

                            serverMessage banCast = genericMessageFactory.createBanCast(Message);
                            foreach (Session lSession in toNotify)
                            {
                                lSession.isValid = false;
                                lSession.leaveRoom(true);
                                lSession.gameConnection.sendMessage(banCast);
                            }
                            #endregion

                            this.logModerationAction(userID, "ban", targetUserID, Message, extraInfo);

                            return true;
                        }
                    }
                }
            }

            return false;
        }


        public bool requestUnban(int userID, string targetUsername, string IP, string machineID)
        {
            int targetUserID = Engine.Game.Users.getUserID(targetUsername);
            return requestUnban(userID, targetUserID, IP, machineID);
        }
        public bool requestUnban(int userID, int targetUserID, string IP, string machineID)
        {
            Database dbClient = new Database(false, false);
            dbClient.addParameterWithValue("userid", userID);
            dbClient.addParameterWithValue("targetid", targetUserID);
            dbClient.addParameterWithValue("ip", IP);
            dbClient.addParameterWithValue("machineid", machineID);
            dbClient.Open();
            if (dbClient.Ready)
            {
                if (targetUserID > 0)
                    dbClient.runQuery("DELETE FROM moderation_bans WHERE userid = @userid");

                if (IP != null && machineID != null)
                    dbClient.runQuery("DELETE FROM moderation_bans WHERE ip = @ip OR machineid = @machineid");

                return true;
            }

            return false;
        }

        public void castHotelAlert(int userID, string Text)
        {
            Engine.Game.Users.broadcastHotelAlert(Text);
            this.logModerationAction(userID, "hotelalert", -1, Text, "");
        }
        public void logModerationAction(int userID, string actionType, int targetID, string Message, string extraInfo)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.addParameterWithValue("targetid", targetID);
            Database.addParameterWithValue("actiontype", actionType);
            Database.addParameterWithValue("message", Message);
            Database.addParameterWithValue("extra", extraInfo);

            Database.Open();
            if (Database.Ready)
                Database.runQuery("INSERT INTO moderation_log VALUES (NOW(),@userid,@targetid,@actiontype,@message,@extra)");

            Logging.Log("Staff member (user " + userID + ") performed action '" + actionType + "', target user/room was " + targetID + ".", Logging.logType.moderationEvent);
        }
        #endregion
        #endregion
    }
}