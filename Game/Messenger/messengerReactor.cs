using System;
using System.Collections.Generic;

using Woodpecker.Specialized.Text;
using Woodpecker.Specialized.Encoding;

namespace Woodpecker.Game.Messenger
{
    /// <summary>
    /// Contains target methods for functions related to the in-game messenger. ('Console')
    /// </summary>
    public class messengerReactor : Reactor
    {
        public void initializeBuddyList()
        {
            Response.Initialize(12); // "@L"
            Response.appendClosedValue(Session.User.messengerMotto);

            int maxLengthExtendedList = Engine.Game.Messenger.maxBuddyListLength_Extended;
            Response.appendWired(maxLengthExtendedList);
            Response.appendWired(Engine.Game.Messenger.maxBuddyListLength);
            Response.appendWired(maxLengthExtendedList);

            messengerBuddy[] Buddies = Engine.Game.Messenger.getBuddies(Session.User.ID);
            
            Response.appendWired(Buddies.Length);
            foreach (messengerBuddy lBuddy in Buddies)
            {
                Response.Append(lBuddy.ToString());
            }

            sendResponse();

            /* V14 messenger sends message 191 and message 233 after receiving @L, V13 does not.
            Invoke it manually... */

            this.MESSENGER_GETMESSAGES();
            this.MESSENGER_GETREQUESTS();
        }
        /// <summary>
        /// 15 - "@O"
        /// </summary>
        public void MESSENGER_UPDATE()
        {
            Response.Initialize(13); // "@M"
            messengerBuddy[] Buddies = Engine.Game.Messenger.getBuddies(Session.User.ID);
            Response.appendWired(Buddies.Length);

            foreach (messengerBuddy lBuddy in Buddies)
            {
                Response.Append(lBuddy.ToStatusString());
            }
            sendResponse();

            Session.User.updateLastActivity();
        }
        /// <summary>
        /// 32 - "@`"
        /// </summary>
        public void MESSENGER_MARKREAD()
        {
            int messageID = Request.getNextWiredParameter();
            Engine.Game.Messenger.Postmaster.markMessageAsRead(Session.User.ID, messageID);
        }
        /// <summary>
        /// 33 - "@a"
        /// </summary>
        public void MESSENGER_SENDMSG()
        {
            int receiverAmount = Request.getNextWiredParameter();
            Request.Content = Request.Content.Substring(wireEncoding.Encode(receiverAmount).Length);

            List<int> buddyIDs = Engine.Game.Messenger.getBuddyIDs(Session.User.ID);
            List<int> receiverIDs = new List<int>();
            for (int i = 0; i < receiverAmount; i++)
            {
                int receiverID = Request.getNextWiredParameter();
                if (buddyIDs.Contains(receiverID) && !receiverIDs.Contains(receiverID))
                    receiverIDs.Add(receiverID);

                Request.Content = Request.Content.Substring(wireEncoding.Encode(receiverID).Length);
            }
            buddyIDs = null;

            messengerMessage pMessage = new messengerMessage();
            pMessage.senderID = Session.User.ID;
            pMessage.Sent = DateTime.Now;
            pMessage.Body = Request.getParameter(0);
            stringFunctions.filterVulnerableStuff(ref pMessage.Body, false);

            Engine.Game.Messenger.Postmaster.sendMessengerMessage(receiverIDs.ToArray(), pMessage);
        }
        /// <summary>
        /// 36 - "@d"
        /// </summary>
        public void MESSENGER_ASSIGNPERSMSG()
        {
            string newMotto = Request.getParameter(0);
            stringFunctions.filterVulnerableStuff(ref newMotto, true);

            Session.User.messengerMotto = newMotto;
            Session.User.updateAppearanceDetails();

            Response.Initialize(147); // "BS"
            Response.appendClosedValue(newMotto);
            sendResponse();
        }
        /// <summary>
        /// 37 - "@e"
        /// </summary>
        public void MESSENGER_ACCEPTBUDDY()
        {
            int buddyID = Request.getNextWiredParameter();
            Engine.Game.Messenger.acceptBuddyRequest(ref this.Session, buddyID);
        }
        /// <summary>
        /// 38 - "@f"
        /// </summary>
        public void MESSENGER_DECLINEBUDDY()
        {
            int buddyID = 0;
            int[] args = Request.getWiredParameters();
            if (args[0] > 0) // Specific request declined
                buddyID = args[1];

            Engine.Game.Messenger.declineBuddyRequest(Session.User.ID, buddyID);
        }
        /// <summary>
        /// 39 - "@g"
        /// </summary>
        public void MESSENGER_REQUESTBUDDY()
        {
            string Username = Request.getParameter(0);
            int userID = Engine.Game.Users.getUserID(Username);

            if (userID == 0) // User does not exist
                return;

            if (Engine.Game.Messenger.areBuddies(Session.User.ID, userID, true)) // Already buddies / already request pending
                return;

            Engine.Game.Messenger.requestBuddy(Session.User, userID);
        }
        /// <summary>
        /// 40 - "@h"
        /// </summary>
        public void MESSENGER_REMOVEBUDDY()
        {
            int buddyID = wireEncoding.Decode(Request.Content.Substring(1));
            Response.Initialize(138); // "BJ"
            Response.appendWired(1);
            Response.appendWired(buddyID);
            sendResponse();

            Engine.Game.Messenger.removeBuddy(Session.User.ID, buddyID);
        }
        /// <summary>
        /// 41 - "@i"
        /// </summary>
        public void FINDUSER()
        {
            string Username = Request.getParameter(0);
            int userID = Engine.Game.Users.getUserID(Username);

            Response.Initialize(128); // "B@"
            Response.appendClosedValue("MESSENGER");

            if (userID > 0)
                Response.Append(Engine.Game.Messenger.getBuddy(userID).ToString());
            else
                Response.appendWired(0);

            sendResponse();
        }
        /// <summary>
        /// 191 - "B"
        /// </summary>
        public void MESSENGER_GETMESSAGES()
        {
            messengerMessage[] Messages = Engine.Game.Messenger.getMessages(Session.User.ID);
            
            foreach(messengerMessage lMessage in Messages)
            {
                Response.Initialize(134); // "BF"
                Response.appendWired(1);
                Response.Append(lMessage.ToString());
                sendResponse();
            }
        }
        /// <summary>
        /// 201 - "CI"
        /// </summary>
        public void MESSENGER_REPORTMESSAGE()
        {
            int messageID = Request.getNextWiredParameter();
            Session.gameConnection.sendLocalizedError("not_implemented");
        }
        /// <summary>
        /// 233 - "Ci"
        /// </summary>
        public void MESSENGER_GETREQUESTS()
        {
            messengerBuddyRequest[] Requests = Engine.Game.Messenger.getBuddyRequests(Session.User.ID);
            
            foreach (messengerBuddyRequest lRequest in Requests)
            {
                Response.Initialize(132); // "BD"
                Response.appendWired(lRequest.userID);
                Response.appendClosedValue(lRequest.Username);
                sendResponse();
            }
        }
    }
}
