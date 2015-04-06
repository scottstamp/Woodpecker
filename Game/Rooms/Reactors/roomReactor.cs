using System;
using System.Collections.Generic;

using Woodpecker.Specialized.Text;
using Woodpecker.Specialized.Encoding;

using Woodpecker.Net.Game.Messages;
using Woodpecker.Game.Moderation;
using Woodpecker.Game.Rooms.Units;
using Woodpecker.Game.Rooms.Instances;
using Woodpecker.Game.Rooms.Pathfinding;

namespace Woodpecker.Game.Rooms.Instances.Interaction
{
    /// <summary>
    /// Provides common target methods for both public spaces and user flats.
    /// </summary>
    public partial class roomReactor : Reactor
    {
        #region Target methods
        /// <summary>
        /// 52 - "@t"
        /// </summary>
        public void CHAT()
        {
            roomUser Me = Session.roomInstance.getRoomUser(Session.ID);
            if (!Me.isMuted)
            {
                string Text = Request.getParameter(0);
                stringFunctions.filterVulnerableStuff(ref Text, true);

                if (!this.handleSpecialChatCommand(Text))
                {
                    Session.roomInstance.sendTalk(Me.ID, Me.X, Me.Y, Text);
                    Session.roomInstance.animateTalkingUnit(Me, ref Text, true);
                    Engine.Game.Moderation.addChatMessageToLogStack(Session.ID, Session.User.ID, Session.roomID, 0, chatType.say, ref Text);
                }
                this.handleBotResponse(Text);
            }
        }
        /// <summary>
        /// 55 - "@w"
        /// </summary>
        public void SHOUT()
        {
            roomUser Me = Session.roomInstance.getRoomUser(Session.ID);
            if (!Me.isMuted)
            {
                string Text = Request.getParameter(0);
                stringFunctions.filterVulnerableStuff(ref Text, true);
                if (!this.handleSpecialChatCommand(Text))
                {
                    Session.roomInstance.sendShout(Me.ID, Me.X, Me.Y, Text);
                    Session.roomInstance.animateTalkingUnit(Me, ref Text, true);
                    Engine.Game.Moderation.addChatMessageToLogStack(Session.ID, Session.User.ID, Session.roomID, 0, chatType.shout, ref Text);
                }
                this.handleBotResponse(Text);
            }
        }
        /// <summary>
        /// 56 - "@x"
        /// </summary>
        public void WHISPER()
        {
            roomUser Me = Session.roomInstance.getRoomUser(Session.ID);
            if (!Me.isMuted)
            {
                string whisperBody = Request.getParameter(0);
                string receiverName = whisperBody.Substring(0, whisperBody.IndexOf(' '));
                string Text = whisperBody.Substring(receiverName.Length + 1);
                stringFunctions.filterVulnerableStuff(ref Text, true);

                serverMessage Whisper = new serverMessage(25); // "@Y"
                Whisper.appendWired(Me.ID);
                Whisper.appendClosedValue(Text);

                Session.gameConnection.sendMessage(Whisper);
                if (receiverName.Length > 0 && receiverName != Session.User.Username)
                {
                    roomUser Receiver = Session.roomInstance.getRoomUser(receiverName);
                    if (Receiver != null)
                        Receiver.Session.gameConnection.sendMessage(Whisper);
                    Engine.Game.Moderation.addChatMessageToLogStack(Session.ID, Session.User.ID, Session.roomID, Receiver.Session.User.ID, chatType.whisper, ref Text);
                }
            }
        }
        /// <summary>
        /// 60 - "@|"
        /// </summary>
        public void G_HMAP()
        {
            Response.Initialize(31); // "@_"
            Response.Append(Session.roomInstance.getClientFloorMap());
            sendResponse();
        }
        /// <summary>
        /// 61 - "@}"
        /// </summary>
        public void G_USRS()
        {
            Response.Initialize(28); // "@\"
            Response.Append(Session.roomInstance.getRoomUnits());
            sendResponse();
        }
        /// <summary>
        /// 62 - "@~"
        /// </summary>
        
        public void G_OBJS()
        {
            if (Session.roomInstance.Information != null && Session.roomInstance.Information.isUserFlat) //  //Session.roomInstance.getModel().typeName.Contains("model")
            { // Private room
                Response.Initialize(30); // "@^"
                sendResponse();

                Response.Initialize(32); // "@`"
                Response.Append(Session.roomInstance.getFloorItems(false));
                sendResponse();
            }
            else
            { // Public room!
                Response.Initialize(30); // "@^"
                Response.Append(Session.roomInstance.getFloorItems(true));
                sendResponse();

                Response.Initialize(32); // "@`"
                Response.Append("H");
                sendResponse();
            }

        }
        /// <summary>
        /// 63 - "@"
        /// </summary>
        public void G_ITEMS()
        {
            Response.Initialize(45); // "@m"
            Response.Append(Session.roomInstance.getWallItems());
            sendResponse();
        }
        /// <summary>
        /// 64 - "A@"
        /// </summary>
        public void G_STAT()
        {
            Response.Initialize(34); // "@b"
            Response.Append(Session.roomInstance.getRoomUnitStatuses());
            sendResponse();

            Session.roomInstance.startUser(Session);
        }
        /// <summary>
        /// 75 - "AK"
        /// </summary>
        public void MOVE()
        {
            try
            {
                byte toX = Convert.ToByte(base64Encoding.Decode(Request.Content.Substring(0, 2)));
                byte toY = Convert.ToByte(base64Encoding.Decode(Request.Content.Substring(2, 2)));

                Session.roomInstance.requestStartRoomUnitWalk(Session.ID, toX, toY, false);
            }
            catch { }
        }
        /// <summary>
        /// 79 - "AO"
        /// </summary>
        public void LOOKTO()
        {
            roomUser Me = Session.roomInstance.getRoomUser(Session.ID);
            if (!Me.hasStatus("sit") && !Me.hasStatus("lay")) // Can rotate
            {
                string[] Coords = Request.Content.Split(' ');
                int toX = int.Parse(Coords[0]);
                int toY = int.Parse(Coords[1]);

                Me.rotationHead = rotationCalculator.calculateHumanDirection(Me.X, Me.Y, toX, toY);
                Me.rotationBody = Me.rotationHead;
                Me.requiresUpdate = true;
            }
        }
        /// <summary>
        /// 80 - "AP"
        /// </summary>
        public void CARRYDRINK()
        {
            roomUser Me = Session.roomInstance.getRoomUser(Session.ID);
            Items.carryItemHelper.setHandItem(ref Me, Request.Content);
        }
        /// <summary>
        /// 87 - "AW"
        /// </summary>
        public void CARRYITEM()
        {
            string Item = "20"; // Camera
            roomUser Me = Session.roomInstance.getRoomUser(Session.ID);
            Items.carryItemHelper.setHandItem(ref Me, Item);
        }
        /// <summary>
        /// 93 - "A]"
        /// </summary>
        public void DANCE()
        {
            roomUser Me = Session.roomInstance.getRoomUser(Session.ID);
            if(Me.hasStatus("sit") || Me.hasStatus("lay") || Me.hasStatus("swim")) // Can't dance right now
                return;

            string danceType = null;
            if (Request.Content.Length > 0) // Club dance
            {
                int danceID = Request.getNextWiredParameter();
                if (danceID < 1 || danceID > 4 || !Session.User.hasFuseRight("fuse_use_club_dance"))
                    return;
                danceType = danceID.ToString();
            }

            Me.removeStatus("handitem");
            Me.addStatus("dance", "dance", danceType, 0, null, 0, 0);
        }
        /// <summary>
        /// 94 - "A^"
        /// </summary>
        public void WAVE()
        {
            roomUser Me = Session.roomInstance.getRoomUser(Session.ID);
            Me.addStatus("wave", "wave", null, 2, null, 0, 0);
        }
        /// <summary>
        /// 115 - "As"
        /// </summary>
        public void GOAWAY()
        {
            Session.kickFromRoom("");
        }
        /// <summary>
        /// 126 - "A~"
        /// </summary>
        public void GETROOMAD()
        {
            Response.Initialize(208); // "CP"
            string adImage = ""; // TODO: configure this ad
            string adUri = ""; // TODO: idem dito
            if (adImage.Length > 0)
            {
                Response.appendTabbedValue(adImage);
                Response.Append(adUri);
            }
            else
                Response.Append(0); // No advertisement to be shown

            sendResponse();
        }
        /// <summary>
        /// 158 - "B^"
        /// </summary>
        public void SETBADGE()
        {
            string newBadge = Request.getParameter(0);
            if(!Engine.Game.Users.userHasBadge(Session.User,newBadge))
                return;

            bool showBadge = wireEncoding.decodeBoolean(Request.Content[Request.Content.Length - 1]);
            if(showBadge)
                Session.User.Badge = newBadge;
            else
                Session.User.Badge = "";

            Session.roomInstance.updateUserBadge(Session.ID);
            Session.User.updateAppearanceDetails();
        }
        /// <summary>
        /// 215 - "CW"
        /// </summary>
        public void GET_ALIAS_LIST()
        {
            Response.Initialize(295); // "Dg"
            Response.Append(Engine.Game.Items.spriteIndex);
            sendResponse();

            Response.Initialize(297); // "Di"
            Response.appendWired(false);
            sendResponse();
        }
        #endregion
    }
}
