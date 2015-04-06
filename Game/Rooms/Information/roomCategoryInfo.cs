using System;
using System.Data;
using System.Collections.Generic;

using Woodpecker.Storage;
using Woodpecker.Net.Game.Messages;
using Woodpecker.Game.Users.Roles;

namespace Woodpecker.Game.Rooms
{
    /// <summary>
    /// Represents a category for virtual rooms.
    /// </summary>
    public class roomCategoryInformation
    {
        #region Fields
        /// <summary>
        /// The database ID of this category.
        /// </summary>
        public int ID;
        /// <summary>
        /// The database ID of this category's parent.
        /// </summary>
        public int parentID;
        /// <summary>
        /// The display name of this category.
        /// </summary>
        public string Name;
        /// <summary>
        /// True if this category is only serving as the root of other categories, and can not hold rooms.
        /// </summary>
        public bool isNode;
        /// <summary>
        /// True if this category is for public spaces. False if for guestrooms.
        /// </summary>
        public bool forPublicSpaces;
        /// <summary>
        /// True if trading is allowed in rooms that are placed in this category.
        /// </summary>
        public bool allowTrading;
        /// <summary>
        /// The sum of all the active users (rooms.visitors_now) in the rooms in this category.
        /// </summary>
        public int currentVisitors;
        /// <summary>
        /// The max amount of visitors in this category. (sum of rooms.visitors_category)
        /// </summary>
        public int maxVisitors;
        /// <summary>
        /// A value of the Woodpecker.Game.Users.Roles.userRole enum representing the minimum user role a usre requires to view this room category and it's rooms. 
        /// </summary>
        private userRole minimumAccessRole;
        /// <summary>
        /// A value of the Woodpecker.Game.Users.Roles.userRole enum representing the minimum user role a usre requires to put a self-created user flat on this room category.
        /// </summary>
        private userRole minimumSetFlatCatRole;
        private DateTime lastUserCountUpdate;
        #endregion

        #region Methods
        /// <summary>
        /// Returns true if a given user role has access to this room category and it's rooms.
        /// </summary>
        /// <param name="Role">A value of the Woodpecker.Game.Users.Roles.userRole enum representing the role to check.</param>
        public bool userRoleHasAccess(userRole Role)
        {
            return (Role >= minimumAccessRole);
        }
        /// <summary>
        /// Returns true if a given user role is allowed to put self-created user flats on this on this room category.
        /// </summary>
        /// <param name="Role">A value of the Woodpecker.Game.Users.Roles.userRole enum representing the role to check.</param>
        public bool userRoleCanPutFlat(userRole Role)
        {
            return (!this.isNode && !this.forPublicSpaces && Role >= minimumSetFlatCatRole); // This category is not just a node, not for public spaces and the user has the right to place rooms on this category
        }
        /// <summary>
        /// Refreshes the user amounts for this category if needed.
        /// </summary>
        public void refreshUserCounts()
        {
            if (this.lastUserCountUpdate.AddSeconds(1) < DateTime.Now)
            {
                Database dbClient = new Database(true, true);
                if (dbClient.Ready)
                {
                    DataRow dRow = dbClient.getRow("SELECT SUM(visitors_now) AS now,SUM(visitors_max) AS max FROM rooms WHERE category = '" + this.ID + "' ORDER BY visitors_now DESC LIMIT 40");
                    if (dRow["now"] != DBNull.Value) // Results
                    {
                        this.currentVisitors = int.Parse(dRow["now"].ToString());
                        maxVisitors = int.Parse(dRow["max"].ToString());
                    }
                    else
                        maxVisitors = 1;
                    this.lastUserCountUpdate = DateTime.Now;
                }
            }
        }
        /// <summary>
        /// Parses a full data row of the rooms_categories table to a Woodpecker.Game.Rooms.roomCategory object. On errors, null is returned.
        /// </summary>
        /// <param name="dRow">The System.Data.DataRow object containing the required fields.</param>
        public static roomCategoryInformation Parse(DataRow dRow)
        {
            if (dRow == null)
                return null;
            else
            {
                roomCategoryInformation ret = new roomCategoryInformation();
                try
                {
                    ret.ID = (int)dRow["id"];
                    ret.parentID = (int)dRow["parentid"];
                    ret.isNode = ((string)dRow["isnode"] == "1");
                    ret.Name = (string)dRow["name"];
                    ret.forPublicSpaces = ((string)dRow["publicspaces"] == "1");
                    ret.allowTrading = ((string)dRow["allowtrading"] == "1");
                    ret.minimumAccessRole = (userRole)int.Parse(dRow["minrole_access"].ToString());
                    ret.minimumSetFlatCatRole = (userRole)int.Parse(dRow["minrole_setflatcat"].ToString());
                }
                catch { ret = null; }

                return ret;
            }
        }
        #endregion
    }
}
