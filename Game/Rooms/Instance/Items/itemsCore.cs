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
        /// 67 - "AC"
        /// </summary>
        public void ADDSTRIPITEM()
        {
            if (Session.roomInstance.sessionHasFlatAdmin(Session.ID)) // Can pickup items
            {
                string[] removalData = Request.Content.Split(' ');
                int itemID = int.Parse(removalData[2]);

                stripItem returnItem = null;
                if (removalData[1] == "item") // Wall item
                    returnItem = Session.roomInstance.pickupWallItem(itemID, Session.User.ID);
                else if (removalData[1] == "stuff") // Floor item
                    returnItem = Session.roomInstance.pickupFloorItem(itemID, Session.User.ID);

                if (returnItem != null) // Successfully picked up to hand
                {
                    Session.itemStripHandler.addHandItem(returnItem);
                    Session.sendHandStrip("update");
                }
            }
        }
        /// <summary>
        /// 90 - "AZ"
        /// </summary>
        public void PLACESTUFF()
        {
            if (Session.roomInstance.sessionHasRights(Session.ID))
            {
                int itemID = int.Parse(Request.Content.Split(' ')[0]);

                stripItem pItem = Session.itemStripHandler.getHandItem(itemID);
                if (pItem == null) // Item not in hand
                    return;

                bool Success = false;
                if (pItem.Definition.Behaviour.isWallItem)
                {
                    if (pItem.Definition.Behaviour.isDecoration) // Can't place down decoration items (floor and wallpaper)
                        return;

                    string wallPosition = Request.Content.Substring(itemID.ToString().Length + 1);
                    Engine.Game.Items.correctWallItemPosition(ref wallPosition);
                    if (wallPosition == null) // Invalid wall position
                        return;

                    if (pItem.Definition.Behaviour.isPostIt) // Sticky pad
                    {
                        int padSize = int.Parse(pItem.customData) - 1;
                        if (padSize <= 0) // Pad is empty now, remove and delete
                            Session.itemStripHandler.removeHandItem(pItem.ID, true);
                        else // Decrease pad size
                        {
                            pItem.customData = padSize.ToString();
                            Engine.Game.Items.setItemCustomData(pItem.ID, pItem.customData);
                        }

                        pItem = Engine.Game.Items.createItemInstance(pItem.Definition.ID, Session.roomInstance.Information.ownerID, "FFFF33");
                    }

                    if (pItem != null)
                        Success = Session.roomInstance.placeWallItem(pItem, wallPosition);
                }
                else
                {
                    string[] locationData = Request.Content.Split(' ');
                    byte X = byte.Parse(locationData[1]);
                    byte Y = byte.Parse(locationData[2]);
                    //byte Rotation = byte.Parse(locationData[3]);
                    Success = Session.roomInstance.moveFloorItem(itemID, pItem, X, Y, 0);
                }

                if (Success && !pItem.Definition.Behaviour.isPostIt)
                    Session.itemStripHandler.removeHandItem(itemID, false);
            }
        }
    }
}

namespace Woodpecker.Game.Rooms.Instances
{
    public partial class roomInstance
    {
        #region Fields

        #endregion

        #region Methods
        /// <summary>
        /// Initializes all the item instances and adds them to the collections for items.
        /// </summary>
        private void loadItems()
        {
            if (this.floorItems != null)
                this.floorItems.Clear();
            if (this.wallItems != null)
                this.wallItems.Clear();
            if (this.roomPets != null)
                this.roomPets.Clear();
            if (this.roomBots != null)
                this.roomBots.Clear();

            this.floorItems = new List<floorItem>();
            if (this.Information.isUserFlat)
            {
                this.wallItems = new List<wallItem>();
                this.roomPets = new List<roomPet>();
            }
            this.roomBots = new List<roomUser>();

            Database dbClient = new Database(false, true);
            dbClient.addParameterWithValue("roomid", this.roomID);

            dbClient.Open();
            if (!dbClient.Ready)
                return; // Too bad

            foreach (DataRow dRow in dbClient.getTable("SELECT * FROM items WHERE roomid = @roomid").Rows)
            {
                itemDefinition Definition = Engine.Game.Items.getItemDefinition((int)dRow["definitionid"]);
                if (Definition == null) // Invalid item
                    continue;

                int itemID = (int)dRow["id"];
                string customData = null;
                if (dRow["customdata"] != DBNull.Value)
                    customData = (string)dRow["customdata"];

                if (Definition.Behaviour.isWallItem)
                {
                    wallItem pItem = new wallItem();
                    pItem.ID = itemID;
                    pItem.Definition = Definition;
                    pItem.roomID = this.roomID;
                    pItem.ownerID = this.Information.ownerID;
                    pItem.customData = customData;
                    pItem.wallPosition = (string)dRow["wallposition"];
                    if (Definition.Behaviour.isPostIt)
                        pItem.postItMessage = (string)dRow["postit_message"];

                    this.wallItems.Add(pItem);
                }
                else
                {
                    floorItem pItem = new floorItem();
                    pItem.ID = itemID;
                    pItem.Definition = Definition;
                    pItem.roomID = this.roomID;
                    pItem.ownerID = this.Information.ownerID;
                    pItem.customData = customData;
                    pItem.X = byte.Parse(dRow["x"].ToString());
                    pItem.Y = byte.Parse(dRow["y"].ToString());
                    pItem.Z = (float)dRow["z"];
                    pItem.Rotation = byte.Parse(dRow["rotation"].ToString());
                    if (pItem.Definition.Behaviour.isTeleporter) // Teleporter, initialize 'brother' ID
                        pItem.teleporterID = (int)dRow["teleporterid"];

                    this.floorItems.Add(pItem);
                }
            }
        }
        /// <summary>
        /// Saves all the current positions of the items in this room.
        /// </summary>
        private void saveItems()
        {
            if (this.floorItems != null)
            {
                lock (this.floorItems)
                {
                    foreach (floorItem lItem in this.floorItems)
                    {
                        if (lItem.requiresUpdate)
                        {
                            lItem.Update();
                        }
                    }
                }
            }

            if (this.wallItems != null)
            {
                lock (this.wallItems)
                {
                    foreach (wallItem lItem in this.wallItems)
                    {
                        if (lItem.requiresUpdate)
                        {
                            lItem.Update();
                        }
                    }
                }
            }

            if (this.roomPets != null)
            {
                lock (this.roomPets)
                {
                    foreach (roomPet lPet in this.roomPets)
                    {
                        lPet.Information.lastX = lPet.X;
                        lPet.Information.lastY = lPet.Y;
                        lPet.Information.Update(); // Update position of pet + current statistics
                    }
                }
            }
        }
        /// <summary>
        /// Unloads all the item instances from the collections and nulls the collections.
        /// </summary>
        private void unloadItems()
        {
            if (this.floorItems != null) {
                this.floorItems.Clear();
                this.floorItems = null;
            }

            if (this.wallItems != null) {
                this.wallItems.Clear();
                this.wallItems = null;
            }

            if (this.roomPets != null) {
                this.roomPets.Clear();
                this.roomPets = null;
            }

            if (this.roomBots != null) {
                this.roomBots.Clear();
                this.roomBots = null;
            }
        }
        #endregion
    }
}
