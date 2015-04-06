using System;
using System.Collections.Generic;
using System.Data;

using Woodpecker.Core;
using Woodpecker.Storage;
using Woodpecker.Sessions;
using Woodpecker.Net.Game.Messages;
using Woodpecker.Game.Users;

namespace Woodpecker.Game.Messenger
{
    /// <summary>
    /// Provides various functions for the in-game messenger ('Console') for sending messages to users, adding users to buddylist etc. Also features a postmaster.
    /// </summary>
    public class messengerManager
    {
        #region Fields
        private int _maxBuddyListLength;
        /// <summary>
        /// The max length of the messenger buddy list on the messenger.
        /// </summary>
        public int maxBuddyListLength
        {
            get { return this._maxBuddyListLength; }
        }
        private int _maxBuddyListLength_Extended;
        /// <summary>
        /// The max length of the extended messenger buddy list on the messenger for.
        /// </summary>
        public int maxBuddyListLength_Extended
        {
            get { return this._maxBuddyListLength_Extended; }
        }
        private messengerPostmaster _Postmaster;
        /// <summary>
        /// The messengerPostmaster, which procivides methods for saving and sending messenger messages, aswell for deleting messages.
        /// </summary>
        public messengerPostmaster Postmaster
        {
            get { return _Postmaster; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the configuration values for the messenger.
        /// </summary>
        public void setConfiguration()
        {
            this._maxBuddyListLength = Configuration.getNumericConfigurationValue("users.messenger.buddylist.maxlength");
            this._maxBuddyListLength = Configuration.getNumericConfigurationValue("users.messenger.buddylist.maxlength_extended");
            if (this._maxBuddyListLength == 0)
                this._maxBuddyListLength = 200;
            if (this._maxBuddyListLength_Extended < this._maxBuddyListLength)
                this._maxBuddyListLength_Extended = 600;

            int maxSyncMessages = Configuration.getNumericConfigurationValue("users.messenger.postmaster.maxsyncmessages");
            this._Postmaster = new messengerPostmaster(maxSyncMessages);
        }
        /// <summary>
        /// Returns the max amount of buddies a given user can have on his buddy list for the messenger. The user's role and club subscription is being checked.
        /// </summary>
        /// <param name="User">The userInformation object containing the values to calculate the length with.</param>
        public int getMaxBuddyListLength(userInformation User)
        {
            if (User.hasFuseRight("fuse_extended_buddylist"))
                return this._maxBuddyListLength_Extended;
            else
                return this._maxBuddyListLength;
        }

        /// <summary>
        /// Returns the messengerBuddy object of a user.
        /// </summary>
        /// <param name="userID">The database ID of the user to get the messengerBuddy object of.</param>
        public messengerBuddy getBuddy(int userID)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.Open();
            DataRow dRow = Database.getRow("SELECT id,username,figure,sex,motto_messenger,lastactivity FROM users WHERE id = @userid");

            return messengerBuddy.Parse(dRow);
        }
        /// <summary>
        /// Returns the messenger buddies for a given user ID as an array of messengerBuddy.
        /// </summary>
        /// <param name="userID">The database ID of the user to get the messenger buddies of.</param>
        public messengerBuddy[] getBuddies(int userID)
        {
            List<messengerBuddy> Buddies = new List<messengerBuddy>();
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.Open();

            if (Database.Ready)
            {
                DataTable dTable = Database.getTable("SELECT id,username,figure,sex,motto_messenger,lastactivity FROM users WHERE id IN(SELECT buddyid FROM messenger_buddylist WHERE userid = @userid AND accepted = '1') OR id IN(SELECT userid FROM messenger_buddylist WHERE buddyid = @userid AND accepted = '1')");
                foreach(DataRow dRow in dTable.Rows)
                {
                    Buddies.Add(messengerBuddy.Parse(dRow));
                }
            }

            return Buddies.ToArray();
        }
        public List<int> getBuddyIDs(int userID)
        {
            List<int> buddyIDs = new List<int>();
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.Open();

            if (Database.Ready)
            {
                DataTable dTable = Database.getTable("SELECT id FROM users WHERE id IN(SELECT buddyid FROM messenger_buddylist WHERE userid = @userid AND accepted = '1') OR id IN(SELECT userid FROM messenger_buddylist WHERE buddyid = @userid AND accepted = '1')");
                foreach (DataRow dRow in dTable.Rows)
                {
                    buddyIDs.Add((int)dRow["id"]);
                }
            }

            return buddyIDs;
        }
        /// <summary>
        /// Returns the messenger buddy requests for a given user ID as an array of messengerBuddyRequest.
        /// </summary>
        /// <param name="userID">The database ID of the user to get the messenger buddy requests of.</param>
        public messengerBuddyRequest[] getBuddyRequests(int userID)
        {
            List<messengerBuddyRequest> Requests = new List<messengerBuddyRequest>();
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.Open();

            if (Database.Ready)
            {
                DataTable dTable = Database.getTable("SELECT id,username FROM users WHERE id = (SELECT userid FROM messenger_buddylist WHERE buddyid = @userid AND accepted = '0')");
                foreach (DataRow dRow in dTable.Rows)
                {
                    messengerBuddyRequest Request = new messengerBuddyRequest();
                    Request.userID = (int)dRow["id"];
                    Request.Username = (string)dRow["username"];

                    Requests.Add(Request);
                }
            }

            return Requests.ToArray();
        }
        /// <summary>
        /// Retrieves all non-read messages of a given user and returns it as a messengerMessage array.
        /// </summary>
        /// <param name="userID">The database ID of the user to get the messages for.</param>
        public messengerMessage[] getMessages(int userID)
        {
            List<messengerMessage> Messages = new List<messengerMessage>();
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.Open();

            if (Database.Ready)
            {
                DataTable dTable = Database.getTable("SELECT messageid,senderid,sent,body FROM messenger_messages WHERE receiverid = @userid ORDER BY messageid ASC");
                foreach (DataRow dRow in dTable.Rows)
                {
                    messengerMessage Message = new messengerMessage();
                    Message.ID = (int)dRow["messageid"];
                    Message.senderID = (int)dRow["senderid"];
                    Message.Sent = (DateTime)dRow["sent"];
                    Message.Body = (string)dRow["body"];

                    Messages.Add(Message);
                }
            }

            return Messages.ToArray();
        }
        /// <summary>
        /// Returns a boolean indicating if two users (given by their ID) are buddies.
        /// </summary>
        /// <param name="userID">The database ID of user 1.</param>
        /// <param name="userID2">The database ID of user 2.</param>
        public bool areBuddies(int userID, int userID2, bool requestsCount)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.addParameterWithValue("userid2", userID2);
            Database.Open();

            if(requestsCount)
                return Database.findsResult("SELECT userid FROM messenger_buddylist WHERE (userid = @userid AND buddyid = @userid2) OR (userid = @userid2 AND buddyid = @userid) LIMIT 1");
            else
                return Database.findsResult("SELECT userid FROM messenger_buddylist WHERE (userid = @userid AND buddyid = @userid2) OR (userid = @userid2 AND buddyid = @userid) AND accepted = '1' LIMIT 1");
        }
        /// <summary>
        /// Writes a buddy request from a given user to another user into the database, and notifies the receiver with the new request if it's online.
        /// </summary>
        /// <param name="User">The userInformation object of the user that sends the request.</param>
        /// <param name="userID2">The database ID of the receiving user.</param>
        public void requestBuddy(userInformation User, int userID2)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", User.ID);
            Database.addParameterWithValue("userid2", userID2);
            Database.Open();
            Database.runQuery("INSERT INTO messenger_buddylist(userid,buddyid) VALUES (@userid,@userid2)");

            if (Engine.Game.Users.userIsLoggedIn(userID2)) // Receiver is online
            {
                serverMessage Message = new serverMessage(132); // "BD"
                Message.appendWired(User.ID);
                Message.appendClosedValue(User.Username);

                Engine.Game.Users.trySendGameMessage(userID2, Message);
            }
        }
        public void removeBuddy(int userID, int buddyID)
        {
            Database Database = new Database(false, false);
            Database.addParameterWithValue("userid", userID);
            Database.addParameterWithValue("buddyid", buddyID);
            Database.Open();

            if (Database.Ready)
            {
                Database.runQuery("DELETE FROM messenger_buddylist WHERE (userid = @userid AND buddyid = @buddyid) OR (userid = @buddyid AND buddyid = @userid) AND accepted = '1' LIMIT 1");
                Database.runQuery("DELETE FROM messenger_messages WHERE (receiverid = @userid AND senderid = @buddyid) OR (receiverid = @buddyid AND senderid = @userid)"); // Delete messages between users

                Database.Close();
            }

            if (Engine.Game.Users.userIsLoggedIn(buddyID)) // Victim is online
            {
                serverMessage Message = new serverMessage(138); // "BJ"
                Message.appendWired(1);
                Message.appendWired(userID);
                Engine.Game.Users.trySendGameMessage(buddyID,Message);
            }
        }
        /// <summary>
        /// Accepts a buddy request and notifies the sender (if online) and the receiver.
        /// </summary>
        /// <param name="Session">The Woodpecker.Sessions.Session object of the user that accepts the request.</param>
        /// <param name="senderID">The database ID of the user that sent the request.</param>
        public void acceptBuddyRequest(ref Session Session, int senderID)
        {
            Database Database = new Database(false, false);
            Database.addParameterWithValue("userid", Session.User.ID);
            Database.addParameterWithValue("senderid", senderID);
            Database.Open();

            if (Database.findsResult("SELECT userid FROM messenger_buddylist WHERE userid = @senderid AND buddyid = @userid AND accepted = '0' LIMIT 1"))
            {
                Database.runQuery("UPDATE messenger_buddylist SET accepted = '1' WHERE userid = @senderid AND buddyid = @userid LIMIT 1");
                Database.Close();

                serverMessage Message = new serverMessage();
                if (Engine.Game.Users.userIsLoggedIn(senderID)) // Sender is online!
                {
                    Message.Initialize(137); // "BI"
                    Message.Append(getBuddy(Session.User.ID).ToString());
                    Engine.Game.Users.trySendGameMessage(senderID, Message);
                }

                Message.Initialize(137); // "BI"
                Message.Append(getBuddy(senderID).ToString());
                Session.gameConnection.sendMessage(Message);
            }
            else
                Database.Close();
        }
        /// <summary>
        /// Declines a unaccepted buddy request for the messenger.
        /// </summary>
        /// <param name="userID">The database ID of the user that declines the request.</param>
        /// <param name="senderID">The database ID of the user that sent the request.</param>
        public void declineBuddyRequest(int userID, int senderID)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            if(senderID > 0)
                Database.addParameterWithValue("senderid", senderID);
            Database.Open();

            if (Database.Ready)
            {
                if (senderID > 0) // Specific request was rejected
                    Database.runQuery("DELETE FROM messenger_buddylist WHERE userid = @senderid AND buddyid = @userid AND accepted = '0' LIMIT 1");
                else // All requests were rejected
                    Database.runQuery("DELETE FROM messenger_buddylist WHERE buddyid = @userid AND accepted = '0'");
            }
        }
        #endregion
    }
}
