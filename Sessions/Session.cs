using System;
using System.Net.Sockets;
using System.Collections.Generic;

using Woodpecker.Core;
using Woodpecker.Net.Game;
using Woodpecker.Net.Game.Messages;

using Woodpecker.Game.Users;
using Woodpecker.Game.Items;
using Woodpecker.Game.Rooms;
using Woodpecker.Game.Rooms.Instances;
using Woodpecker.Game.Rooms.Instances.Interaction;

namespace Woodpecker.Sessions
{
    /// <summary>
    /// A global class for a user session, containing connections to the server for game client and MUS.
    /// </summary>
    public class Session
    {
        #region Fields
        /// <summary>
        /// The session ID (connection ID) of this session. Datatype is unsigned 32 bits integer.
        /// </summary>
        public uint ID;
        /// <summary>
        /// The DateTime object containing the date and time of this session's start.
        /// </summary>
        private DateTime Started;
        /// <summary>
        /// The gameConnection object, representing the gameclient connection to the server.
        /// </summary>
        public gameConnection gameConnection;
        /// <summary>
        /// True if this session can still process packets etc.
        /// </summary>
        public bool isValid = true;
        /// <summary>
        /// The userInformation object containing data about this session user. (after login)
        /// </summary>
        public userInformation User;
        public userAccessInformation Access = new userAccessInformation();
        /// <summary>
        /// True if this session is currently holding a user. (so 'is logged in')
        /// </summary>
        public bool isHoldingUser
        {
            get { return this.User != null; }
        }

        /// <summary>
        /// Gets the IP address of this session user from the gameConnection object.
        /// </summary>
        public string ipAddress
        {
            get
            {
                if (this.gameConnection != null)
                    return this.gameConnection.ipAddress;
                else
                    return "";
            }
        }
        /// <summary>
        /// True if pong ('CD') has been received.
        /// </summary>
        public bool pongReceived;
        /// <summary>
        /// True if this connection is ready to send packets to.
        /// </summary>
        public bool isReady
        {
            get { return (this.gameConnection != null); }
        }

        #region Room related
        /// <summary>
        /// The database ID of the room the user currently is in.
        /// </summary>
        public int roomID;
        /// <summary>
        /// True if this user is currently an active user in a room.
        /// </summary>
        public bool inRoom
        {
            get { return this.roomID > 0 && this.roomInstance != null; }
        }
        /// <summary>
        /// The Woodpecker.Game.Rooms.Instances.Room room instance of the room the user is currently entering/is active in.
        /// </summary>
        public roomInstance roomInstance;
        /// <summary>
        /// The database ID of the room the user has authenticated for.
        /// </summary>
        public int authenticatedFlat;
        /// <summary>
        /// The database ID of the room the user is waiting for a doorbell response of.
        /// </summary>
        public int waitingFlat;
        /// <summary>
        /// The database ID of the teleporter item ('door') that this user is currently using to 'teleport' between two room instances.
        /// </summary>
        public int authenticatedTeleporter;

        public itemStripHandler itemStripHandler;
        #endregion
        #region Administration
        public bool StackAnything = false;
        public float StackHeight = 0.0F;

        public int PurchaseMultiplier = 1;
        #endregion
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes the Session object.
        /// </summary>
        /// <param name="sessionID">The ID this session was assigned to by the Woodpecker.Sessions.sessionManager.</param>
        /// <param name="gameClient">The System.Net.Sockets.Socket object of the game client that has been accepted by the game connection listener.</param>
        public Session(uint sessionID, Socket gameClient)
        {
            this.ID = sessionID;
            this.gameConnection = new gameConnection(this, gameClient);
        }
        #endregion

        #region Methods
        #region Core methods
        /// <summary>
        /// Starts the message procedure for the game client.
        /// </summary>
        public void Start()
        {
            this.gameConnection.Initialize();
            DateTime dtNow = DateTime.Now;
            this.Started = dtNow;
        }
        /// <summary>
        /// Destroys the session and clears up all used resources.
        /// </summary>
        public void Destroy()
        {
            if (this.isHoldingUser)
            {
                Engine.Game.Users.removeUserSession(this.User.ID);
                this.leaveRoom(false);
                this.User = null;

                this.itemStripHandler.saveHandItems();
                this.itemStripHandler.Clear();
                this.itemStripHandler = null;
            }
            this.gameConnection.Abort();
            this.gameConnection = null;
        }
        #endregion

        #region Room instance related methods
        /// <summary>
        /// Tries to remove this session's user from it's current room instance.
        /// </summary>
        public void leaveRoom(bool cleanupReactors)
        {
            this.abortTrade();

            if (this.waitingFlat > 0)
            {
                if (Engine.Game.Rooms.roomInstanceRunning(this.waitingFlat))
                    Engine.Game.Rooms.getRoomInstance(this.waitingFlat).unRegisterSession(this.ID);
            }
            else
            {
                if (this.inRoom)
                    this.roomInstance.unRegisterSession(this.ID);
            }

            if (cleanupReactors) // Cleanup!
            {
                gameConnection.reactorHandler.unRegister(new flatReactor().GetType());
                gameConnection.reactorHandler.unRegister(new roomReactor().GetType());
            }

            this.roomInstance = null;
            this.roomID = 0;
            if(this.authenticatedTeleporter == 0)
                this.authenticatedFlat = 0;
            this.waitingFlat = 0;
        }
        /// <summary>
        /// Kicks a user from it's current room and optionally sends a 'moderator warning' message.
        /// </summary>
        /// <param name="Alert">Optional. The moderator message to display.</param>
        public void kickFromRoom(string Alert)
        {
            gameConnection.sendMessage(new serverMessage(18)); // "@R"
            leaveRoom(true);
            
            if (Alert.Length > 0)
            {
                gameConnection.sendLocalizedError("mod_warn/" + Alert);
            }
        }
        #endregion

        #region Item inventory and trading
        /// <summary>
        /// Modifies the hand page index with a given mode and sends the current hand page of this user.
        /// </summary>
        /// <param name="szMode">'How-to' modify the hand page index.</param>
        public void sendHandStrip(string szMode)
        {
            if (this.itemStripHandler != null)
            {
                this.itemStripHandler.changeHandStripPage(szMode); // Switch!

                serverMessage Message = new serverMessage(140); // "BL"
                Message.Append(this.itemStripHandler.getHandItemCasts());
                Message.appendChar(13);
                Message.Append(this.itemStripHandler.handItemCount);

                this.gameConnection.sendMessage(Message);
            }
        }
        /// <summary>
        /// Refresh the trade boxes for this session's user and his/her trade partner's session user. Only works when both users are trading.
        /// </summary>
        public void refreshTradeBoxes()
        {
            if (this.itemStripHandler.isTrading)
            {
                Session partnerSession = Engine.Sessions.getSession(this.itemStripHandler.tradePartnerSessionID);
                if (partnerSession == null || !partnerSession.itemStripHandler.isTrading)
                    return;

                string myBox = itemStripHandler.generateTradeBox(this);
                string partnerBox = itemStripHandler.generateTradeBox(partnerSession);

                serverMessage Message = new serverMessage(108); // "Al"
                Message.Append(myBox);
                Message.Append(partnerBox);
                this.gameConnection.sendMessage(Message);

                Message.Initialize(108); // "Al"
                Message.Append(partnerBox);
                Message.Append(myBox);
                partnerSession.gameConnection.sendMessage(Message);
            }
        }
        /// <summary>
        /// Aborts the current trade.
        /// </summary>
        public void abortTrade()
        {
            if (this.itemStripHandler.isTrading && this.inRoom)
            {
                serverMessage tradeWindowCloser = new serverMessage(110); // "An"
                serverMessage forcedRefresh = new serverMessage(101); // "Ae"
                Session partnerSession = Engine.Sessions.getSession(this.itemStripHandler.tradePartnerSessionID);
                
                this.itemStripHandler.stopTrade();
                partnerSession.itemStripHandler.stopTrade();

                this.roomInstance.getRoomUser(this.ID).removeStatus("trd");
                this.roomInstance.getRoomUser(partnerSession.ID).removeStatus("trd");

                this.gameConnection.sendMessage(tradeWindowCloser);
                this.gameConnection.sendMessage(forcedRefresh);

                partnerSession.gameConnection.sendMessage(tradeWindowCloser);
                partnerSession.gameConnection.sendMessage(forcedRefresh);
            }
        }
        #endregion

        #region Refresh methods
        /// <summary>
        /// Sends the available figure parts for this session user.
        /// </summary>
        public void refreshFigureParts()
        {
            serverMessage Message = new serverMessage(8); // "@H"
            Message.Append("[");
            if (this.isHoldingUser && this.User.hasClub) // Can use 'Club' parts
                Message.Append(Configuration.getConfigurationValue("users.figure.parts.club"));
            else
                Message.Append(Configuration.getConfigurationValue("users.figure.parts.default"));
            Message.Append("]");

            this.gameConnection.sendMessage(Message);
        }
        /// <summary>
        /// Retrieves the FUSE rights for the user on this session and sends it to the client.
        /// </summary>
       public void refreshFuseRights()
        {
            if (this.isHoldingUser)
            {
                serverMessage Message = new serverMessage(2); // "@B"
                Message.Append(Engine.Game.Roles.getRightsForRole(this.User.Role, this.User.hasClub));
                
                this.gameConnection.sendMessage(Message);
            }
        }
        /// <summary>
        /// Refreshes the session user's credit amount. (message 6: @F)
        /// </summary>
        public void refreshCredits()
        {
            if (this.isHoldingUser)
            {
                serverMessage Message = new serverMessage(6); // "@F"
                Message.Append(this.User.Credits);
                Message.Append(".0");

                this.gameConnection.sendMessage(Message);
            }
        }
        /// <summary>
        /// Refreshes the session user's game ticket amount. (message 124: A|)
        /// </summary>
        public void refreshTickets()
        {
            if (this.isHoldingUser)
            {
                this.User.updateValueables();

                serverMessage Message = new serverMessage(124); // "A|"
                Message.Append(this.User.Tickets);

                this.gameConnection.sendMessage(Message);
            }
        }
        /// <summary>
        /// Refreshes the session user's film amount for the camera. (message 4: @D)
        /// </summary>
        public void refreshFilm()
        {
            if (this.isHoldingUser)
            {
                serverMessage Message = new serverMessage(4); // "@D"
                Message.Append(this.User.Film);

                this.gameConnection.sendMessage(Message);
            }
        }
        /// <summary>
        /// Refreshes the badges for this session user and re-builds the badges message. (229, "Ce")
        /// </summary>
        public void refreshBadgeList()
        {
            if (this.isHoldingUser)
            {
                List<string> myBadges = Engine.Game.Roles.getDefaultBadgesForRole(this.User.Role);
                myBadges.Remove("HC1");
                Engine.Game.Users.addPrivateBadgesToList(this.User.ID, this.User.Role, ref myBadges);

                if (this.User.hasClub) // Regular club member
                    myBadges.Add("HC1"); // TODO: make this configurable
                if (this.User.hasGoldClub) // Gold club member
                    myBadges.Add("HC2"); // TODO: make this configurable

                if (!myBadges.Contains(this.User.Badge)) // Set badge not valid anymore
                    this.User.Badge = "";

                serverMessage Message = new serverMessage(229); // "Ce"
                Message.appendWired(myBadges.Count);

                int badgeSlot = 0;
                int slotCounter = 0;
                foreach (string lBadge in myBadges)
                {
                    Message.appendClosedValue(lBadge);
                    if (lBadge == this.User.Badge) // Current badge, set slot
                        badgeSlot = slotCounter;

                    slotCounter++;
                }
                Message.appendWired(badgeSlot);
                Message.appendWired((this.User.Badge.Length > 0));

                this.gameConnection.sendMessage(Message);
            }
        }
        /// <summary>
        /// Refreshes the session user's club status.
        /// </summary>
        public void refreshClubStatus()
        {
            if (this.isHoldingUser)
            {
                serverMessage Message = new serverMessage(7); // "@G"
                Message.appendClosedValue("club_habbo");
                Message.appendWired(this.User.clubDaysLeft);
                Message.appendWired(this.User.clubMonthsExpired);
                Message.appendWired(this.User.clubMonthsLeft);
                Message.appendWired(true); // Hide/shows 'days left' label

                this.gameConnection.sendMessage(Message);
            }
        }
        /// <summary>
        /// Only works if the session user is in a room. If so, then a whisper with a given message is sent to the user only.
        /// </summary>
        /// <param name="Message">The text message to whisper to the user.</param>
        public void castWhisper(string sMessage)
        {
            if (this.inRoom)
            {
                serverMessage Message = new serverMessage(25); // "@Y"
                Message.appendWired(this.roomInstance.getSessionRoomUnitID(this.ID));
                Message.appendClosedValue(sMessage);

                this.gameConnection.sendMessage(Message);
            }
        }
        #endregion
        #endregion
    }
}
