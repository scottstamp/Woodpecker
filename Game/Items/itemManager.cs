using System;
using System.Data;
using System.Collections.Generic;

using MySql.Data.MySqlClient;
using Woodpecker.Core;
using Woodpecker.Storage;
using Woodpecker.Specialized.Text;
using Woodpecker.Game.Items.Pets;
using Woodpecker.Game.Items.Bots;

namespace Woodpecker.Game.Items
{
    /// <summary>
    /// Provides various functions for creating and disposing items and item definitions.
    /// </summary>
    public class itemManager
    {
        #region Fields
        /// <summary>
        /// Holds the loaded item definitions.
        /// </summary>
        private Dictionary<int, itemDefinition> mItemDefinitions = new Dictionary<int, itemDefinition>();
        private string mSpriteIndex;
        public string spriteIndex
        {
            get
            {
                return mSpriteIndex;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Loads all the item definitions from the database and stores them in the definition collection.
        /// </summary>
        public void loadDefinitions()
        {
            mItemDefinitions.Clear(); // Clear old definitions from collection
            fuseStringBuilder spriteIndex = new fuseStringBuilder();
            Logging.Log("Loading item definitions...");

            Database dbClient = new Database(true, true);
            if (dbClient.Ready)
            {
                List<string> includedSprites = new List<string>();
                DataRowCollection Definitions = dbClient.getTable("SELECT * FROM items_definitions ORDER BY id ASC").Rows;

                spriteIndex.appendWired(Definitions.Count);

                foreach (DataRow dRow in Definitions)
                {
                    int ID = (int)dRow["id"];
                    itemDefinition Definition = itemDefinition.Parse(dRow);
                    Definition.directoryID = (int)dRow["cast_directory"];
                    mItemDefinitions.Add(ID, Definition);

                    if (!includedSprites.Contains(Definition.Sprite))
                    {
                        spriteIndex.appendClosedValue(Definition.Sprite);
                        spriteIndex.appendWired(Definition.directoryID);
                        includedSprites.Add(Definition.Sprite);
                    }
                }

                mSpriteIndex = spriteIndex.ToString();
                Logging.Log("Loaded " + Definitions.Count + " item definitions.");
            }
            else
                Logging.Log("Failed to load the item definitions, database was not contactable!", Logging.logType.commonError);
        }

        /// <summary>
        /// Returns the current highest item ID in the items table of the database.
        /// </summary>
        public int getItemIdOffset()
        {
            Database dbClient = new Database(true, true);
            if (dbClient.Ready)
                return dbClient.getInteger("SELECT MAX(id) FROM items LIMIT 1");
            else
                return 0;
        }
        /// <summary>
        /// Creates an instance of a given item definition for a given user, and places it in the hand of that user.
        /// </summary>
        /// <param name="definitionID">The definition ID of the item to create an instance of.</param>
        /// <param name="ownerID">The database ID of the user that is the owner of this item.</param>
        /// <param name="customData">Optional. Custom data to set for the new item instance.</param>
        public int createItemInstanceReturnID(int definitionID, int ownerID, string customData)
        {
            itemDefinition pItemDefinition = getItemDefinition(definitionID);
            if (pItemDefinition == null)
                return -1;

            int ID = -1;
            Database Database = new Database(false, false);
            Database.addParameterWithValue("definitionid", definitionID);
            Database.addParameterWithValue("ownerid", ownerID);

            if (customData == null)
                Database.addParameterWithValue("customdata", DBNull.Value);
            else
                Database.addParameterWithValue("customdata", customData);

            Database.Open();
            if (Database.Ready)
            {
                Database.runQuery("INSERT INTO items(definitionid,ownerid,customdata) VALUES (@definitionid,@ownerid,@customdata)");
                ID = Database.getInteger("SELECT MAX(id) FROM items ORDER BY id DESC LIMIT 1");
                Database.Close();
            }

            return ID;
        }
        public stripItem createItemInstance(int definitionID, int ownerID, string customData)
        {
            int newID = this.createItemInstanceReturnID(definitionID, ownerID, customData);
            if (newID > 0)
            {
                stripItem ret = new stripItem();
                ret.ID = newID;
                ret.Definition = this.getItemDefinition(definitionID);
                ret.ownerID = ownerID;
                ret.customData = customData;

                return ret;
            }
            else
                return null;
        }
        public void createItemInstances(List<stripItem> pItems)
        {
            MySqlParameter paramCustomData = new MySqlParameter("customdata", MySqlDbType.VarChar);

            Database dbClient = new Database(false, false);
            dbClient.addRawParameter(paramCustomData);
            dbClient.Open();

            if (dbClient.Ready)
            {
                foreach (stripItem lItem in pItems)
                {
                    if (lItem.customData == null)
                        paramCustomData.Value = DBNull.Value;
                    else
                        paramCustomData.Value = lItem.customData;
                    dbClient.runQuery("INSERT INTO items(id,definitionid,ownerid,customdata,teleporterid) VALUES ('" + lItem.ID + "','" + lItem.Definition.ID + "','" + lItem.ownerID + "',@customdata,'" + lItem.teleporterID + "')");
                }
                dbClient.Close();
            }
        }
        public stripItem createItemFromDefinition(itemDefinition pDefinition, int specialSpriteID)
        {
            stripItem pItem = new stripItem();
            pItem.Definition = pDefinition;
            if (specialSpriteID > 0)
                pItem.customData = specialSpriteID.ToString();

            return pItem;
        }
        /// <summary>
        /// Deletes an item instance with a given ID from the database table.
        /// </summary>
        /// <param name="ID">The database ID of the item to delete.</param>
        public void deleteItemInstance(int ID)
        {
            Database dbClient = new Database(false, true);
            dbClient.addParameterWithValue("id", ID);

            dbClient.Open();
            if (dbClient.Ready)
                dbClient.runQuery
                    (
                    "DELETE FROM items WHERE id = @id LIMIT 1;" +
                    "DELETE FROM items_pets WHERE id = @id LIMIT 1;"
                    );

            // TODO: soundmachines, present content etc
        }

        public itemDefinition getItemDefinition(int ID)
        {
            try { return mItemDefinitions[ID]; }
            catch { return null; }
        }
        public itemDefinition getItemDefinitionByName(string name)
        {
            try
            {
                foreach (itemDefinition def in mItemDefinitions.Values)
                {
                    if (def.Sprite == name)
                        return def;
                }
                return null;
            }
            catch { return null; }
        }
        public Dictionary<int, itemDefinition> getItemDefinitionCollection()
        {
            return mItemDefinitions;
        }
        /// <summary>
        /// Returns an itemDefinition object for a random present box.
        /// </summary>
        public itemDefinition getRandomPresentBoxDefinition()
        {
            int rndID = new Random(DateTime.Now.Millisecond).Next(1, 7); // 1 - 6
            string requiredSprite = "present_gen" + rndID;

            foreach (itemDefinition lDefinition in mItemDefinitions.Values)
            {
                if (lDefinition.Sprite == requiredSprite)
                    return lDefinition;
            }

            return new itemDefinition();
        }
        public int getRandomPresentBoxDefinitionID()
        {
            string requiredSprite = "present_gen" + new Random(DateTime.Now.Millisecond).Next(1, 7).ToString();
            foreach (itemDefinition lDefinition in mItemDefinitions.Values)
            {
                if (lDefinition.Sprite == requiredSprite)
                    return lDefinition.ID;
            }

            return -1;
        }
        private string getItemExternalTextKey(itemDefinition pItem, int specialSpriteID)
        {
            string s = "";
            if (specialSpriteID == 0)
            {
                if (pItem.Behaviour.isWallItem)
                {
                    if (specialSpriteID == 0)
                        s = "wallitem";
                }
                else
                    s = "furni";

                s += "_";
            }

            s += pItem.Sprite;
            if(specialSpriteID > 0)
                s += "_" + specialSpriteID;

            return s;
        }
        public string getItemName(itemDefinition pItem, int specialSpriteID)
        {
            if (pItem.Behaviour.isDecoration)
                return pItem.Sprite;
            else
            {
                string extKey = this.getItemExternalTextKey(pItem, specialSpriteID) + "_name";
                return Engine.Game.Externals.getTextEntry(extKey);
            }
        }
        public string getItemDescription(itemDefinition pItem, int specialSpriteID)
        {
            if (pItem.Behaviour.isDecoration)
                return pItem.Sprite;
            else
            {
                string extKey = this.getItemExternalTextKey(pItem, specialSpriteID) + "_desc";
                return Engine.Game.Externals.getTextEntry(extKey);
            }
        }
        public string generateSpecialSprite(ref string Sprite, int specialSpriteID)
        {
            string ret = Sprite;
            if (specialSpriteID > 0)
                ret += " " + specialSpriteID.ToString();

            return ret;
        }
        /// <summary>
        /// Sets the database field 'customdata' of a given item instance to a given value. If the given value is null, then the database field will be set to MySQL null.
        /// </summary>
        /// <param name="itemID">The database ID of the item to update.</param>
        /// <param name="customData">The new custom data value.</param>
        public void setItemCustomData(int itemID, string customData)
        {
            Database dbClient = new Database(false, true);
            dbClient.addParameterWithValue("itemid", itemID);
            if (customData == null)
                dbClient.addParameterWithValue("customdata", DBNull.Value);
            else
                dbClient.addParameterWithValue("customdata", customData);

            dbClient.Open();
            if (dbClient.Ready)
                dbClient.runQuery("UPDATE items SET customdata = @customdata WHERE id = @itemid LIMIT 1");
        }
        /// <summary>
        /// Corrects a given string to a valid wall item position. If the input is totally valid, then the output will be as well.
        /// </summary>
        /// <param name="Position">The input position. Pass this argument with the 'ref' parameter.</param>
        public void correctWallItemPosition(ref string Position)
        {
            //:w=3,2 l=9,63 l
            string[] wallData = Position.Split(' ');
            Position = null;

            try
            {
                if (wallData[2] != "l" && wallData[2] != "r")
                    return; // Invalid wall (l or r is valid)

                string[] posData = wallData[0].Substring(3).Split(',');
                int widthX = int.Parse(posData[0]);
                int widthY = int.Parse(posData[1]);
                if (widthX < 0 || widthX > 200 || widthY < 0 || widthY > 200) // Invalid width position
                    return;

                posData = wallData[1].Substring(2).Split(',');
                int lengthX = int.Parse(posData[0]);
                int lengthY = int.Parse(posData[1]);
                if (lengthX < 0 || lengthX > 200 || lengthY < 0 || lengthY > 200) // Invalid length position
                    return;

                Position = ":w=" + widthX + "," + widthY + " " + "l=" + lengthX + "," + lengthY + " " + wallData[2];
            }
            catch { } // Invalid input
        }
        /// <summary>
        /// Returns a boolean indicating whether a given post.it color is valid.
        /// </summary>
        /// <param name="Color">The input color to check. This value should be passed with the ref parameter.</param>
        public bool postItColorIsValid(ref string Color)
        {
            string[] allowedPostItColors = new string[] { "FFFFFF", "FFFF33", "FF9CFF", "9CFF9C", "9CCEFF" }; // Should be a constant
            foreach (string s in allowedPostItColors)
            {
                if (Color == s)
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Returns the post.it text message of a given post.it item.
        /// </summary>
        /// <param name="itemID">The database ID of the item.</param>
        public string getPostItMessage(int itemID)
        {
            Database dbClient = new Database(false, true);
            dbClient.addParameterWithValue("itemid", itemID);

            dbClient.Open();
            if (dbClient.Ready)
                return dbClient.getString("SELECT postit_message FROM items WHERE id = @itemid LIMIT 1");
            else
                return null;
        }
        public int getTeleporterRoomID(int itemID)
        {
            Database dbClient = new Database(false, true);
            dbClient.addParameterWithValue("itemid", itemID);

            dbClient.Open();
            if (dbClient.Ready)
                return dbClient.getInteger("SELECT roomid FROM items WHERE id = @itemid LIMIT 1");
            else
                return 0;
        }

        /// <summary>
        /// Returns a virtualPetInformation object with all information about a given pet. Null is returned if the pet is not found or any error occurs.
        /// </summary>
        /// <param name="nestID">The database ID of the pet's nest item.</param>
        public virtualPetInformation getPetInformation(int nestID)
        {
            Database dbClient = new Database(false, true);
            dbClient.addParameterWithValue("id", nestID);
            dbClient.Open();

            if (dbClient.Ready)
            {
                DataRow dRow = dbClient.getRow("SELECT * FROM items_pets WHERE id = @id LIMIT 1");
                return virtualPetInformation.Parse(dRow);
            }
            else
                return null;
        }
        public void createPet(int nestID, string Name, char Type, byte Race, string htmlColor)
        {
            Database dbClient = new Database(false, true);
            dbClient.addParameterWithValue("id", nestID);
            dbClient.addParameterWithValue("name", Name);
            dbClient.addParameterWithValue("type", Type);
            dbClient.addParameterWithValue("race", Race);
            dbClient.addParameterWithValue("color", htmlColor);
            dbClient.addParameterWithValue("np", 0);
            dbClient.addParameterWithValue("nn", 0);

            dbClient.Open();
            if (dbClient.Ready)
            {
                dbClient.runQuery(
                    "INSERT INTO items_pets" +
                    "(id,name,type,race,color,nature_positive,nature_negative,born) " +
                    "VALUES " +
                    "(@id,@name,@type,@race,@color,@np,@nn,NOW())"
                    );

                virtualPetInformation newPet = new virtualPetInformation();
                newPet.ID = nestID;
                newPet.fFriendship = 1;
                newPet.dtLastKip = DateTime.Now.AddHours(-12);
                newPet.dtLastFed = newPet.dtLastKip;
                newPet.dtLastDrink = newPet.dtLastKip;
                newPet.dtLastPlayToy = DateTime.Now.AddDays(-1);
                newPet.dtLastPlayUser = newPet.dtLastPlayToy;

                newPet.Update(); // Update the fields
            }
        }

        /// <summary>
        /// Returns a virtualPetInformation object with all information about a given pet. Null is returned if the pet is not found or any error occurs.
        /// </summary>
        /// <param name="nestID">The database ID of the pet's nest item.</param>
        public virtualBotInformation getBotInformation(int botID)
        {
            Database dbClient = new Database(false, true);
            dbClient.addParameterWithValue("id", botID);
            dbClient.Open();

            if (dbClient.Ready)
            {
                DataRow dRow = dbClient.getRow("SELECT * FROM rooms_bots WHERE room_id = @id LIMIT 1");
                return virtualBotInformation.Parse(dRow);
            }
            else
                return null;
        }
        #endregion
    }
}