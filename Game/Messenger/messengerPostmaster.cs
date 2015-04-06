using System;
using System.Data;

using Woodpecker.Storage;
using Woodpecker.Net.Game.Messages;

namespace Woodpecker.Game.Messenger
{
    /// <summary>
    /// Provides various functions for processing messenger messages.
    /// </summary>
    public class messengerPostmaster
    {
        #region Fields
        /// <summary>
        /// The maximum amount of messages that will be written in database & sent to receivers synchronous. If the delivery size is over this value, the database actions & delivery will be ran on a separate worker thread.
        /// </summary>
        private int mMaxSyncMessages;
        /// <summary>
        /// A delegate void for writing messages in the database and sending the message to online receivers asynchronous.
        /// </summary>
        /// <param name="receiverIDs">An integer array with the database IDs of the users that receive the message on their messenger.</param>
        /// <param name="pMessage">The Woodpecker.Game.Messenger.messengerMessage object that will be distributed.</param>
        private delegate void messageBirdWorker(int[] receiverIDs, messengerMessage pMessage);
        /// <summary>
        /// An instance of the messageBirdWorker worker thread.
        /// </summary>
        private messageBirdWorker messageBird;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes the postmaster object.
        /// </summary>
        /// <param name="maxSyncMessages">The maximum amount of messages that will be written in database & sent to receivers synchronous. If the delivery size is over this value, the database actions & delivery will be ran on a separate worker thread.</param>
        public messengerPostmaster(int maxSyncMessages)
        {
            if (maxSyncMessages > 0)
                mMaxSyncMessages = maxSyncMessages;
            else
                mMaxSyncMessages = 10;

            messageBird = new messageBirdWorker(_launchBird);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Delivers a messengerMessage object to a given list of receivers. This is done either synchronous or asynchronous depending on the amount of receivers and the configured value for max synchronous messages.
        /// </summary>
        /// <param name="receiverIDs">An integer array with the database IDs of the users that receive the message on their messenger.</param>
        /// <param name="pMessage">The Woodpecker.Game.Messenger.messengerMessage object that will be distributed.</param>
        public void sendMessengerMessage(int[] receiverIDs, messengerMessage pMessage)
        {
            if (pMessage.senderID == 0) // Invalid message object
                return;

            if (receiverIDs.Length > mMaxSyncMessages) // Launch the message bird on a separate thread
                messageBird.BeginInvoke(receiverIDs, pMessage, null, null);
            else
                messageBird.Invoke(receiverIDs, pMessage);
        }
        private void _launchBird(int[] receiverIDs, messengerMessage pMessage)
        {
            Database Database = new Database(false, false);
            Database.addParameterWithValue("senderid", pMessage.senderID);
            Database.addParameterWithValue("sent", pMessage.Sent);
            Database.addParameterWithValue("body", pMessage.Body);
            Database.Open();
            if (Database.Ready)
            {
                //DateTime now = DateTime.Now;
                //Woodpecker.Core.Logging.Log("Start: " + now.ToString("hh:mm:ss:fff"));
                foreach (int receiverID in receiverIDs)
                {
                    if (Engine.Game.Users.userIsLoggedIn(receiverID)) // Receiver is logged in, retrieve the next message ID, write the message in the database & send it to the receiver
                    {
                        int messageID = Database.getInteger("SELECT MAX(messageid) + 1 FROM messenger_messages WHERE receiverid = '" + receiverID + "' LIMIT 1");
                        Database.runQuery("INSERT INTO messenger_messages(receiverid,messageid,senderid,sent,body) VALUES ('" + receiverID + "','" + messageID + "',@senderid,@sent,@body)");

                        serverMessage Message = new serverMessage(134); // "BF"
                        Message.appendWired(1);
                        pMessage.ID = messageID;
                        Message.Append(pMessage.ToString());
                        Engine.Game.Users.trySendGameMessage(receiverID, Message);
                    }
                    else // Receiver is not online, no need for getting our hands on the next message ID etc
                    {
                        Database.runQuery(
                        "INSERT INTO messenger_messages(receiverid,messageid,senderid,sent,body) " +
                        "SELECT " +
                            "'" + receiverID + "'," +
                            "(MAX(messageid) + 1)," +
                            "@senderid," +
                            "@sent," +
                            "@body " +
                       "FROM messenger_messages WHERE receiverid = '" + receiverID + "' LIMIT 1");
                    }
                }
                //now = DateTime.Now;
                //Woodpecker.Core.Logging.Log("Stop: " + now.ToString("hh:mm:ss:fff"));

                Database.Close();
            }
        }
        /// <summary>
        /// Deletes a messenger message of a user, with only the message ID given.
        /// </summary>
        /// <param name="userID">The database ID of the user to delete the message of.</param>
        /// <param name="messageID">The ID of the message to delete.</param>
        public void markMessageAsRead(int userID, int messageID)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.addParameterWithValue("messageid", messageID);
            Database.Open();

            if (Database.Ready)
                Database.runQuery("DELETE FROM messenger_messages WHERE receiverid = @userid AND messageid = @messageid LIMIT 1");
        }
        /// <summary>
        /// Deletes all unread messenger messages that have been dispatched to users, but the sender isnt'in the receiver's buddy list anymore. Keeping the messages in their inbox will error the upon viewing them.
        /// </summary>
        public void dropInvalidMessages()
        {
            Database Database = new Database(true, true);
            if (Database.Ready)
                Database.runQuery("DELETE FROM messenger_messages WHERE senderid NOT IN(SELECT buddyid FROM messenger_buddylist)");
        }
        #endregion
    }
}
