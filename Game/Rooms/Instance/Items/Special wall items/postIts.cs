using System;
using System.Data;
using System.Threading;
using System.Collections.Generic;

using Woodpecker.Storage;
using Woodpecker.Specialized.Text;
using Woodpecker.Specialized.Fun;
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
        /// 83 - "AS"
        /// </summary>
        public void G_IDATA()
        {
            int itemID = int.Parse(Request.Content);
            wallItem pItem = Session.roomInstance.getWallItem(itemID);
            if (pItem != null && pItem.Definition.Behaviour.isPostIt)
            {
                Response.Initialize(48); // "@p"
                Response.appendTabbedValue(itemID.ToString());
                Response.Append(pItem.customData); // Color
                Response.Append(" ");
                Response.Append(pItem.postItMessage);
                sendResponse();

                // Read data aloud if message starts with '[!]'
                if (pItem.postItMessage.Length > 4 && pItem.postItMessage.Substring(1, 3) == "[!]")
                {
                    Response = FunUtils.CreateVoiceSpeakMessage(pItem.postItMessage.Substring(4));
                    sendResponse();
                }
            }
        }
        /// <summary>
        /// 84 - "AT"
        /// </summary>
        public void SETITEMDATA()
        {
            if (Session.roomInstance.sessionHasRights(Session.ID))
            {
                int itemID = int.Parse(Request.Content.Substring(0, Request.Content.IndexOf('/')));
                int idStringLength = itemID.ToString().Length;
                string Color = Request.Content.Substring(idStringLength + 1, 6);
                if (!Engine.Game.Items.postItColorIsValid(ref Color)) // Junior haxor
                {
                    Core.Logging.Log("Session " + Session.ID + " [username: " + Session.User.Username + "] tried to assign an invalid color to a post.it item. Session destroyed and IP address blacklisted.", Woodpecker.Core.Logging.logType.haxEvent);
                    Engine.Net.Game.blackListIpAddress(Session.ipAddress);
                    Engine.Sessions.destroySession(Session.ID);
                    return;
                }

                string Message = Request.Content.Substring(idStringLength + 7);
                if (Message.Length > 684)
                    Message = Message.Substring(0, 684); // Truncate message
                stringFunctions.filterVulnerableStuff(ref Message, false);

                Session.roomInstance.setPostItData(itemID, Color, ref Message);
            }
        }
        /// <summary>
        /// 85 - "AU"
        /// </summary>
        public void REMOVEITEM()
        {
            if (Session.roomInstance.sessionHasFlatAdmin(Session.ID) || Session.User.hasFuseRight("fuse_remove_stickies"))
            {
                int itemID = int.Parse(Request.Content);
                Session.roomInstance.removePostIt(itemID);
            }
        }
    }
}

namespace Woodpecker.Game.Rooms.Instances
{
    public partial class roomInstance
    {
        #region Fields
        public const int maxPostItsPerRoom = 50;
        #endregion
        
        #region Methods
        /// <summary>
        /// Sets a new color and message for a post.it wall item, marks it as 'require update' and refreshes the item in the room.
        /// </summary>
        /// <param name="itemID">The database ID of the item.</param>
        /// <param name="Color">The new color value of the post.it.</param>
        /// <param name="Message">The new message of the post.it.</param>
        public void setPostItData(int itemID, string Color, ref string Message)
        {
            wallItem pItem = this.getWallItem(itemID);
            if (pItem != null && pItem.Definition.Behaviour.isPostIt)
            {
                pItem.customData = Color;
                pItem.postItMessage = Message;
                pItem.requiresUpdate = true;

                broadcoastWallItemStateUpdate(pItem);
            }
        }
        /// <summary>
        /// Removes a post.it wal item from the room and deletes it from the database.
        /// </summary>
        /// <param name="itemID">The database ID of the item.</param>
        public void removePostIt(int itemID)
        {
            wallItem pItem = this.getWallItem(itemID);
            if (pItem != null && pItem.Definition.Behaviour.isPostIt)
            {
                this.pickupWallItem(itemID, -1); // Remove from room and delete item
            }
        }
        #endregion
    }
}