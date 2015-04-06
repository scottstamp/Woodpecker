using System;
using System.Data;
using System.Collections.Generic;
using System.Collections;

using Woodpecker.Core;
using Woodpecker.Storage;
using Woodpecker.Specialized.Text;

using Woodpecker.Sessions;
using Woodpecker.Net.Game.Messages;
using Woodpecker.Game.Users.Roles;

namespace Woodpecker.Game.Users
{
    /// <summary>
    /// Provides management for logged in users. This class works together with the sessionManager.
    /// </summary>
    public class userManager
    {
        #region Fields
        /// <summary>
        /// A private Dictionary object with an integer as key (user ID) and a Session object as value.
        /// </summary>
        private Dictionary<int, Session> mUserSessions = new Dictionary<int, Session>();
        /// <summary>
        /// The current amount of active users on the server.
        /// </summary>
        public int userCount
        {
            get { return mUserSessions.Count; }
        }
        #endregion

        #region Methods
        #region Manager
        /// <summary>
        /// Clears the user collection of the manager.
        /// </summary>
        public void Clear()
        {
            mUserSessions.Clear();
        }
        /// <summary>
        /// Lists various statistics about the registered users in the textbox on the GUI.
        /// </summary>
        public void listUserStats()
        {
            Database db = new Database(false, false);
            DataTable dTable = null;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int Counter = 0;

            Logging.Log("Listing user statistics...\n");

            db.Open();

            sb.Append("Role user amounts:\n");
            for (int roleID = 0; roleID <= 6; roleID++)
            {
                int Count = db.getInteger("SELECT COUNT(id) FROM users WHERE role = '" + roleID + "'");
                sb.Append("Role '" + ((userRole)roleID).ToString() + "' has " + Count + " users.\n");
            }

            sb.Append("\n> Top 20 of richest normal users:\n");
            dTable = db.getTable("SELECT id,username,credits FROM users WHERE role <= '3' ORDER BY credits DESC LIMIT 20");
            foreach (DataRow dRow in dTable.Rows)
            {
                Counter++;
                int userID = (int)dRow["id"];
                string Username = (string)dRow["username"];
                int Credits = (int)dRow["credits"];
                sb.Append(Counter + ") " + Username + " [id: " + userID + "] has " + Credits + " credits.\n");
            }
            Logging.Log(sb.ToString());
            Logging.Log("User statistics listed.\n");

            db.Close();
        }
        #endregion

        #region User sessions
        /// <summary>
        /// Returns a boolean indicating if a value for a certain user ID (key) is found in the user collecting, thus meaning the user is logged in.
        /// </summary>
        /// <param name="userID">The database ID of the user to get the login status for.</param>
        public bool userIsLoggedIn(int userID)
        {
            return mUserSessions.ContainsKey(userID);
        }
        /// <summary>
        /// If the given user is currently logged in, the Woodpecker.Sessions.Session object of this user is returned. Otherwise, null is returned.
        /// </summary>
        /// <param name="userID">The database ID of the user to get the session of.</param>
        public Session getUserSession(int userID)
        {
            if (mUserSessions.ContainsKey(userID))
                return mUserSessions[userID];
            else
                return null;
        }

        public ArrayList UserFuses(string fuse)
        {
            ArrayList list = new ArrayList();
            lock (mUserSessions)
            {
                foreach (Session lSession in mUserSessions.Values)
                {
                    if (lSession.User != null && lSession.User.hasFuseRight(fuse))
                    {
                        list.Add(lSession.User.ID);
                    }
                }
            }
            return list;
        }

        public Session getUserSession(string Username)
        {
            Session ret = null;
            Username = Username.ToLower();
            lock (mUserSessions)
            {
                foreach (Session lSession in mUserSessions.Values)
                {
                    if (lSession.User != null && lSession.User.Username.ToLower() == Username)
                    {
                        ret = lSession;
                        break;
                    }
                }
            }

            return ret;
        }
        /// <summary>
        /// Returns the session ID of a user as an unsigned integer. If the user is not logged in, then 0 is returned.
        /// </summary>
        /// <param name="userID">The database ID of the user to get the session ID of.</param>
        public uint getUserSessionID(int userID)
        {
            if (mUserSessions.ContainsKey(userID))
                return mUserSessions[userID].ID;
            else
                return 0;
        }

        /// <summary>
        /// Adds a Woodpecker.Sessions.Session object to the collection of user sessions, with the user ID as key.
        /// </summary>
        /// <param name="userSession">The Woodpecker.Sessions.Session to add.</param>
        public void addUserSession(Session userSession)
        {
            if (!mUserSessions.ContainsKey(userSession.User.ID))
            {
                mUserSessions.Add(userSession.User.ID, userSession);
                Configuration.setConfigurationValue("users.online", mUserSessions.Count.ToString());
            }
        }
        /// <summary>
        /// Removes a user session from the user sessions collection, when the user ID is given.
        /// </summary>
        /// <param name="userID">The database ID of the user to remove the session of.</param>
        public void removeUserSession(int userID)
        {
            mUserSessions.Remove(userID);
            Configuration.setConfigurationValue("users.online", mUserSessions.Count.ToString());
        }
        /// <summary>
        /// Tries to send a game message (serverMessage object) to a user, catching & dumping any exceptions that occur.
        /// </summary>
        /// <param name="userID">The database ID of the user to send the message to.</param>
        /// <param name="Message">The message to send to the user as serverMessage object.</param>
        public void trySendGameMessage(int userID, serverMessage Message)
        {
            try
            {
                mUserSessions[userID].gameConnection.sendMessage(Message);
            }
            catch { }
        }
        /// <summary>
        /// Tries to send a game message (string, don't forget that char1 is used to break messages) to a user, after checking if the user is logged in.
        /// </summary>
        /// <param name="userID">The database ID of the user to send the message to.</param>
        /// <param name="Message">The message to send to the user as a string.</param>
        public void trySendGameMessage(int userID, string Message)
        {
            try
            {
                mUserSessions[userID].gameConnection.sendMessage(Message);
            }
            catch { }
        }
        #endregion

        #region User info
        /// <summary>
        /// Returns a boolean that indicates if a user with a certain username exists in the 'users' table of the database.
        /// </summary>
        /// <param name="Username">The username to check.</param>
        public bool userExists(string Username)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("username", Username);
            Database.Open();
            return Database.findsResult("SELECT id FROM users WHERE username = @username");
        }
        /// <summary>
        /// Retrieves the database ID of a user, when only the username is known.
        /// </summary>
        /// <param name="Username">The username of the user to get the user ID of.</param>
        public int getUserID(string Username)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("username", Username);
            Database.Open();

            return Database.getInteger("SELECT id FROM users WHERE username = @username");
        }
        /// <summary>
        /// Retrieves the username of a user, when only the user ID is known.
        /// </summary>
        /// <param name="userID">The user ID of the user to get the username of.</param>
        public string getUsername(int userID)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.Open();

            return Database.getString("SELECT username FROM users WHERE id = @userid");
        }

        public basicUserInformation getBasicUserInfo(int userID)
        {
            basicUserInformation returnInfo = new basicUserInformation();
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.Open();
            if (Database.Ready)
            {
                try
                {
                    DataRow dRow = Database.getRow("SELECT username,figure,sex,motto,motto_messenger,lastactivity FROM users WHERE id = @userid");
                    returnInfo.ID = userID;
                    returnInfo.Username = (string)dRow["username"];
                    returnInfo.Figure = (string)dRow["figure"];
                    returnInfo.Sex = Convert.ToChar(dRow["sex"].ToString());
                    returnInfo.Motto = (string)dRow["motto"];
                    returnInfo.messengerMotto = (string)dRow["motto_messenger"];
                    returnInfo.lastActivity = (DateTime)dRow["lastactivity"];
                }
                catch { returnInfo = new basicUserInformation(); }
            }

            return returnInfo;
        }

        public basicUserInformation getBasicUserInfo(string Username)
        {
            int userID = this.getUserID(Username);
            if (userID > 0)
                return this.getBasicUserInfo(userID);
            else
                return new basicUserInformation();
        }
        public userInformation getUserInfoByTicket(string ssoTicket)
        {
            userInformation returnInfo = new userInformation();
            Database Database = new Database(false, true);
            Database.addParameterWithValue("ticket", ssoTicket);
            Database.Open();

            if (Database.Ready)
            {
                try
                {
                    DataRow dRow = Database.getRow("SELECT * FROM users WHERE ticket = @ticket");
                    returnInfo.ID = (int)dRow["id"];
                    returnInfo.Username = (string)dRow["username"];
                    returnInfo.Password = (string)dRow["password"];
                    returnInfo.Role = (userRole)(int.Parse(dRow["role"].ToString()));
                    returnInfo.Figure = (string)dRow["figure"];
                    returnInfo.Sex = Convert.ToChar(dRow["sex"].ToString());
                    returnInfo.Motto = (string)dRow["motto"];
                    returnInfo.messengerMotto = (string)dRow["motto_messenger"];
                    returnInfo.Credits = (int)dRow["credits"];
                    returnInfo.Tickets = (int)dRow["tickets"];
                    returnInfo.Film = (int)dRow["film"];
                    returnInfo.Badge = (string)dRow["currentbadge"];
                    returnInfo.lastActivity = (DateTime)dRow["lastactivity"];
                    returnInfo.Email = (string)dRow["email"];
                    returnInfo.DateOfBirth = (string)dRow["dob"];
                    returnInfo.clubDaysLeft = (int)dRow["club_daysleft"];
                    returnInfo.clubMonthsLeft = (int)dRow["club_monthsleft"];
                    returnInfo.clubMonthsExpired = (int)dRow["club_monthsexpired"];
                    returnInfo.clubLastUpdate = (DateTime)dRow["club_lastupdate"];
                    returnInfo.SSO = (string)dRow["ticket"];
                    Logging.Log("SELECT username,password,ticket,role,figure,sex,motto,motto_messenger,credits,tickets,film,currentbadge,lastactivity,club_daysleft,club_monthsleft,club_monthsexpired,club_lastupdate,email,dob FROM users WHERE ticket = " + ssoTicket + ";;");
                    Logging.Log(returnInfo.SSO);
                }
                catch { returnInfo = null; }
            }

            return returnInfo;
        }
        public userInformation getUserInfo(int userID, bool forceRefresh)
        {
            if (!forceRefresh && mUserSessions.ContainsKey(userID)) // Why load it? :)
                return mUserSessions[userID].User;

            userInformation returnInfo = new userInformation();
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.Open();

            if (Database.Ready)
            {
                try
                {
                    DataRow dRow = Database.getRow("SELECT username,password,ticket,role,figure,sex,motto,motto_messenger,credits,tickets,film,currentbadge,lastactivity,club_daysleft,club_monthsleft,club_monthsexpired,club_lastupdate,email,dob FROM users WHERE id = @userid");
                    returnInfo.ID = userID;
                    returnInfo.Username = (string)dRow["username"];
                    returnInfo.Password = (string)dRow["password"];
                    returnInfo.Role = (userRole)(int.Parse(dRow["role"].ToString()));
                    returnInfo.Figure = (string)dRow["figure"];
                    returnInfo.Sex = Convert.ToChar(dRow["sex"].ToString());
                    returnInfo.Motto = (string)dRow["motto"];
                    returnInfo.messengerMotto = (string)dRow["motto_messenger"];
                    returnInfo.Credits = (int)dRow["credits"];
                    returnInfo.Tickets = (int)dRow["tickets"];
                    returnInfo.Film = (int)dRow["film"];
                    returnInfo.Badge = (string)dRow["currentbadge"];
                    returnInfo.lastActivity = (DateTime)dRow["lastactivity"];
                    returnInfo.Email = (string)dRow["email"];
                    returnInfo.DateOfBirth = (string)dRow["dob"];
                    returnInfo.clubDaysLeft = (int)dRow["club_daysleft"];
                    returnInfo.clubMonthsLeft = (int)dRow["club_monthsleft"];
                    returnInfo.clubMonthsExpired = (int)dRow["club_monthsexpired"];
                    returnInfo.clubLastUpdate = (DateTime)dRow["club_lastupdate"];
                    returnInfo.SSO = (string)dRow["ticket"];
                }
                catch { returnInfo = null; }
            }

            return returnInfo;
        }
        public userInformation getUserInfo(string Username, bool forceRefresh)
        {
            if (!forceRefresh)
            {
                lock (mUserSessions)
                {
                    Username = Username.ToLower();
                    foreach (Session lSession in mUserSessions.Values)
                    {
                        if (lSession.User != null && lSession.User.Username.ToLower() == Username)
                            return lSession.User;
                    }
                }
            }

            int userID = this.getUserID(Username);
            if (userID > 0)
                return this.getUserInfo(userID, forceRefresh);
            else
                return null;
        }
        public userAccessInformation getLastAccess(int userID)
        {
            if (mUserSessions.ContainsKey(userID))
                return mUserSessions[userID].Access;

            Database dbClient = new Database(false, true);
            dbClient.addParameterWithValue("userid", userID);

            dbClient.Open();
            if (dbClient.Ready)
                return userAccessInformation.Parse(dbClient.getRow("SELECT * FROM users_access WHERE userid = @userid ORDER BY moment DESC LIMIT 1"));
            else
                return null;
        }
        #endregion

        #region Registration etc
        /// <summary>
        /// Returns the ID of the error at a name check of a username/new pet. If no errors, then 0 is returned.
        /// </summary>
        /// <param name="Pet">Specifies if this namecheck is for a pet purchase.</param>
        /// <param name="Name">The name to check.</param>
        public int getNameCheckError(bool Pet, string Name)
        {
            if (Name.Length > 15 || !stringFunctions.usernameIsValid(Name)) // TODO: MOD-, ADM-, SOS- etc blocking
                return 2; // Invalid
            else
            {
                if (Pet == false && userExists(Name))
                    return 4; // Username already taken
                else
                    return 0; // OK
            }
        }
        /// <summary>
        /// Registers a new user by writing the given details into the 'users' table of the database.
        /// </summary>
        /// <param name="Session"></param>
        /// <param name="Info">The information about the new user in a userInformation object.</param>
        public void registerUser(Session Session, userInformation Info)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("username", Info.Username);
            Database.addParameterWithValue("password", Info.Password);
            Database.addParameterWithValue("role", "1");
            Database.addParameterWithValue("figure", Info.Figure);
            Database.addParameterWithValue("sex", Info.Sex.ToString());
            Database.addParameterWithValue("motto", Configuration.getConfigurationValue("users.registration.motto"));
            Database.addParameterWithValue("motto_messenger", Configuration.getConfigurationValue("users.registration.messengermotto"));
            Database.addParameterWithValue("credits", Configuration.getNumericConfigurationValue("users.registration.credits"));
            Database.addParameterWithValue("tickets", Configuration.getNumericConfigurationValue("users.registration.tickets"));
            Database.addParameterWithValue("film", 0);
            Database.addParameterWithValue("email", Info.Email);
            Database.addParameterWithValue("dob", Info.DateOfBirth);
          
            Database.Open();
            if (Database.Ready)
            {
                //Database.runQuery("CALL register_user(@username,@password,@figure,@sex,@email,@dob,@receivemails)");
                Database.runQuery(
                    "INSERT INTO users " + 
                    "(username,password,role,signedup,figure,sex,motto,motto_messenger,credits,tickets,film,lastactivity,club_lastupdate,email,dob) " +
                    "VALUES " +
                    "(@username,@password,@role,NOW(),@figure,@sex,@motto,@motto_messenger,@credits,@tickets,@film,NOW(),NOW(),@email,@dob)");

                Logging.Log("Created user '" + Info.Username + "'.", Logging.logType.userVisitEvent);
            }
            else
                Logging.Log("Failed to create user " + Info.Username + ", because the database was not contactable!", Logging.logType.commonWarning);
        }
        #endregion

        #region Hotel alert & CFH
        public void broadcastMessage(serverMessage Message)
        {
            string sMessage = Message.ToString();
            lock (mUserSessions)
            {
                foreach (Session lSession in mUserSessions.Values)
                {
                    lSession.gameConnection.sendMessage(sMessage);
                }
            }
        }
        public void broadcastHotelAlert(string Text)
        {
            serverMessage hhCast = genericMessageFactory.createMessageBoxCast("Message from Hotel Management:<br>" + Text);
            this.broadcastMessage(hhCast);

            Logging.Log("Hotel Alert ('" + Text + "') sent to all online users.");
        }
        #endregion

        #region Badges
        /// <summary>
        /// Gives a badge to user, after checking if the user doesn't already have this badge. A boolean is returned that indicates if the badge is given or not.
        /// </summary>
        /// <param name="userID">The database ID of the user to give the badge to.</param>
        /// <param name="Badge">The badge of the user to </param>
        public bool giveBadgeToUser(int userID, string Badge)
        {
            Database Database = new Database(false, false);
            Database.addParameterWithValue("userid", userID);
            Database.addParameterWithValue("badge", Badge);
            Database.Open();
            if (Database.Ready)
            {
                if (!Database.findsResult("SELECT userid FROM users_badges WHERE userid = @userid AND badge = @badge LIMIT 1"))
                {
                    Database.runQuery("INSERT INTO users_badges(userid,badge) VALUES (@userid,@badge)");
                    return true;
                }
                Database.Close();
            }

            return false;
        }
        /// <summary>
        /// Removes a certain badge from a given user.
        /// </summary>
        /// <param name="userID">The database ID of the user to remove the badge of.</param>
        /// <param name="Badge">The badge to remove.</param>
        public void removeBadgeFromUser(int userID, string Badge)
        {
            Database Database = new Database(false, false);
            Database.addParameterWithValue("userid", userID);
            Database.addParameterWithValue("badge", Badge);
            Database.Open();
            if (Database.Ready)
                Database.runQuery("DELETE FROM users_badges WHERE userid = @userid AND badge = @badge LIMIT 1");
        }
        /// <summary>
        /// Returns a boolean indicating if a given user posesses a given badge.
        /// </summary>
        /// <param name="User">The database ID of the user to check.</param>
        /// <param name="Badge">The badge to check.</param>
        public bool userHasBadge(userInformation User, string Badge)
        {
            if ((Badge == "HC1" && User.hasClub) || (Badge == "HC2" && User.hasGoldClub)) // Club badge
                return true;

            if (Engine.Game.Roles.roleHasBadge(User.Role, Badge)) // Role badge
                return true;

            // Private badge check
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", User.ID);
            Database.addParameterWithValue("badge", Badge);
            Database.Open();

            return Database.findsResult("SELECT userid FROM users_badges WHERE userid = @userid AND badge = @badge LIMIT 1"); // True if this user has the searched badge as private
        }
        public void addPrivateBadgesToList(int userID, userRole Role, ref List<string> lBadges)
        {
            Database db = new Database(false, true);
            db.addParameterWithValue("userid", userID);
            db.Open();
            if (db.Ready)
            {
                DataTable dTable = db.getTable("SELECT badge FROM users_badges WHERE userid = @userid");
                foreach (DataRow dRow in dTable.Rows)
                {
                    lBadges.Add(dRow["badge"].ToString());
                }
            }
        }
        #endregion
        #endregion
    }
}
