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
        /// 214 - "CV"
        /// </summary>
        public void SETITEMSTATE()
        {
            int[] Parameters = this.Request.getNumericMixedParameters();
            Session.roomInstance.setWallItemState(Parameters[0], Session.ID, Parameters[1]);
        }
    }
}

namespace Woodpecker.Game.Rooms.Instances
{
    public partial class roomInstance
    {
        #region Fields
        /// <summary>
        /// A List collection with wallItem objects as values. This collection holds all the active wall items in the room instance.
        /// </summary>
        private List<wallItem> wallItems;
        #endregion

        #region Methods
        /// <summary>
        /// Returns a string with all the string representations of all the wall items in this room instance.
        /// </summary>
        /// <returns></returns>
        public string getWallItems()
        {
            if (this.Information.isUserFlat)
            {
                fuseStringBuilder FSB = new fuseStringBuilder();
                lock (this.wallItems)
                {
                    foreach (wallItem lItem in this.wallItems)
                    {
                        FSB.Append(lItem.ToString());
                        FSB.appendChar(13);
                    }
                }

                return FSB.ToString();
            }
            else
                return "";
        }

        #region Collection management
        /// <summary>
        /// Returns true if this room instance contains a wall item with a given ID.
        /// </summary>
        /// <param name="itemID">The database ID of the item to check.</param>
        public bool containsWallItem(int itemID)
        {
            foreach (wallItem lItem in this.wallItems)
            {
                if (lItem.ID == itemID)
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Tries to return a wall item with a given ID from the wall item collection. If the item is not in the collection, then null is returned.
        /// </summary>
        /// <param name="itemID">The database ID of the item to get.</param>
        public wallItem getWallItem(int itemID)
        {
            foreach (wallItem lItem in this.wallItems)
            {
                if (lItem.ID == itemID)
                    return lItem;
            }

            return null;
        }
        public void unloadWallItem(int itemID)
        {
            for (int x = 0; x < this.wallItems.Count; x++)
            {
                if (this.wallItems[x].ID == itemID)
                {
                    this.wallItems.RemoveAt(x);
                    return;
                }
            }
        }
        #endregion

        #region Generic item interaction
        /// <summary>
        /// Places a wall item in the room and makes it visible for all clients, and returns a boolean that holds true if the operation has succeeded.
        /// </summary>
        /// <param name="handItemInstance">The stripItem instance of the wall item that is being placed down.</param>
        /// <param name="Position">The new position of the wall item as a string.</param>
        public bool placeWallItem(stripItem handItemInstance, string Position)
        {
            if (this.containsWallItem(handItemInstance.ID))
                return false;

            wallItem pItem = new wallItem();
            pItem.ID = handItemInstance.ID;
            pItem.roomID = this.roomID;
            pItem.ownerID = handItemInstance.ownerID;
            pItem.Definition = handItemInstance.Definition;
            pItem.customData = handItemInstance.customData;
            pItem.wallPosition = Position;
            if (handItemInstance.Definition.Behaviour.isPostIt) // New post.it, set blank message data
            {
                pItem.postItMessage = String.Empty;
            }

            this.broadcoastWallItemPlacement(pItem);
            this.wallItems.Add(pItem);
            pItem.Update(); // Save position

            return true;
        }
        /// <summary>
        /// Tries to remove a wallitem from the room, broadcoasts the removal and updates the item, and optionally sends the item to the hand of a given user and returns a stripItem copy of the item with the out parameter.
        /// </summary>
        /// <param name="itemID">The database ID of the item to remove.</param>
        /// <param name="toUserID">The database ID of the user that should receive this item in his/her hand. Supply -1 to delete the item from the database.</param>
        /// <param name="handItemInstance">If toUserID > 0, then the stripItem copy of the removed item will be returned via the out parameter.</param>
        public stripItem pickupWallItem(int itemID, int toUserID)
        {
            wallItem pItem = this.getWallItem(itemID);
            if (pItem == null || (pItem.Definition.Behaviour.isPostIt && toUserID > 0))
                return null; // Can't pickup item to hand

            broadcoastWallItemRemoval(itemID);
            this.unloadWallItem(itemID);

            if (toUserID > 0) // Item picked up to hand of user
            {
                pItem.roomID = 0;
                pItem.ownerID = toUserID;
                pItem.wallPosition = null;
                pItem.Update();
            }
            else
                Engine.Game.Items.deleteItemInstance(itemID);

            if (toUserID > 0)
                return (stripItem)pItem;
            else
                return null;
        }
        /// <summary>
        /// Attempts to set the custom data for a wall item, refreshes the item in the room and flags the item as 'requires update'.
        /// </summary>
        /// <param name="itemID">The database ID of the wall item.</param>
        /// <param name="sessionID">The ID of the session who's room user requests a state change.</param>
        /// <param name="State">The new state as an integer.</param>
        public void setWallItemState(int itemID, uint sessionID, int State)
        {
            wallItem pItem = this.getWallItem(itemID);
            if (pItem != null && pItem.Definition.canContainCustomData) // Valid item
            {
                if (pItem.Definition.Behaviour.requiresRightsForInteraction && !this.sessionHasRights(sessionID))
                    return;  // This wall item requires rights for interaction and this session does not have room rights

                if (pItem.Definition.Behaviour.customDataNumericOnOff && State != 1 && State != 2)
                    return; // This wall item only accepts '1' or '2' for custom data, and the given number does not match

                pItem.customData = State.ToString();
                pItem.requiresUpdate = true;

                broadcoastWallItemStateUpdate(pItem);
            }
        }
        #endregion

        #region Event broadcoasting
        /// <summary>
        /// Broacoasts the placement of a wall item to all room users.
        /// </summary>
        /// <param name="pItem">The wallItem instance of the wall item that is placed.</param>
        private void broadcoastWallItemPlacement(wallItem pItem)
        {
            serverMessage Message = new serverMessage(83); // "AS"
            Message.Append(pItem.ToString());
            this.sendMessage(Message);
        }
        /// <summary>
        /// Broadcoasts the removal of a wall item to all room users.
        /// </summary>
        /// <param name="itemID">The database ID of the wall item that is removed.</param>
        private void broadcoastWallItemRemoval(int itemID)
        {
            serverMessage Message = new serverMessage(84); // "AT"
            Message.Append(itemID);
            this.sendMessage(Message);
        }
        /// <summary>
        /// Broadcoasts the state update of a given wall item to all active room users.
        /// </summary>
        /// <param name="pItem">The wallItem instance of the wall item that is updated.</param>
        private void broadcoastWallItemStateUpdate(wallItem pItem)
        {
            serverMessage Message = new serverMessage(85); // "AU"
            Message.Append(pItem.ToString());
            this.sendMessage(Message);
        }
        #endregion
        #endregion
    }
}
