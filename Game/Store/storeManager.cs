using System;
using System.Data;
using System.Collections.Generic;

using Woodpecker.Core;
using Woodpecker.Storage;
using Woodpecker.Sessions;
using Woodpecker.Net.Game.Messages;

using Woodpecker.Game.Users.Roles;
using Woodpecker.Game.Items;
using Woodpecker.Game.Items.Pets;

namespace Woodpecker.Game.Store
{
    public class storeManager
    {
        #region Fields
        /// <summary>
        /// A Dictionary collection holding the storeCataloguePage objects representing the pages of the virtual item shop, the indexname of the page is the key of the collection entries.
        /// </summary>
        private Dictionary<string, storeCataloguePage> Pages;
        /// <summary>
        /// A Dictionary collection holding the storeCatalogueSale objects representing the sales of the virtual item shop, the salecode is the key of the collection entries.
        /// </summary>
        private Dictionary<string, storeCatalogueSale> Sales;
        /// <summary>
        /// A string array with the attribute names for every page. This field is read only.
        /// </summary>
        private readonly string[] pageAttributes = new string[] { "name_index", "name", "layout", "label_pick", "img_headline", "img_teasers", "body", "label_extra_s" };
        #endregion

        #region Methods
        #region User credit log etc
        /// <summary>
        /// Writes a new row in the users_creditlog table of the database, with the user ID of a user, the type of purchase/action and the activity. (+100, -100 etc) The current datetime is inserted aswell. This record can be viewed by the user via the 'Transactions' button in the Purse.
        /// </summary>
        /// <param name="userID">The database ID of the user to log the action for.</param>
        /// <param name="Type">The type of the action. (win_voucher, club_habbo etc)</param>
        /// <param name="Activity">The activity of the action. (+100, -100 etc)</param> 
        public void logTransaction(int userID, string Type, int Activity)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.addParameterWithValue("type", Type);
            Database.addParameterWithValue("activity", Activity);
            Database.Open();
            if (Database.Ready)
            {
                Database.runQuery("INSERT INTO users_creditlog(userid,moment,type,activity) VALUES (@userid,NOW(),@type,@activity)");
            }
        }
        /// <summary>
        /// Gets the transactions (credit log) of a user on given user session and sends it to the session's game connection.
        /// </summary>
        /// <param name="Session">The Woodpecker.Sessions.Session object to get the transaction for and to send the message to.</param>
        public void sendTransactions(ref Session Session)
        {
            if (Session.User == null)
                return;

            serverMessage Message = new serverMessage(209); // "CQ"
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", Session.User.ID);

            Database.Open();
            if (Database.Ready)
            {
                DataTable creditLogData = Database.getTable("SELECT moment,type,activity FROM users_creditlog WHERE userid = @userid LIMIT 50");
                foreach (DataRow dRow in creditLogData.Rows)
                {
                    DateTime Moment = (DateTime)dRow["moment"];
                    Message.appendTabbedValue(Moment.ToString("dd/MM/yyyy"));
                    Message.appendTabbedValue(Moment.ToString("hh:mm"));
                    Message.appendTabbedValue(dRow["activity"].ToString());
                    Message.appendTabbedValue("0");
                    Message.appendTabbedValue("");
                    Message.Append(dRow["type"].ToString());
                    Message.appendChar(13);
                }
            }

            Session.gameConnection.sendMessage(Message);
        }
        /// <summary>
        /// Tries to redeem a credit/item voucher for a user session.
        /// </summary>
        /// <param name="Session">The Woodpecker.Sessions.Session object to redeem the voucher with.</param>
        /// <param name="Code">The vouchercode the user entered.</param>
        public void redeemVoucher(ref Session Session, string Code)
        {
            serverMessage Response = new serverMessage();
            Database Database = new Database(false, false);
            Database.addParameterWithValue("code", Code);

            Database.Open();
            if (Database.Ready)
            {
                DataRow dRow = Database.getRow("SELECT type,value FROM users_vouchers WHERE code = @code AND ISNULL(redeemer_userid)");
                if (dRow != null) // Voucher found
                {
                    // Mark voucher as redeemed
                    Database.addParameterWithValue("userid", Session.User.ID);
                    Database.runQuery("UPDATE users_vouchers SET redeemer_userid = @userid WHERE code = @code");
                    Database.Close();

                    string Type = (string)dRow["type"];
                    if (Type == "credits")
                    {
                        int Credits = int.Parse(dRow["value"].ToString());
                        Session.User.Credits += Credits;
                        Session.User.updateValueables();
                        this.logTransaction(Session.User.ID, "win_voucher", Credits);

                        Session.refreshCredits();
                    }
                    else if (Type == "item")
                    {
                        string[] Items = ((string)dRow["value"]).Split(';');

                    }

                    // Success!
                    Response.Initialize(212); // "CT"
                    Session.gameConnection.sendMessage(Response);
                    return;
                }
                else
                {
                    // Error 1! (not found)
                    Response.Initialize(213); // "CU"
                    Response.Append(1);
                }
                Session.gameConnection.sendMessage(Response);
            }
        }
        public bool purchaseSubscription(ref Session Session, string Subscription, int Choice)
        {
            if (Subscription != "club_habbo")
                return false;

            int Cost = 0;
            int Months = 0;
            if (Choice == 1)
            {
                Cost = 25;
                Months = 1;
            }
            else if (Choice == 2)
            {
                Cost = 60;
                Months = 3;
            }
            else if (Choice == 3)
            {
                Cost = 105;
                Months = 6;
            }
            else
                return false;

            if (Cost > Session.User.Credits)
                return false;
            else
            {
                for (int i = 1; i <= Months; i++)
                {
                    if (Session.User.clubDaysLeft == 0)
                        Session.User.clubDaysLeft = 31;
                    else
                        Session.User.clubMonthsLeft++;
                }

                this.logTransaction(Session.User.ID, "club_habbo", -Cost);
                Session.User.Credits -= Cost;
                Session.User.updateValueables();
                Session.User.updateClub(true);

                return true;
            }
        }
        #endregion

        #region Item store
        /// <summary>
        /// Initializes the sales sold in the catalogue.
        /// </summary>
        public void loadSales()
        {
            if (this.Sales != null)
                this.Sales.Clear();
            this.Sales = new Dictionary<string, storeCatalogueSale>();

            Database dbClient = new Database(true, false);
            if (dbClient.Ready)
            {
                foreach (DataRow dSale in dbClient.getTable("SELECT * FROM store_catalogue_sales").Rows)
                {
                    storeCatalogueSale pSale = new storeCatalogueSale((string)dSale["salecode"], (int)dSale["price"]); // Create blank sale object
                    bool isPackage = (
                        (dSale["ispackage"].ToString() == "true")
                        && dSale["package_name"] != DBNull.Value
                        && dSale["package_description"] != DBNull.Value);

                    if (isPackage)
                    {
                        pSale.setPackage((string)dSale["package_name"], (string)dSale["package_description"]);
                        foreach (DataRow dPackageItem in dbClient.getTable("SELECT definitionid,amount,specialspriteid FROM store_catalogue_sales_packages WHERE salecode = '" + pSale.saleCode + "'").Rows)
                        {
                            itemDefinition pItemDefinition = Engine.Game.Items.getItemDefinition((int)dPackageItem["definitionid"]);
                            if (pItemDefinition != null)
                                pSale.addPackageItem(pItemDefinition, (int)dPackageItem["amount"], (int)dPackageItem["specialspriteid"]);
                        }
                    }
                    else
                    {
                        itemDefinition pItemDefinition = Engine.Game.Items.getItemDefinition((int)dSale["item_definitionid"]);
                        if (pItemDefinition != null)
                            pSale.setItem(pItemDefinition, (int)dSale["item_specialspriteid"]);
                    }
                    this.Sales.Add(pSale.saleCode, pSale);
                }
                dbClient.Close();
            }
        }
        /// <summary>
        /// Initializes the store catalogue pages.
        /// </summary>
        public void loadCataloguePages()
        {
            Database dbClient = new Database(true, true);
            if (!dbClient.Ready)
            {
                Logging.Log("Failed to load store catalogue pages, database was not contactable!", Logging.logType.commonError);
                return;
            }

            this.Pages = new Dictionary<string,storeCataloguePage>(); // New collection
            foreach (DataRow pageData in dbClient.getTable("SELECT * FROM store_catalogue_pages ORDER BY orderid ASC").Rows)
            {
                storeCataloguePage pPage = new storeCataloguePage();
                pPage.ID = (int)pageData["id"];
                pPage.setMinimumAccessRole(Engine.Game.Roles.parseRoleFromString(pageData["minrole"].ToString()));

                // Set attributes for page
                foreach(string szAttribute in this.pageAttributes)
                    pPage.setAttribute(szAttribute, pageData[szAttribute]);

                if (pageData["label_extra_t"] != DBNull.Value)
                {
                    bool skip = false;
                    string[] extraTypedData = pageData["label_extra_t"].ToString().Split(Environment.NewLine.ToCharArray());
                    foreach (string szTypedData in extraTypedData)
                    {
                        if (!skip)
                        {
                            string ID = szTypedData.Substring(0, szTypedData.IndexOf(':'));
                            string szData = szTypedData.Substring(ID.ToString().Length + 1);

                            pPage.setAttribute("label_extra_t_" + ID, szData);
                        }
                        skip = !skip;
                    }
                }

                pPage.initializeSales();
                this.Pages.Add(pPage.getStringAttribute("name_index"), pPage);
            }
        }
        /// <summary>
        /// Tries to return the storeCataloguePage of a given page ID. If the page isn't found in the collection, then null is returned.
        /// </summary>
        /// <param name="pageID">The database ID of the catalogue page to retrieve.</param>
        /// <returns></returns>
        public storeCataloguePage getCataloguePage(int pageID)
        {
            if (this.Pages != null)
            {
                foreach(storeCataloguePage lPage in this.Pages.Values)
                {
                    if (lPage.ID == pageID)
                        return lPage;
                }
            }

            return null;
        }
        /// <summary>
        /// Tries to return the storeCataloguePage of a given page index name. If the page isn't found in the collection, then null is returned.
        /// </summary>
        /// <param name="indexName">The index name of the storeCataloguePage to retrieve.</param>
        public storeCataloguePage getCataloguePage(string indexName)
        {
            if (this.Pages != null && this.Pages.ContainsKey(indexName))
                return this.Pages[indexName];
            else
                return null;
        }
        /// <summary>
        /// Returns an array of the type storeCataloguePage with all the catalogue pages in the virtual store.
        /// </summary>
        /// <returns></returns>
        public storeCataloguePage[] getCataloguePages()
        {
            storeCataloguePage[] tmp = new storeCataloguePage[this.Pages.Count];
            this.Pages.Values.CopyTo(tmp, 0);

            return tmp;
        }
        /// <summary>
        /// Returns the storeCatalogueSale object of a given sale code. If the sale is not found, then null is returned.
        /// </summary>
        /// <param name="saleCode">The sale code of the sale to retrieve.</param>
        public storeCatalogueSale getSale(string saleCode)
        {
            try { return this.Sales[saleCode]; }
            catch { return null; }
        }

        public void requestSaleShipping(int receivingUserID, string saleCode, bool isNewPurchase, bool purchaseAsPresent, string presentNote, string customData)
        {
            storeCatalogueSale pSale = this.getSale(saleCode);
            if (pSale == null)
            {
                Logging.Log("Failed to purchase sale '" + saleCode + "' for user " + receivingUserID + ", the requested sale ('" + saleCode + "') was not found!", Logging.logType.commonWarning);
                return;
            }

            List<stripItem> shippedItems = new List<stripItem>();
            if (purchaseAsPresent)
            {
                stripItem presentBox = this.createPresent(receivingUserID, saleCode, presentNote, customData);
                if (presentBox != null)
                    shippedItems.Add(presentBox);
                else
                    return;
            }
            else
            {
                int itemIdOffset = Engine.Game.Items.getItemIdOffset();
                foreach (stripItem lItem in pSale.getItemInstances())
                {
                    lItem.ID = ++itemIdOffset;
                    lItem.ownerID = receivingUserID;

                    #region Special events upon purchase
                    if (lItem.Definition.Behaviour.isTeleporter) // Teleporter, create linking teleporter
                    {
                        stripItem Teleporter2 = new stripItem();
                        Teleporter2.ID = ++itemIdOffset;
                        Teleporter2.ownerID = receivingUserID;
                        Teleporter2.Definition = lItem.Definition;
                        Teleporter2.teleporterID = lItem.ID;
                        lItem.teleporterID = Teleporter2.ID;

                        shippedItems.Add(Teleporter2);
                    }
                    else if (lItem.Definition.Behaviour.isPostIt)
                    {
                        lItem.customData = "20";
                    }
                    else if (lItem.Definition.Behaviour.isDecoration || lItem.Definition.Behaviour.isPrizeTrophy)
                    {
                        lItem.customData = customData;
                    }
                    else if (lItem.Definition.Behaviour.isRedeemable)
                    {
                        int creditValue = 0;
                        if (int.TryParse(lItem.Definition.Sprite.Split('_')[1], out creditValue))
                            lItem.customData = creditValue.ToString();
                    }
                    else if (lItem.Definition.Sprite == "nest")
                    {
                        string[] petData = customData.Split(Convert.ToChar(2));
                        string Name = petData[0];
                        char Type = char.Parse(petData[1]);
                        byte Race = byte.Parse(petData[2]);
                        string Color = petData[3];

                        Engine.Game.Items.createPet(lItem.ID, Name, Type, Race, Color);
                    }
                    #endregion

                    shippedItems.Add(lItem);
                }

                Engine.Game.Items.createItemInstances(shippedItems);
            }

            Session Receiver = Engine.Game.Users.getUserSession(receivingUserID);
            if (Receiver != null) // Receiver was online
            {
                Receiver.itemStripHandler.addHandItems(shippedItems);

                serverMessage Notification = new serverMessage();
                if (isNewPurchase)
                {
                    Notification.Initialize(67); // "AC"
                }
                else
                #region Open as present box
                {
                    stripItem displayItem = shippedItems[0];

                    Notification.Initialize(129); // "BA"
                    Notification.appendNewLineValue(displayItem.Definition.Sprite);

                    string displaySprite = displayItem.Definition.Sprite;
                    //if (displayItem.Definition.isPartialSprite && displayItem.customData != null)
                    //    displaySprite += " " + displayItem.customData;
                    Notification.appendNewLineValue(displaySprite);

                    if (!displayItem.Definition.Behaviour.isWallItem)
                    {
                        Notification.appendStripValue(displayItem.Definition.Length.ToString());
                        Notification.appendStripValue(displayItem.Definition.Width.ToString());
                        Notification.appendStripValue(displayItem.Definition.Color);
                    }
                }
                #endregion
                Receiver.gameConnection.sendMessage(Notification);
            }
        }
        public void openPresent(int presentID, int userID)
        {
            Database dbClient = new Database(false, true);
            dbClient.addParameterWithValue("presentid", presentID);

            dbClient.Open();
            if (dbClient.Ready)
            {
                DataRow dRow = dbClient.getRow(
                    "SELECT salecode,customdata FROM items_presents WHERE presentid = @presentid LIMIT 1;" +
                    "DELETE FROM items_presents WHERE presentid = @presentid LIMIT 1;");
                if (dRow != null) // Present found
                {
                    string saleCode = (string)dRow["salecode"];
                    string customData = null;
                    if(dRow["customdata"] != DBNull.Value)
                        customData = (string)dRow["customdata"];

                    requestSaleShipping(userID, saleCode, false, false, null, customData ?? "");
                }
            }
        }
        public stripItem createPresent(int receivingUserID, string saleCode, string Note, string customData)
        {
            int definitionID = Engine.Game.Items.getRandomPresentBoxDefinitionID();
            if (definitionID != 1)
            {
                stripItem presentBoxItem = Engine.Game.Items.createItemInstance(definitionID, receivingUserID, "!" + Note);
                
                Database dbClient = new Database(false, true);
                dbClient.addParameterWithValue("itemid", presentBoxItem.ID);
                dbClient.addParameterWithValue("salecode", saleCode);
                if (customData == "")
                    dbClient.addParameterWithValue("customdata", DBNull.Value);
                else
                    dbClient.addParameterWithValue("customdata", customData);

                dbClient.Open();
                if (dbClient.Ready)
                    dbClient.runQuery("INSERT INTO items_presents VALUES (@itemid,@salecode,@customdata)");

                return presentBoxItem;
            }
            else
            {
                Logging.Log("Failed to create presentbox for sale '" + saleCode + "', one of the present box definitions is missing!", Logging.logType.commonError);
                return null;
            }
        }
        public void deliverItemsToSession(int userID, List<stripItem> Items, bool Refresh)
        {
            Session Receiver = Engine.Game.Users.getUserSession(userID);
            if (Receiver != null) // Receiver was online
            {
                Receiver.itemStripHandler.addHandItems(Items);
                if (Refresh)
                {
                    serverMessage Message = new serverMessage(101); // "Ae"
                    Receiver.gameConnection.sendMessage(Message);
                }
            }
        }
        #endregion

        #endregion
    }
}
