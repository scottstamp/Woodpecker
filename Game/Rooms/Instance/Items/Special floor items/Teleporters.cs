using System;
using System.Data;
using System.Threading;
using System.Collections.Generic;

using Woodpecker.Storage;
using Woodpecker.Specialized.Text;
using Woodpecker.Net.Game.Messages;

using Woodpecker.Game.Items;
using Woodpecker.Game.Items.Pets;
using Woodpecker.Game.Rooms.Units;
using Woodpecker.Game.Rooms.Pathfinding;

namespace Woodpecker.Game.Rooms.Instances.Interaction
{
    public partial class flatReactor : roomReactor
    {
        /// <summary>
        /// 28 - "@\"
        /// </summary>
        public void GETDOORFLAT()
        {
            int itemID = int.Parse(Request.Content);
            Session.roomInstance.startTeleporter(Session.ID, itemID);
        }
        /// <summary>
        /// 81 - "AQ"
        /// </summary>
        public void INTODOOR()
        {
            int itemID = int.Parse(Request.Content);
            Session.roomInstance.enterTeleporter(Session.ID, itemID);
        }
        /// <summary>
        /// 82 - "AR"
        /// </summary>
        public void DOORGOIN()
        {
            int itemID = int.Parse(Request.Content);
            if (itemID == Session.authenticatedTeleporter)
                Session.roomInstance.broadcoastTeleportActivity(itemID, Session.User.Username, true);
        }
    }
}

namespace Woodpecker.Game.Rooms.Instances
{
    public partial class roomInstance
    {
        #region Fields
        private delegate void teleporterOperation(uint sessionID, int itemID);
        #endregion

        #region Methods
        #region Interaction
        public void enterTeleporter(uint sessionID, int itemID)
        {
            floorItem pItem = this.getFloorItem(itemID);
            if (pItem != null && pItem.Definition.Behaviour.isTeleporter) // Valid item
            {
                roomUser pUser = this.getRoomUser(sessionID);
                if (pItem.Rotation == 2 && !(pUser.X == pItem.X + 1 && pUser.Y == pItem.Y))
                    return; // Invalid position of room user
                if (pItem.Rotation == 4 && !(pUser.X == pItem.X && pUser.Y == pItem.Y + 1))
                    return; // Invalid position of room user

                pUser.Path.Clear();
                blisterMoleNode pNode = new blisterMoleNode();
                pNode.X = pItem.X;
                pNode.Y = pItem.Y;
                pUser.Path.Add(pNode);

                requestStartRoomUnitWalk(pUser.Session.ID, pNode.X, pNode.Y, true);
            }
        }
        public void startTeleporter(uint sessionID, int itemID)
        {
            new teleporterOperation(this.operateTeleporter).BeginInvoke(sessionID, itemID, null, null);
        }
        private void operateTeleporter(uint sessionID, int itemID)
        {
            floorItem pItem = this.getFloorItem(itemID);
            if (pItem != null && pItem.Definition.Behaviour.isTeleporter) // Valid item
            {
                Thread.Sleep(550); // Wait for room worker thread to locate user into teleporter
                roomUser pUser = this.getRoomUser(sessionID);

                if (pUser != null && pUser.X == pItem.X && pUser.Y == pItem.Y) // In teleporter
                {
                    int roomID = Engine.Game.Items.getTeleporterRoomID(pItem.teleporterID);
                    if (roomID == 0)
                        return;

                    pUser.Clamped = true;
                    this.broadcoastTeleportActivity(pItem.ID, pUser.Session.User.Username, true);
                    this.gridUnit[pItem.X, pItem.Y] = false; // Unblock teleporter

                    if (roomID == this.roomID)
                    {
                        Thread.Sleep(500);
                        floorItem pTeleporter2 = this.getFloorItem(pItem.teleporterID);
                        pUser.X = pTeleporter2.X;
                        pUser.Y = pTeleporter2.Y;
                        pUser.Z = pTeleporter2.Z;
                        pUser.rotationHead = pTeleporter2.Rotation;
                        pUser.rotationBody = pTeleporter2.Rotation;
                        this.gridUnit[pUser.X, pUser.Y] = true; // Block teleporter 2

                        this.broadcoastTeleportActivity(pTeleporter2.ID, pUser.Session.User.Username, false);
                        pUser.Clamped = false;
                        pUser.requiresUpdate = true;
                    }
                    else
                    {
                        pUser.Session.authenticatedFlat = roomID;
                        pUser.Session.authenticatedTeleporter = pItem.teleporterID;

                        serverMessage Message = new serverMessage(62); // "@~"
                        Message.appendWired(pItem.teleporterID);
                        Message.appendWired(roomID);
                        pUser.Session.gameConnection.sendMessage(Message);
                    }
                }
            }
        }
        #endregion

        #region Event broadcoasting
        public void broadcoastTeleportActivity(int itemID, string Username, bool disappearUser)
        {
            floorItem pItem = this.getFloorItem(itemID);
            if (pItem != null && pItem.Definition.Behaviour.isTeleporter)
            {
                serverMessage Message = new serverMessage();
                if (disappearUser)
                    Message.Initialize(89); // "AY"
                else
                    Message.Initialize(92); // "A\"
                Message.Append(pItem.ID);
                Message.Append("/");
                Message.Append(Username);
                Message.Append("/");
                Message.Append(pItem.Definition.Sprite);

                this.sendMessage(Message);
            }
        }
        #endregion
        #endregion
    }
}