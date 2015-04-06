using System;
using System.Data;

using Woodpecker.Storage;
using Woodpecker.Specialized.Text;
using Woodpecker.Game.Users;

namespace Woodpecker.Game.Rooms
{
    /// <summary>
    /// Contains information about a certain room. This class cannot be inherited.
    /// </summary>
    public sealed class roomInformation
    {
        #region Fields
        /// <summary>
        /// The database ID of this room.
        /// </summary>
        public int ID;
        /// <summary>
        /// True if this room is a user flat.
        /// </summary>
        public bool isUserFlat
        {
            get { return this.ownerID > 0; }
        }

        /// <summary>
        /// The database ID of the owner of this room.
        /// </summary>
        public int ownerID;
        /// <summary>
        /// The username of this room's owner.
        /// <seealso>ownerID</seealso>
        /// </summary>
        public string Owner = "-";

        /// <summary>
        /// The database ID of the parent category of this room.
        /// </summary>
        public int categoryID;

        /// <summary>
        /// The current name of this room.
        /// </summary>
        public string Name;
        /// <summary>
        /// The current description of this room.
        /// </summary>
        public string Description;

        /// <summary>
        /// A value of the Woodpecker.Game.Rooms.roomAccessType representing the current state of the room. (locked, doorbell only etc)
        /// </summary>
        public roomAccessType accessType;
        /// <summary>
        /// True if the room's owner username should be displayed.
        /// </summary>
        public bool showOwner;
        /// <summary>
        /// True if all users in the room have rights to move furniture. (furniture CANNOT be taken out of the room)
        /// </summary>
        public bool superUsers;
        /// <summary>
        /// The password if this room. (if access type = 'password')
        /// </summary>
        public string Password;

        /// <summary>
        /// <summary>
        /// The current amount of visitors in an instance of this room.
        /// </summary>
        public int currentVisitors;
        /// <summary>
        /// The configured max amount of visitors for an instance of this room.
        /// </summary>
        public int maxVisitors;

        /// <summary>
        /// The model type of this room.
        /// </summary>
        public string modelType;
        /// <summary>
        /// Public spaces only. A string with extra casts for the room. (external .ccts etc)
        /// </summary>
        public string CCTs;
        /// <summary>
        /// The floor decoration value.
        /// </summary>
        public int Floor;
        /// <summary>
        /// The wallpaper decoration value.
        /// </summary>
        public int Wallpaper;
        #endregion

        #region Methods
        /// <summary>
        /// Parses all required fields from a given System.Data.DataRow object to a full roomInformation object.
        /// </summary>
        /// <param name="dRow">The System.Data.DataRow object containing the required fields.</param>
        public static roomInformation Parse(DataRow dRow, bool parseOwnerName)
        {
            if (dRow == null)
                return null;
            else
            {
                roomInformation Room = new roomInformation();
                Room.ID = (int)dRow["id"];
                Room.ownerID = (int)dRow["ownerid"];
                Room.categoryID = (int)dRow["category"];
                if(parseOwnerName && dRow["owner"] != DBNull.Value)
                    Room.Owner = (string)dRow["owner"];
                Room.Name = (string)dRow["roomname"];
                Room.Description = (string)dRow["description"];
                Room.modelType = (string)dRow["modeltype"];
                if(dRow["ccts"] != DBNull.Value)
                    Room.CCTs = (string)dRow["ccts"];
                Room.Wallpaper = int.Parse(dRow["wallpaper"].ToString());
                Room.Floor = int.Parse(dRow["floor"].ToString());
                Room.accessType = (roomAccessType)int.Parse(dRow["accesstype"].ToString());
                Room.showOwner = (dRow["showname"].ToString() == "true");
                Room.superUsers = (dRow["superusers"].ToString() == "true");
                Room.Password = (string)dRow["password"];
                Room.currentVisitors = int.Parse(dRow["visitors_now"].ToString());
                Room.maxVisitors = int.Parse(dRow["visitors_max"].ToString());

                return Room;
            }
        }
        /// <summary>
        /// Parses all required fields for a user flat (guestroom) from a given System.Data.DataRow object to a flat-only roomInformation object.
        /// </summary>
        /// <param name="dRow">The System.Data.DataRow object containing the required fields.</param>
        public static roomInformation ParseFlat(DataRow dRow)
        {
            if (dRow == null)
                return null;
            else
            {
                roomInformation Room = new roomInformation();
                Room.ID = (int)dRow["id"];
                Room.ownerID = (int)dRow["ownerid"];
                Room.Owner = (string)dRow["owner"];

                Room.Name = (string)dRow["roomname"];
                Room.Description = (string)dRow["description"];
                Room.accessType = (roomAccessType)int.Parse(dRow["accesstype"].ToString());
                Room.currentVisitors = int.Parse(dRow["visitors_now"].ToString());
                Room.maxVisitors = int.Parse(dRow["visitors_max"].ToString());

                return Room;
            }
        }

        /// <summary>
        /// Parses the generic roomInformation object to a room information string.
        /// </summary>
        /// <param name="viewingUser">The userInformation object of the user that requests the flat information string.</param>
        public string ToString(userInformation viewingUser)
        {
            fuseStringBuilder FSB = new fuseStringBuilder();
            FSB.appendWired(this.ID); // Room ID
            if (!this.isUserFlat) // Public space flag
                FSB.appendWired(true);

            FSB.appendClosedValue(this.Name); // Room name

            if (this.isUserFlat) // User flat
            {
                if (this.showOwner || this.ownerID == viewingUser.ID || viewingUser.hasFuseRight("fuse_see_all_roomowners"))
                    FSB.appendClosedValue(this.Owner);
                else
                    FSB.Append("-");
                FSB.appendClosedValue(this.accessType.ToString());
            }

            FSB.appendWired(this.currentVisitors);
            FSB.appendWired(maxVisitors);

            if (!this.isUserFlat)
                FSB.appendWired(this.categoryID);

            FSB.appendClosedValue(this.Description);
            if (!this.isUserFlat)
            {
                FSB.appendWired(this.ID);
                FSB.appendWired(false);
                FSB.appendClosedValue(this.CCTs);
                FSB.appendWired(false);
                FSB.appendWired(true);
            }

            return FSB.ToString();
        }
        /// <summary>
        /// Converts the required fields from this object to a user flat information string. The 'show owner username' property is handled here as well, by checking the permissions of the given user.
        /// </summary>
        /// <param name="viewingUser">The userInformation object of the user that requests the flat information string.</param>
        /// <returns></returns>
        public string ToUserFlatString()
        {
            fuseStringBuilder FSB = new fuseStringBuilder();
            FSB.appendTabbedValue(this.ID.ToString());
            FSB.appendTabbedValue(this.Name);
            FSB.appendTabbedValue(this.Owner);

            FSB.appendTabbedValue(this.accessType.ToString());
            FSB.appendTabbedValue("x");
            FSB.appendTabbedValue(this.currentVisitors.ToString());
            FSB.appendTabbedValue(maxVisitors.ToString());
            FSB.appendTabbedValue("null");
            FSB.appendTabbedValue(this.Description);
            FSB.appendTabbedValue(this.Description);
            FSB.appendChar(13);

            return FSB.ToString();
        }

        public void updateSettings()
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("roomid", this.ID);
            Database.addParameterWithValue("category", this.categoryID);
            Database.addParameterWithValue("roomname", this.Name);
            Database.addParameterWithValue("description", this.Description);
            Database.addParameterWithValue("accesstype", ((int)this.accessType).ToString());
            Database.addParameterWithValue("showname", this.showOwner.ToString().ToLower());
            Database.addParameterWithValue("superusers", this.superUsers.ToString().ToLower());
            Database.addParameterWithValue("password", this.Password);
            Database.addParameterWithValue("visitors_max", maxVisitors);
            Database.Open();

            if (Database.Ready)
                Database.runQuery("UPDATE rooms SET category = @category,roomname = @roomname,description = @description,accesstype = @accesstype,showname = @showname,superusers = @superusers,password = @password,visitors_max = @visitors_max WHERE id = @roomid LIMIT 1");
        }
        /// <summary>
        /// Updates the fields 'wallpaper' and 'floor' for this room.
        /// </summary>
        public void updateFlatProperties()
        {
            Database dbClient = new Database(false, true);
            dbClient.addParameterWithValue("roomid", this.ID);
            dbClient.addParameterWithValue("wallpaper", this.Wallpaper);
            dbClient.addParameterWithValue("floor", this.Floor);

            dbClient.Open();
            if (dbClient.Ready)
                dbClient.runQuery("UPDATE rooms SET wallpaper = @wallpaper,floor = @floor WHERE id = @roomid LIMIT 1");
        }
        #endregion
    }
}
