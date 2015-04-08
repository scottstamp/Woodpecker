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
        /// 73 - "AI"
        /// </summary>
        public void MOVESTUFF()
        {
            if (Session.roomInstance.sessionHasRights(Session.ID))
            {
                string[] locationData = Request.Content.Split(' ');
                int itemID = int.Parse(locationData[0]);
                byte newX = byte.Parse(locationData[1]);
                byte newY = byte.Parse(locationData[2]);
                byte newRotation = byte.Parse(locationData[3]);

                Session.roomInstance.moveFloorItem(itemID, null, newX, newY, newRotation);
            }
        }
        /// <summary>
        /// 74 - "AJ"
        /// </summary>
        public void SETSTUFFDATA()
        {
            string[] Parameters = this.Request.Parameters;
            int itemID = int.Parse(Parameters[0]);
            Session.roomInstance.setFloorItemData(itemID, Session.ID, Parameters[1]);
        }
        /// <summary>
        /// 78 - "AN"
        /// </summary>
        public void PRESENTOPEN()
        {
            if (Session.roomInstance.sessionHasFlatAdmin(Session.ID))
            {
                int itemID = int.Parse(Request.Content);
                floorItem pItem = Session.roomInstance.getFloorItem(itemID);
                if (pItem != null)
                {
                    Session.roomInstance.pickupFloorItem(itemID, -1);
                    Engine.Game.Store.openPresent(itemID, Session.User.ID);

                    Response.Initialize(101); // "Ae"
                    sendResponse();
                }
            }
        }
        /// <summary>
        /// 99 - "Ac"
        /// </summary>
        public void REMOVESTUFF()
        {
            if (Session.roomInstance.sessionHasFlatAdmin(Session.ID))
            {
                int itemID = int.Parse(Request.Content);
                Session.roomInstance.pickupFloorItem(itemID, -1);
                Session.itemStripHandler.removeHandItem(itemID, false);
            }
        }
        /// <summary>
        /// 183 - "Bw"
        /// </summary>
        public void CONVERT_FURNI_TO_CREDITS()
        {
            if (Session.roomInstance.sessionHasFlatAdmin(Session.ID))
            {
                int itemID = Request.getNextWiredParameter();
                floorItem pItem = Session.roomInstance.getFloorItem(itemID);
                if (pItem != null && pItem.Definition.Behaviour.isRedeemable) // Valid item
                {
                    Session.roomInstance.pickupFloorItem(itemID, -1);

                    int creditsValue = 0;
                    if (int.TryParse(pItem.customData, out creditsValue))
                    {
                        Session.User.Credits += creditsValue;
                        Session.refreshCredits();
                        Session.User.updateValueables();
                    }
                }
            }
        }
    }
}

namespace Woodpecker.Game.Rooms.Instances
{
    public partial class roomInstance
    {
        #region Fields
        /// <summary>
        /// A List collection with floorItem objects as values. This collection holds all the active floor items in the room instance.
        /// </summary>
        private List<floorItem> floorItems;
        #endregion

        #region Methods
        /// <summary>
        /// Returns a string with all the string representations of all the floor items in this room instance.
        /// </summary>
        /// <param name="publicSpaceItems">Supply true to return the public space items, false for user items.</param>
        public string getFloorItems(bool publicSpaceItems)
        {
            fuseStringBuilder FSB = new fuseStringBuilder();
            if (!(publicSpaceItems && this.Information.isUserFlat))
            {
                if (!publicSpaceItems)
                    FSB.appendWired(this.floorItems.Count);

                lock (this.floorItems)
                {
                    foreach (floorItem lItem in this.floorItems)
                    {
                        FSB.Append(lItem.ToString());
                    }
                }
            }

            return FSB.ToString();
        }

        #region Collection management
        /// <summary>
        /// Returns true if this room instance contains a floor item with a given ID.
        /// </summary>
        /// <param name="itemID">The database ID of the item to check.</param>
        public bool containsFloorItem(int itemID)
        {
            foreach (floorItem lItem in this.floorItems)
            {
                if (lItem.ID == itemID)
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Tries to return a floor item with a given ID from the floor item collection. If the item is not in the collection, then null is returned.
        /// </summary>
        /// <param name="itemID">The database ID of the item to get.</param>
        public floorItem getFloorItem(int itemID)
        {
            foreach (floorItem lItem in this.floorItems)
            {
                if (lItem.ID == itemID)
                    return lItem;
            }

            return null;
        }
        public List<floorItem> getFloorItems(int X, int Y)
        {
            List<floorItem> Items = new List<floorItem>();
            foreach (floorItem lItem in this.floorItems)
            {
                foreach (roomTile lTile in getAffectedTiles(lItem, true).Values)
                {
                    if (lTile.X == X && lTile.Y == Y) // Item is residing here
                        Items.Add(lItem);
                }
            }

            return Items;
        }
        public void unloadFloorItem(int itemID)
        {
            for (int x = 0; x < this.floorItems.Count; x++)
            {
                if (this.floorItems[x].ID == itemID)
                {
                    this.floorItems.RemoveAt(x);
                    return;
                }
            }
        }
        #endregion

        #region Generic item interaction
        public bool moveFloorItem(int itemID, stripItem handItemInstance, byte newX, byte newY, byte newRotation)
        {
            bool isNewPlacement = (handItemInstance != null);

            floorItem pItem = null;
            if (isNewPlacement)
            {
                if (this.containsFloorItem(itemID)) // L o l w u t
                    return false;

                pItem = new floorItem();
                pItem.ID = handItemInstance.ID;
                pItem.roomID = this.roomID;
                pItem.ownerID = this.Information.ownerID;
                pItem.Definition = handItemInstance.Definition;
                pItem.customData = handItemInstance.customData;
                pItem.teleporterID = handItemInstance.teleporterID;
            }
            else
            {
                pItem = this.getFloorItem(itemID);
                if (pItem == null)
                    return false;
            }

            Sessions.Session owner = Engine.Game.Users.getUserSession(pItem.ownerID);

            // Calculate new height
            float newZ = this.gridHeight[newX, newY];

            // Trying to stack on itself?
            if (pItem.Rotation == newRotation
                && pItem.X == newX
                && pItem.Y == newY
                && pItem.Z != newZ)
                if (owner != null && !owner.StackAnything) return false;

            // Get the tiles this item would reside on if this item was placed here, and verify if they exist
            Dictionary<int, roomTile> itemTiles = getAffectedTiles(pItem.Definition.Length, pItem.Definition.Width, newX, newY, newRotation, true);
            List<floorItem> itemsOnTile = getFloorItems(newX, newY);
            List<floorItem> itemsAffected = new List<floorItem>();
            List<floorItem> itemsComplete = new List<floorItem>();
            foreach (roomTile lTile in itemTiles.Values)
            {
                if (!this.tileExists(lTile.X, lTile.Y) || this.gridClientMap[lTile.X, lTile.Y] == 'x')
                    if (owner != null && !owner.StackAnything) return false; // Out of map range / invalid tile

                if (this.gridUnit[lTile.X, lTile.Y]) // Room unit on this tile
                {
                    if (newRotation == pItem.Rotation
                        && (lTile.X != pItem.X
                        || lTile.Y != pItem.Y))
                        if (owner != null && !owner.StackAnything) return false; // Can't rotate
                }

                List<floorItem> tileItems = getFloorItems(lTile.X, lTile.Y);
                if (tileItems.Count > 0)
                {
                    if (pItem.Definition.Behaviour.isRoller)
                    {
                        if (newX != pItem.X || newY != pItem.Y) // Not just rotation change
                            if (owner != null && !owner.StackAnything) return false; // Can't place these items on diff ones
                    }
                    itemsAffected.AddRange(tileItems);
                }
            }

            itemsComplete.AddRange(itemsOnTile);
            itemsComplete.AddRange(itemsAffected);

            // If item is at same position, keep height
            if (pItem.Z != newZ
                && pItem.X == newX
                && pItem.Y == newY)
                newZ = pItem.Z;

            if (!pItem.Definition.Behaviour.isRoller) // Rollers are never placed above the the floor
            {
                foreach (floorItem lItem in itemsComplete)
                {
                    if (lItem.ID != pItem.ID
                        && !lItem.Definition.Behaviour.canStackOnTop
                        || (lItem.Definition.Behaviour.isRoller && (pItem.Definition.Width > 1 || pItem.Definition.Length > 1))) // Placing on roller, but item exceeds 1x1 size
                        if (owner != null && !owner.StackAnything) return false; // Can't stack on item (in this way)
                }

                // Position item on top of stack
                foreach (floorItem lItem in itemsAffected)
                {
                    if (lItem.ID != pItem.ID && lItem.totalHeight > newZ)
                        newZ = lItem.totalHeight;
                }

                //if (newZ > 8) // Max height: 8
                //    newZ = 8;
            }

            byte oldX = pItem.X;
            byte oldY = pItem.Y;
            byte oldRotation = pItem.Rotation;

            pItem.X = newX;
            pItem.Y = newY;
            pItem.Z = newZ;
            pItem.Rotation = newRotation;

            if (owner != null && owner.StackAnything)
            {
                pItem.Z = Engine.Game.Users.getUserSession(pItem.ownerID).StackHeight;
            }

            if (isNewPlacement)
            {
                pItem.Update();
                this.floorItems.Add(pItem);
            }
            else
                pItem.requiresUpdate = true;

            this.generateFloorMap();
            this.broadcoastFloorItemMove(pItem, isNewPlacement);

            if (!isNewPlacement) // Item moves to different tiles, reset room units on current tiles
            {
                foreach (roomTile lTile in this.getAffectedTiles(pItem.Definition.Length, pItem.Definition.Width, oldX, oldY, oldRotation, true).Values)
                {
                    refreshRoomUnitOnTile(lTile.X, lTile.Y);
                }
            }

            foreach (roomTile lTile in getAffectedTiles(pItem, true).Values)
            {
                refreshRoomUnitOnTile(lTile.X, lTile.Y);
            }

            if (isNewPlacement) // Item is not just moved
            {
                if (pItem.Definition.Sprite == "nest") // Pet nest
                {
                    this.loadRoomPet(pItem.ID, newX, newY); // Load pet at new position
                }
            }

            this.broadcoastHeightmap();

            return true;
        }
        /// <summary>
        /// Tries to remove a flooritem from the room, updates the maps and users that currently interact with the item, broadcoasts the removal and updates the item, and optionally sends the item to the hand of a given user and returns a stripItem copy of the item.
        /// </summary>
        /// <param name="itemID">The database ID of the item to remove.</param>
        /// <param name="toUserID">The database ID of the user that should receive this item in his/her hand. Supply -1 to delete the item from the database.</param>
        public stripItem pickupFloorItem(int itemID, int toUserID)
        {
            floorItem pItem = this.getFloorItem(itemID);
            if(pItem == null)
                return null;

            Dictionary<int, roomTile> affectedTiles = this.getAffectedTiles(pItem, true);
            if (toUserID > 0) // Item picked up to hand of user
            {
                pItem.roomID = 0;
                pItem.ownerID = toUserID;
                pItem.X = 0;
                pItem.Y = 0;
                pItem.Z = 0;
                pItem.Rotation = 0;
                pItem.Update();
            }
            else
                Engine.Game.Items.deleteItemInstance(itemID);

            if (pItem.Definition.Sprite == "nest")
                this.unloadRoomPet(pItem.ID, true);

            this.unloadFloorItem(itemID);
            this.generateFloorMap();
            foreach (roomTile lTile in affectedTiles.Values)
            {
                refreshRoomUnitOnTile(lTile.X, lTile.Y);
            }
            this.broadcoastFloorItemRemoval(itemID);
            this.broadcoastHeightmap();
            
            return (stripItem)pItem;
        }
        /// <summary>
        /// Attempts to set custom data to a floor item, refreshes it in the room, flags it for 'requires database update' and performs optional actions regarding the map etc.
        /// </summary>
        /// <param name="itemID">The database ID of the item.</param>
        /// <param name="sessionID">The ID of the session who's room users requests this action.</param>
        /// <param name="Data">The new custom data for this floor item as a string.</param>
        public void setFloorItemData(int itemID, uint sessionID, string Data)
        {
            floorItem pItem = this.getFloorItem(itemID);
            if (pItem != null && pItem.Definition.canContainCustomData) // Valid item
            {
                if (pItem.Definition.Behaviour.requiresRightsForInteraction && !this.sessionHasRights(sessionID))
                    return; // Item only allows users with room rights to set the custom data, and this user does not have room rights

                if (pItem.Definition.Behaviour.requiresTouchingForInteraction) // Item requires room user to stand one tile removed from the item in order to interact
                {
                    roomUser pUser = this.getRoomUser(sessionID);
                    if (!this.tilesTouch(pItem.X, pItem.Y, pUser.X, pUser.Y))
                        return; // Tile of floor item and tile of room user do not 'touch'
                }

                if (!pItem.Definition.Behaviour.isDoor)
                {
                    if (pItem.Definition.Behaviour.customDataTrueFalse && Data != "TRUE" && Data != "I")
                        Data = "FALSE";
                    else if (pItem.Definition.Behaviour.customDataNumericOnOff && Data != "2")
                        Data = "1";
                    else if (pItem.Definition.Behaviour.customDataOnOff && Data != "ON")
                        Data = "OFF";
                    else if (pItem.Definition.Behaviour.customDataNumericState) // Verify numeric state
                    {
                        if (Data != "x") // EXCEPTION: 00-99 for hockey light = 'x'
                        {
                            int stateID = 0;
                            if (int.TryParse(Data, out stateID) && stateID >= 0 && stateID <= 99)
                                Data = stateID.ToString();
                            else
                                return; // Invalid numeric state
                        }
                    }
                }
                else
                {
                    if (Data == "C") // Request to close door
                    {
                        foreach (roomTile lTile in getAffectedTiles(pItem, true).Values)
                        {
                            if (gridUnit[lTile.X, lTile.Y])
                                return; // User is on tile, can't close door
                        }
                    }
                    else
                        Data = "O";
                    pItem.customData = Data;
                    generateFloorMap();
                }

                pItem.customData = Data;
                if (!pItem.Definition.Behaviour.customDataTrueFalse)
                    pItem.requiresUpdate = true;

                this.broadcoastFloorItemCustomDataUpdate(pItem);
            }
        }
        #endregion

        #region Event broadcoasting
        /// <summary>
        /// Broacoasts the placement of a floor item to all room users.
        /// </summary>
        /// <param name="pItem">The floorItem instance of the wall item that is placed.</param>
        /// <param name="isPlacement">True if this item is new in the room, false otherwise.</param>
        private void broadcoastFloorItemMove(floorItem pItem, bool isPlacement)
        {
            serverMessage Message = new serverMessage();
            if (isPlacement)
                Message.Initialize(93); // "A]"
            else
                Message.Initialize(95); // "A_"
            Message.Append(pItem.ToString());

            this.sendMessage(Message);
        }
        /// <summary>
        /// Broadcoasts the removal of a floor item to all room users.
        /// </summary>
        /// <param name="itemID">The database ID of the floor item that is removed.</param>
        private void broadcoastFloorItemRemoval(int itemID)
        {
            serverMessage Message = new serverMessage(94); // "A^"
            Message.Append(itemID);
            this.sendMessage(Message);
        }
        /// <summary>
        /// Broadcoasts the custom data update of a floor item to all room users.
        /// </summary>
        /// <param name="pItem">The floorItem instance of the updated floor item.</param>
        private void broadcoastFloorItemCustomDataUpdate(floorItem pItem)
        {
            serverMessage Message = new serverMessage(88); // "AX"
            Message.appendClosedValue(pItem.ID.ToString());
            Message.appendClosedValue(pItem.customData);
            Message.appendClosedValue(null); // TODO: pet interaction with toy/goodie
            this.sendMessage(Message);
        }
        public void broadcoastHeightmap()
        {
            serverMessage Message = new serverMessage(31); // "@_"
            Message.Append(this.getClientFloorMap());
            this.sendMessage(Message);
        }
        #endregion
        #endregion
    }
}