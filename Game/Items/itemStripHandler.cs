using System;
using System.Data;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

using Woodpecker.Storage;
using Woodpecker.Sessions;
using Woodpecker.Specialized.Text;

namespace Woodpecker.Game.Items
{
    /// <summary>
    /// Provides management and methods for items that a user has in his/her inventory. Also provides methods for trading items with other sessions.
    /// </summary>
    public class itemStripHandler
    {
        #region Fields
        #region General
        /// <summary>
        /// The database ID of the user this item strip belongs to.
        /// </summary>
        private int userID;
        #endregion

        #region Hand (item inventory)
        /// <summary>
        /// The page ID the hand is currently on.
        /// </summary>
        private byte handStripPageIndex;
        /// <summary>
        /// A List of the type Woodpecker.Game.Items.stripItem holding all the stripItem instances that a user currently has in his/her Hand. (item inventory)
        /// </summary>
        private List<stripItem> handItems;
        /// <summary>
        /// The total amount of items in the hand as an integer.
        /// </summary>
        public int handItemCount
        {
            get { return this.handItems.Count; }
        }
        #endregion

        #region Item trading
        /// <summary>
        /// The session ID of the session where this user is currently trading with.
        /// </summary>
        public uint tradePartnerSessionID;
        /// <summary>
        /// Holds the current 'user accepts trade' state.
        /// </summary>
        public bool tradeAccept;
        /// <summary>
        /// A List of the type integer holding all the IDs of the items from this user's hand collection that are currently being offered in a trade.
        /// </summary>
        private List<int> tradeOfferItemIDs;
        /// <summary>
        /// True if this user's hand strip is currently in trade with another user.
        /// </summary>
        public bool isTrading
        {
            get { return this.tradePartnerSessionID > 0; }
        }
        /// <summary>
        /// The total amount of items that this user is currently offering in a trade with another user.
        /// </summary>
        public int tradeOfferItemCount
        {
            get
            {
                if (this.tradeOfferItemIDs == null)
                    return 0;
                else
                    return this.tradeOfferItemIDs.Count;
            }
        }
        #endregion
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a item strip handler for a given user and loads the hand items.
        /// </summary>
        /// <param name="userID">The database ID of the user to load this item strip handler for.</param>
        public itemStripHandler(int userID)
        {
            this.userID = userID;
            this.handItems = new List<stripItem>();
            this.loadHandItems();
        }
        #endregion

        #region Methods
        #region Hand
        /// <summary>
        /// Loads all the items this user currently has in her/hand and stores them in the item collection.
        /// </summary>
        public void loadHandItems()
        {
            this.handItems.Clear();

            Database dbClient = new Database(false, true);
            dbClient.addParameterWithValue("userid", this.userID);

            dbClient.Open();
            if (dbClient.Ready)
            {
                foreach (DataRow dItem in dbClient.getTable("SELECT id,definitionid,customdata,teleporterid FROM items WHERE ownerid = @userid AND roomid = '0' ORDER BY id ASC").Rows)
                {
                    stripItem pItem = new stripItem();
                    pItem.ID = (int)dItem["id"];
                    pItem.ownerID = this.userID;
                    pItem.Definition = Engine.Game.Items.getItemDefinition((int)dItem["definitionid"]);
                    if (dItem["customdata"] != DBNull.Value)
                        pItem.customData = (string)dItem["customdata"];
                    else
                        pItem.customData = null;
                    if (pItem.Definition.Behaviour.isTeleporter)
                        pItem.teleporterID = (int)dItem["teleporterid"];

                    this.handItems.Add(pItem);
                }
            }
        }
        /// <summary>
        /// Saves all the items in the hand that have been marked for update. (items that are new in the hand etc)
        /// </summary>
        public void saveHandItems()
        {
            Database dbClient = null;
            MySqlParameter vchrCustomData = null;
            foreach (stripItem lItem in this.handItems)
            {
                if (lItem.requiresUpdate)
                {
                    #region Create database connection
                    if (dbClient == null) // No database connection yet
                    {
                        dbClient = new Database(false, true);
                        dbClient.addParameterWithValue("userid", this.userID);
                        vchrCustomData = new MySqlParameter("customdata", MySqlDbType.VarChar);
                        dbClient.addRawParameter(vchrCustomData);
                        dbClient.Open();

                        if (!dbClient.Ready) // Can't use this database connection for some reason
                            return;
                    }
                    #endregion

                    if (lItem.customData == null)
                        vchrCustomData.Value = DBNull.Value;
                    else
                        vchrCustomData.Value = lItem.customData;
                    dbClient.runQuery("UPDATE items SET ownerid = @userid,roomid = '0',customdata = @customdata WHERE id = '" + lItem.ID + "' LIMIT 1");
                }
            }
            if (dbClient != null)
                dbClient.Close();
        }
        /// <summary>
        /// Clears all hand items objects from the hand item collection.
        /// </summary>
        public void Clear()
        {
            this.handItems.Clear();
        }

        /// <summary>
        /// Modifies the internal hand page ID, by checking a given string.
        /// </summary>
        /// <param name="szTo">The action to perform on the index.</param>
        public void changeHandStripPage(string szTo)
        {
            switch (szTo)
            {
                case "new":
                    this.handStripPageIndex = 0;
                    break;

                case "next":
                    this.handStripPageIndex++;
                    break;

                case "prev":
                    if (this.handStripPageIndex > 0)
                        this.handStripPageIndex--;
                    break;

                case "last":
                    this.handStripPageIndex = 255;
                    break;

                    // Other = 'update'
            }
        }
        /// <summary>
        /// Returns the item list of all the items in the hand ordered by ID.
        /// </summary>
        public string getHandItemCasts()
        {
            fuseStringBuilder Items = new fuseStringBuilder();
            int startID = 0;
            int endID = this.handItems.Count;

            if(this.handStripPageIndex == 255)
                this.handStripPageIndex = (byte)((endID - 1) / 9);
            calculateStripOffset:
            if (endID > 0)
            {
                startID = this.handStripPageIndex * 9;
                if(endID > (startID + 9))
                    endID = startID + 9;
                if(startID >= endID)
                {
                    this.handStripPageIndex--;
                    goto calculateStripOffset;
                }

                for (int stripSlotID = startID; stripSlotID < endID; stripSlotID++)
                {
                    Items.Append(this.handItems[stripSlotID].ToStripString(stripSlotID));
                }
            }

            return Items.ToString();
        }

        /// <summary>
        /// Adds a given strip item to the hand item collection.
        /// </summary>
        /// <param name="Item">The strip item to add.</param>
        public void addHandItem(stripItem Item)
        {
            this.handItems.Add(Item);
        }
        public void addHandItems(List<stripItem> Items)
        {
            this.handItems.AddRange(Items);
        }
        /// <summary>
        /// Removes an item from the hand item collection and optionally deletes it from the database.
        /// </summary>
        /// <param name="ID">The database ID of the item to remove from the hand.</param>
        /// <param name="deleteItem"></param>
        public void removeHandItem(int ID, bool deleteItem)
        {
            for(int i = 0; i < this.handItems.Count; i++)
            {
                if(this.handItems[i].ID == ID)
                {
                    this.handItems.RemoveAt(i);

                    if (deleteItem)
                        Engine.Game.Items.deleteItemInstance(ID);
                    break;
                }
            }
        }
        /// <summary>
        /// Returns true if the hand item collection currently contains a strip item with a given ID.
        /// </summary>
        /// <param name="ID">The database ID of the item to check.</param>
        public bool containsHandItem(int ID)
        {
            foreach (stripItem lItem in this.handItems)
            {
                if (lItem.ID == ID)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Tries to find a hand item with a given ID in the hand item collection and return it. If the item isn't found, null is returned.
        /// </summary>
        /// <param name="ID">The database ID of the item to retrieve.</param>
        public stripItem getHandItem(int ID)
        {
            foreach (stripItem lItem in this.handItems)
            {
                if (lItem.ID == ID)
                    return lItem;
            }

            return null;
        }
        #endregion

        #region Trading
        /// <summary>
        /// Prepares the inner collection etc of the item strip manager for an item trade with the user of a different session.
        /// </summary>
        /// <param name="partnerSessionID">The session ID of the session of the user that will be the trade partner.</param>
        public void initTrade(uint partnerSessionID)
        {
            if (this.isTrading)
                return;

            this.tradePartnerSessionID = partnerSessionID;
            this.tradeOfferItemIDs = new List<int>();
        }
        /// <summary>
        /// Disposes and resets all trading related fields in the item strip handler.
        /// </summary>
        public void stopTrade()
        {
            if (this.isTrading)
            {
                this.tradeOfferItemIDs.Clear();
                this.tradeOfferItemIDs = null;
                this.tradeAccept = false;
                this.tradePartnerSessionID = 0;
            }
        }

        /// <summary>
        /// Adds an item to the trade offer item ID collection and sets the 'accept trade' state to false. The item is only added if the item is indeed in the hand item collection.
        /// </summary>
        /// <param name="ID">The database ID of the item to add to the trade offer.</param>
        public void addItemToTradeOffer(int ID)
        {
            if (this.isTrading && this.containsHandItem(ID))
            {
                this.tradeOfferItemIDs.Add(ID);
                this.tradeAccept = false;
            }
        }
        /// <summary>
        /// Removes an item from the trade offer item ID collection and sets the 'accept trade' state to false.
        /// </summary>
        /// <param name="ID"></param>
        public void removeItemFromTradeOffer(int ID)
        {
            if (this.isTrading)
            {
                this.tradeOfferItemIDs.Remove(ID);
                this.tradeAccept = false;
            }
        }
        /// <summary>
        /// Returns true if an item with a given item ID is currently being offered in a trade with another user.
        /// </summary>
        /// <param name="ID">The database ID of the item to check.</param>
        public bool itemIsInTradeOffer(int ID)
        {
            if (this.isTrading)
            {
                foreach (int lID in this.tradeOfferItemIDs)
                {
                    if (lID == ID)
                        return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Returns a string with all the offered trade items represented as a string.
        /// </summary>
        public string getTradeOfferItems()
        {
            fuseStringBuilder FSB = new fuseStringBuilder();
            if (this.isTrading)
            {
                for (int slotID = 0; slotID < this.tradeOfferItemIDs.Count; slotID++)
                {
                    stripItem pItem = this.getHandItem(this.tradeOfferItemIDs[slotID]);
                    if (pItem != null)
                        FSB.Append(pItem.ToStripString(slotID));
                }
            }
            return FSB.ToString();
        }
        /// <summary>
        /// Generates the trade box for a given session and returns it as as string.
        /// </summary>
        /// <param name="Session">The Woodpecker.Sessions.Session object to generate the trade box string for.</param>
        public string generateTradeBox(Session Session)
        {
            fuseStringBuilder Box = new fuseStringBuilder();
            if (Session.itemStripHandler.isTrading)
            {
                Box.appendTabbedValue(Session.User.Username);
                Box.appendTabbedValue(Session.itemStripHandler.tradeAccept.ToString().ToLower());
                Box.Append(Session.itemStripHandler.getTradeOfferItems());
                Box.appendChar(13);
            }

            return Box.ToString();
        }
        /// <summary>
        /// Swaps the trade offer items of two trade partners and updates the database, hand item collection etc. The swap is aborted if the trade is invalid.
        /// </summary>
        /// <param name="partnerItemStrip">The itemStripHandler object of the trade partner session.</param>
        public void swapTradeOffers(itemStripHandler partnerItemStrip)
        {
            if (this.isTrading && partnerItemStrip.isTrading && (this.tradeOfferItemCount > 0 || partnerItemStrip.tradeOfferItemCount > 0)) // Can swap items
            {
                Database dbClient = new Database(true, false);
                if (!dbClient.Ready)
                    return;

                foreach (int myTradeOfferItemID in this.tradeOfferItemIDs)
                {
                    stripItem lItem = this.getHandItem(myTradeOfferItemID);
                    if (lItem == null)
                        return; // Trade invalid

                    this.removeHandItem(myTradeOfferItemID, false); // Remove from this item strip
                    partnerItemStrip.addHandItem(lItem); // Add to partner item strip
                    dbClient.runQuery("UPDATE items SET ownerid = '" + partnerItemStrip.userID + "' WHERE id = '" + lItem.ID + "' LIMIT 1"); // Update database
                }

                foreach (int partnerTradeOfferItemID in partnerItemStrip.tradeOfferItemIDs)
                {
                    stripItem lItem = partnerItemStrip.getHandItem(partnerTradeOfferItemID);
                    if (lItem == null)
                        return; // Trade invalid

                    partnerItemStrip.removeHandItem(partnerTradeOfferItemID, false); // Remove from partner item strip
                    this.addHandItem(lItem); // Add to this item strip
                    dbClient.runQuery("UPDATE items SET ownerid = '" + this.userID + "' WHERE id = '" + lItem.ID + "' LIMIT 1"); // Update database
                }

                dbClient.Close(); // Close database connection
            }
        }
        #endregion
        #endregion
    }
}
