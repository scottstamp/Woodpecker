using System;
using System.Data;

using Woodpecker.Storage;
using Woodpecker.Specialized.Text;

namespace Woodpecker.Game.Items
{
    /// <summary>
    /// Represents the base information for both floor and wall items.
    /// </summary>
    public class stripItem
    {
        #region Fields
        /// <summary>
        /// The database ID of this item.
        /// </summary>
        public int ID;
        /// <summary>
        /// The database ID of the user that owns this item.
        /// </summary>
        public int ownerID;
        /// <summary>
        /// The database ID of the room this user is currently in.
        /// </summary>
        public int roomID;
        /// <summary>
        /// A reference to the itemDefinition object for this item.
        /// </summary>
        public itemDefinition Definition;
        /// <summary>
        /// A string that holds the custom data (status etc) of this item.
        /// </summary>
        public string customData;
        /// <summary>
        /// Holds the database ID of the other teleporter if this item is a teleporter. Otherwise, it holds 0.
        /// </summary>
        public int teleporterID;
        /// <summary>
        /// Holds the current requires-update status of this item.
        /// </summary>
        public bool requiresUpdate;
        #endregion

        #region Methods
        /// <summary>
        /// Converts this item instance representation to a string that displays this item on a strip (hand or trading box) and returns it.
        /// </summary>
        /// <param name="slotID">The current slot ID of this item on the strip.</param>
        public string ToStripString(int stripSlotID)
        {
            /*
            * Wallitem:
            * + "SI"
            * + itemID
            * + slotID
            * + stripItemType 'I'
            * + itemID
            * + sprite
            * + color (incase of decoration or post.it pad: customdata)
            * + recycleable 1/0
            * + "/"

            * Flooritem:
            * + "SI"
            * + itemID (negative)
            * + slotID
            * + stripItemType 'S'
            * + itemID
            * + sprite
            * + length
            * + width
            * + customdata
            * + color
            * + recycleable 1/0
            * + sprite
            * + "/"
            */

            fuseStringBuilder FSB = new fuseStringBuilder();
            FSB.appendStripValue("SI");
            //if (!this.Definition.isWallItem) // Floor item ID = negative, so prefix with '-'
            //    FSB.Append("-");
            FSB.appendStripValue(this.ID.ToString());
            FSB.appendStripValue(stripSlotID.ToString());
            if (this.Definition.Behaviour.isWallItem)
                FSB.appendStripValue("I");
            else
                FSB.appendStripValue("S");
            FSB.appendStripValue(this.ID.ToString());
            FSB.appendStripValue(this.Definition.Sprite);
            if (this.Definition.Behaviour.isWallItem)
            {
                FSB.appendStripValue(this.customData);
                FSB.appendStripValue("0"); // Not-recycleable
            }
            else
            {
                FSB.appendStripValue(this.Definition.Length.ToString());
                FSB.appendStripValue(this.Definition.Width.ToString());
                FSB.appendStripValue(this.customData);
                FSB.appendStripValue(this.Definition.Color);
                FSB.appendStripValue("0"); // Not-recycleable
                FSB.appendStripValue(this.Definition.Sprite);
            }
            FSB.Append("/");

            return FSB.ToString();
        }
        #endregion
    }

    /// <summary>
    /// Represents a floor item. This class inherits stripItem.
    /// <seealso>stripItem</seealso>
    /// </summary>
    public class floorItem : stripItem
    {
        #region Fields
        /// <summary>
        /// The X position of this item on the map as a byte.
        /// </summary>
        public byte X;
        /// <summary>
        /// The Y position of this item on the map as a byte.
        /// </summary>
        public byte Y;
        /// <summary>
        /// The height this item is located at as floating point value.
        /// </summary>
        public float Z;
        /// <summary>
        /// The rotation of this item on the map as a byte.
        /// </summary>
        public byte Rotation;
        /// <summary>
        /// Returns the sum of the item's position height and the item's top height. (height offset)
        /// <seealso>Height</seealso>
        /// </summary>
        public float totalHeight
        {
            get { return this.Z + this.Definition.topHeight; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Updates the information of this item instance in the database.
        /// </summary>
        public void Update()
        {
            Database dbClient = new Database(false, true);
            dbClient.addParameterWithValue("z", this.Z);
            if (this.customData == null || this.Definition.Behaviour.customDataTrueFalse)
                dbClient.addParameterWithValue("customdata", DBNull.Value);
            else
                dbClient.addParameterWithValue("customdata", this.customData);

            dbClient.Open();
            if (dbClient.Ready)
            {
                dbClient.runQuery("UPDATE items SET ownerid = '" + this.ownerID + "',roomid = '" + this.roomID + "',x = '" + this.X + "',y = '" + this.Y + "',z = @z,rotation = '" + this.Rotation + "',customdata = @customdata WHERE id = '" + this.ID + "' LIMIT 1");
                this.requiresUpdate = false; // Update performed!
            }
        }
        /// <summary>
        /// Converts this floor item to a string representation and returns it.
        /// </summary>
        public override string ToString()
        {
            /* Format example: public space object
             * 2732 invisichair 27 32 1 2
            */

            /* Format example: furniture
             * 802610
             * chair_polyfon
             * PB PA I I H 0.0
             * 0,0,0
             * 
             * H data
             */

            fuseStringBuilder FSB = new fuseStringBuilder();
            if (!this.Definition.Behaviour.isPublicSpaceObject)
            {
                FSB.appendClosedValue(this.ID.ToString());
                FSB.appendClosedValue(this.Definition.Sprite);

                FSB.appendWired(this.X);
                FSB.appendWired(this.Y);
                FSB.appendWired(this.Definition.Length);
                FSB.appendWired(this.Definition.Width);
                FSB.appendWired(this.Rotation);
                FSB.appendClosedValue(stringFunctions.formatFloatForClient(this.Z));

                FSB.appendClosedValue(this.Definition.Color);
                FSB.appendClosedValue(null);

                FSB.appendWired(this.teleporterID);
                FSB.appendClosedValue(this.customData);
            }
            else
            {
                //FSB.appendWhiteSpacedValue(this.ID.ToString());
                FSB.appendWhiteSpacedValue(this.customData);
                FSB.appendWhiteSpacedValue(this.Definition.Sprite);
                FSB.appendWhiteSpacedValue(this.X.ToString());
                FSB.appendWhiteSpacedValue(this.Y.ToString());
                FSB.appendWhiteSpacedValue(this.Z.ToString());
                FSB.Append(this.Rotation.ToString());
                FSB.appendChar(13);
            }

            return FSB.ToString();
        }
        #endregion
    }
    /// <summary>
    /// Represents a wall item. This class inherits stripItem.
    /// <seealso>stripItem</seealso>
    /// </summary>
    public class wallItem : stripItem
    {
        #region Fields
        /// <summary>
        /// The position of this wall item on the wall as a string.
        /// </summary>
        public string wallPosition;
        /// <summary>
        /// The actual text message of a a post.it wall item. This field holds null if this wall item is not a post.it.
        /// </summary>
        public string postItMessage;
        #endregion

        #region Methods
        public void Update()
        {
            Database dbClient = new Database(false, true);
            if (this.wallPosition == null)
                dbClient.addParameterWithValue("wallposition", DBNull.Value);
            else
                dbClient.addParameterWithValue("wallposition", this.wallPosition);
            if (this.customData == null)
                dbClient.addParameterWithValue("customdata", DBNull.Value);
            else
                dbClient.addParameterWithValue("customdata", this.customData);
            if (this.postItMessage == null)
                dbClient.addParameterWithValue("postit_message", DBNull.Value);
            else
                dbClient.addParameterWithValue("postit_message", this.postItMessage);

            dbClient.Open();
            if (dbClient.Ready)
            {
                dbClient.runQuery("UPDATE items SET ownerid = '" + this.ownerID + "',roomid = '" + this.roomID + "',customdata = @customdata,wallposition = @wallposition,postit_message = @postit_message WHERE id = '" + this.ID + "' LIMIT 1");
                this.requiresUpdate = false; // Update performed!
            }
        }
        /// <summary>
        /// Converts this wall item to a string representation and returns it.
        /// </summary>
        public override string ToString()
        {
            fuseStringBuilder FSB = new fuseStringBuilder();
            FSB.appendTabbedValue(this.ID.ToString());
            FSB.appendTabbedValue(this.Definition.Sprite);
            FSB.appendTabbedValue(" ");
            FSB.appendTabbedValue(this.wallPosition);
            if(this.customData != null)
                FSB.Append(this.customData);

            return FSB.ToString();
        }
        #endregion
    }
}
