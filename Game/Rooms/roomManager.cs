using System;
using System.Text;
using System.Collections.Generic;
using System.Data;

using Woodpecker.Core;
using Woodpecker.Storage;
using Woodpecker.Specialized.Text;
using Woodpecker.Game.Users;
using Woodpecker.Game.Users.Roles;
using Woodpecker.Game.Rooms.Instances;

namespace Woodpecker.Game.Rooms
{
    /// <summary>
    /// Provides all kinds of functions for room (instances), room categories and the navigator.
    /// </summary>
    public class roomManager
    {
        #region Fields
        private Dictionary<int, roomInstance> mRooms = new Dictionary<int, roomInstance>();
        /// <summary>
        /// The current amount of active room instances on the server.
        /// </summary>
        public int roomCount
        {
            get { return mRooms.Count; }
        }
        private Dictionary<string, roomModel> mModels = new Dictionary<string, roomModel>();
        private Dictionary<int, roomCategoryInformation> mCategories = new Dictionary<int, roomCategoryInformation>();
        #endregion

        #region Methods
        /// <summary>
        /// Initializes the room category informations and stores them in a persistent collection object for further reference.
        /// </summary>
        public void initRoomCategories()
        {
            Logging.Log("Initializing room categories...");
            mCategories.Clear();

            Database dbClient = new Database(true, true);
            if (dbClient.Ready)
            {
                foreach (DataRow dRow in dbClient.getTable("SELECT * FROM rooms_categories ORDER BY orderid ASC").Rows)
                {
                    roomCategoryInformation pCategory = roomCategoryInformation.Parse(dRow);
                    if (pCategory != null)
                        mCategories.Add(pCategory.ID, pCategory);
                }
            }
            Logging.Log("Initialized " + mCategories.Count + " room categories.");
        }
        /// <summary>
        /// Initializes the room models and stores them in a persistent collection object for further reference.
        /// </summary>
        public void initRoomModels()
        {
            Logging.Log("Initializing room models...");
            mModels.Clear();

            Database dbClient = new Database(true, true);
            if (dbClient.Ready)
            {
                foreach (DataRow dRow in dbClient.getTable("SELECT * FROM rooms_models").Rows)
                {
                    roomModel pModel = roomModel.Parse(dRow);
                    if (pModel != null)
                        mModels.Add(pModel.typeName, pModel);
                }
            }
            Logging.Log("Initialized " + mModels.Count + " room models.");
        }
        #region Database related
        #region Flat creating & deleting
        /// <summary>
        /// Creates a new user flat room by writing the given details in the 'rooms' table of the database. The new room's ID is returned upon success.
        /// </summary>
        /// <param name="Details">The Woodpecker.Game.Rooms.roomInformation object containing the details of the new room.</param>
        public int createFlat(roomInformation Details)
        {
            int roomID = 0;
            Database Database = new Database(false, false);
            Database.addParameterWithValue("roomname", Details.Name);
            Database.addParameterWithValue("ownerid", Details.ownerID);
            Database.addParameterWithValue("modeltype", Details.modelType);
            Database.addParameterWithValue("accesstype", ((int)Details.accessType).ToString());
            Database.addParameterWithValue("showname", Details.showOwner.ToString().ToLower());
            Database.Open();

            if (Database.Ready)
            {
                Database.runQuery("INSERT INTO rooms(ownerid,roomname,modeltype,showname,accesstype) VALUES (@ownerid,@roomname,@modeltype,@showname,@accesstype)");
                roomID = Database.getInteger("SELECT MAX(id) FROM rooms WHERE ownerid = @ownerid LIMIT 1");
                Database.Close();
            }

            return roomID;
        }
        /// <summary>
        /// Deletes a given user flat from the database,
        /// </summary>
        /// <param name="roomID">The database ID of the room to delete.</param>
        public void deleteFlat(int roomID)
        {
            Database Database = new Database(false, false);
            Database.addParameterWithValue("roomid", roomID);
            Database.Open();

            if (Database.Ready)
            {
                Database.runQuery("DELETE FROM rooms WHERE id = @roomid LIMIT 1");
                Database.runQuery("DELETE FROM rooms_rights WHERE roomid = @roomid");
                Database.Close();
            }
        }
        #endregion

        #region Navigator
        /// <summary>
        /// Returns the total amount of existing user flats that a given user has created.
        /// </summary>
        /// <param name="userID">The database ID of the user to get the room count of.</param>
        /// <returns></returns>
        public int getUserRoomCount(int userID)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.Open();

            if (Database.Ready)
                return Database.getInteger("SELECT COUNT(id) FROM rooms WHERE ownerid = @ownerid");
            else
                return int.MaxValue; // Never make a room in this case
        }
        /// <summary>
        /// Returns a List of the type roomCategoryInformation with all the available flat categories for a given user role.
        /// </summary>
        /// <param name="Role">A value of the Woodpecker.Game.Users.Roles.userRole enum.</param>
        public List<roomCategoryInformation> getAvailableFlatCategories(userRole Role)
        {
            List<roomCategoryInformation> Categories = new List<roomCategoryInformation>();
            foreach(roomCategoryInformation lCategory in mCategories.Values)
            {
                if (lCategory.userRoleCanPutFlat(Role))
                    Categories.Add(lCategory);
            }

            return Categories;
        }
        /// <summary>
        /// Returns the instance of a room category and updates the active user count in the category if neccessary.
        /// </summary>
        /// <param name="ID">The database ID of the category to return.</param>
        public roomCategoryInformation getRoomCategory(int ID)
        {
            if (mCategories.ContainsKey(ID))
            {
                roomCategoryInformation pCategory = mCategories[ID];
                pCategory.refreshUserCounts();

                return pCategory;
            }
            else
                return null;
        }
        /// <summary>
        /// Returns a List of the type roomCategoryInformation with all the room categories in a given parent category (currently only one level deep) and updates the user amounts if neccessary.
        /// </summary>
        /// <param name="parentID">The database ID of the parent category to get the child categories of.</param>
        /// <returns></returns>
        public List<roomCategoryInformation> getChildRoomCategories(int parentID)
        {
            List<roomCategoryInformation> Categories = new List<roomCategoryInformation>();
            foreach (roomCategoryInformation lCategory in mCategories.Values)
            {
                if (lCategory.parentID == parentID)
                {
                    lCategory.refreshUserCounts();
                    Categories.Add(lCategory);
                }
            }

            return Categories;
        }
        /// <summary>
        /// Returns a List of the type roomInformation with all the rooms in a given category.
        /// </summary>
        /// <param name="categoryID">The database ID of the category to get the rooms of. </param>
        /// <param name="orderByVisitorsAmountDescending"></param>
        /// <returns></returns>
        public List<roomInformation> getCategoryRooms(int categoryID, bool userFlats)
        {
            List<roomInformation> Rooms = new List<roomInformation>();
            Database Database = new Database(false, true);
            Database.addParameterWithValue("category", categoryID);
            Database.Open();

            if (Database.Ready)
            {
                DataTable dTable = null;
                if (userFlats)
                    dTable = Database.getTable("SELECT rooms.*,users.username AS owner FROM rooms LEFT JOIN users ON (rooms.ownerid = users.id) WHERE rooms.category = @category ORDER BY id ASC,visitors_now DESC LIMIT 40");
                else
                    dTable = Database.getTable("SELECT * FROM rooms WHERE rooms.category = @category ORDER BY id ASC");
                
                foreach (DataRow dRow in dTable.Rows)
                {
                    Rooms.Add(roomInformation.Parse(dRow, userFlats));
                }
            }

            return Rooms;
        }

        /// <summary>
        /// Tries to find a room with a given ID. If the room is found, a full roomInformation object is created and returned. If the room is not found, then null is returned.
        /// </summary>
        /// <param name="roomID">The database ID of the room to get the information of.</param>
        public roomInformation getRoomInformation(int roomID)
        {
            if (mRooms.ContainsKey(roomID)) // Why load it? :)
                return mRooms[roomID].Information;

            Database Database = new Database(false, true);
            Database.addParameterWithValue("roomid", roomID);
            Database.Open();

            if (Database.Ready)
            {
                DataRow dRow = Database.getRow("SELECT rooms.*,users.username AS owner FROM rooms LEFT JOIN users ON rooms.ownerid = users.id WHERE rooms.id = @roomid LIMIT 1");
                return roomInformation.Parse(dRow, true);
            }
            else
                return null;
        }
        public roomInformation[] getFlatsForUser(userInformation User)
        {
            List<roomInformation> Rooms = new List<roomInformation>();
            Database Database = new Database(false, true);
            Database.addParameterWithValue("ownerid", User.ID);
            Database.Open();

            if (Database.Ready)
            {
                DataTable dTable = Database.getTable("SELECT rooms.*,users.username AS owner FROM rooms LEFT JOIN users ON (rooms.ownerid = users.id) WHERE ownerid = @ownerid");
                foreach (DataRow dRow in dTable.Rows)
                {
                    Rooms.Add(roomInformation.Parse(dRow, true));
                }
            }

            return Rooms.ToArray();
        }
        public roomInformation[] getUserFlatsSearchResult(string Criteria)
        {
            List<roomInformation> Rooms = new List<roomInformation>();
            Database Database = new Database(false, true);
            Database.addParameterWithValue("criteria", Criteria + "%"); // Only search for rooms with names starting with the criteria
            Database.addParameterWithValue("owner", Criteria); // Also search for rooms who's ownername is equal to the criteria
            Database.Open();

            if (Database.Ready)
            {
                DataTable dTable = Database.getTable("SELECT rooms.*,users.username AS owner FROM rooms LEFT JOIN users ON (rooms.ownerid = users.id) WHERE rooms.ownerid > 0 AND (users.username = @owner OR rooms.roomname LIKE @criteria) ORDER BY visitors_now DESC LIMIT 30");
                foreach (DataRow dRow in dTable.Rows)
                {
                    Rooms.Add(roomInformation.Parse(dRow, true));
                }
            }

            return Rooms.ToArray();
        }
        #endregion

        #region Room privileges
        /// <summary>
        /// Returns a boolean indicating if a given user owns a given room.
        /// </summary>
        /// <param name="userID">The database ID of the user to check.</param>
        /// <param name="roomID">The database ID of the room to check.</param>
        public bool userOwnsRoom(int userID, int roomID)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.addParameterWithValue("roomid", roomID);
            Database.Open();

            return Database.findsResult("SELECT id FROM rooms WHERE id = @roomid AND ownerid = @userid LIMIT 1");
        }
        /// <summary>
        /// Returns a boolean indicating if a given user has room rights in a given room.
        /// </summary>
        /// <param name="userID">The database ID of the user to check.</param>
        /// <param name="roomID">The database ID of the room to check.</param>
        public bool userHasRightsInRoom(int userID, int roomID)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.addParameterWithValue("roomid", roomID);
            Database.Open();

            return Database.findsResult("SELECT roomid FROM rooms_rights WHERE roomid = @roomid AND userid = @userid LIMIT 1");
        }
        /// <summary>
        /// Inserts room rights in a given room for a given user.
        /// </summary>
        /// <param name="roomID">The database ID of the room to assign the user rights to.</param>
        /// <param name="userID">The database ID of the user that gets the rights.</param>
        public void addRoomRights(int roomID, int userID)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("roomid", roomID);
            Database.addParameterWithValue("userid", userID);
            Database.Open();

            Database.runQuery("INSERT INTO rooms_rights(roomid,userid) VALUES (@roomid,@userid)");
        }
        /// <summary>
        /// Clears all roomrights for a given room and updates the room instance. (if active)
        /// </summary>
        /// <param name="roomID">The database ID of the room to clear the rights of.</param>
        public void removeRoomRights(int roomID)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("roomid", roomID);
            Database.Open();

            Database.runQuery("DELETE FROM rooms_rights WHERE roomid = @roomid");
            // TODO: refresh in room instance
        }
        /// <summary>
        /// Removes roomrights for a given user in a given room.
        /// </summary>
        /// <param name="roomID">The database ID of the room to remove the rights for the user of.</param>
        /// <param name="userID">The database ID of the user that gets the rights removed.</param>
        public void removeRoomRights(int roomID, int userID)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("roomid", roomID);
            Database.addParameterWithValue("userid", userID);
            Database.Open();

            Database.runQuery("DELETE FROM rooms_rights WHERE roomid = @roomid AND userid = @userid LIMIT 1");
            // TODO: refresh in room instance
        }
        #endregion

        #region Favorite rooms
        /// <summary>
        /// Returns the amount of favorite rooms a given user has in his/her list as an integer.
        /// </summary>
        /// <param name="userID">The database ID of the user to get the favorite room count of.</param>
        public int getFavoriteRoomCount(int userID)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.Open();

            return Database.getInteger("SELECT COUNT(id) FROM rooms_favorites WHERE userid = @userid");
        }
        /// <summary>
        /// Returns the favorite rooms of a given user as a string.
        /// </summary>
        /// <param name="User">The userInformation object of the user to retrieve the favorite rooms for.</param>
        public string getFavoriteRooms(userInformation User)
        {
            int guestRoomCount = 0;
            StringBuilder Rooms = new StringBuilder();

            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", User.ID);
            Database.Open();
            DataTable dTable = Database.getTable("SELECT rooms.*,users.username AS owner FROM rooms LEFT JOIN users ON rooms.ownerid = users.id WHERE rooms.id IN (SELECT roomid FROM rooms_favorites WHERE userid = @userid) ORDER BY rooms.id DESC LIMIT 30"); // User flats first
            
            foreach (DataRow dRow in dTable.Rows)
            {
                roomInformation Room = roomInformation.Parse(dRow, true);
                if (Room.isUserFlat)
                    guestRoomCount++;

                Rooms.Append(Room.ToString(User));
            }

            fuseStringBuilder FSB = new fuseStringBuilder();
            FSB.appendWired(guestRoomCount);
            FSB.Append(Rooms.ToString());

            return FSB.ToString();
        }
        /// <summary>
        /// Adds a given room to a given user's favorite room list.
        /// </summary>
        /// <param name="userID">The database ID of the user to modify the list for.</param>
        /// <param name="roomID">The database ID of the room to add to the user's favorite list.</param>
        public void addFavoriteRoom(int userID, int roomID)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.addParameterWithValue("roomid", roomID);
            Database.Open();

            if (Database.Ready)
                Database.runQuery("INSERT INTO rooms_favorites(userid,roomid) VALUES (@userid,@roomid)");
        }
        /// <summary>
        /// Removes a given room to a given user's favorite room list.
        /// </summary>
        /// <param name="userID">The database ID of the user to modify the list for.</param>
        /// <param name="roomID">The database ID of the room to remove to the user's favorite list.</param>
        public void removeFavoriteRoom(int userID, int roomID)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", userID);
            Database.addParameterWithValue("roomid", roomID);
            Database.Open();

            if (Database.Ready)
                Database.runQuery("DELETE FROM rooms_favorites WHERE userid = @userid AND roomid = @roomid LIMIT 1");
        }
        /// <summary>
        /// Removes all records in user favorite room lists that refer to non-existing rooms. (eg, deleted etc)
        /// </summary>
        public void dropInvalidFavoriteRoomEntries()
        {
            Database Database = new Database(true, true);
            if (Database.Ready)
                Database.runQuery("DELETE FROM rooms_favorites WHERE NOT(rooms_favorites.roomid) IN(SELECT rooms.id FROM rooms)");
        }
        #endregion
        #endregion

        #region Room models related
        public roomModel getModel(string Type)
        {
            if (!mModels.ContainsKey(Type)) // Not initialized yet
            {
                Database Database = new Database(false, true);
                Database.addParameterWithValue("modeltype", Type);
                Database.Open();

                if (Database.Ready)
                {
                    DataRow dRow = Database.getRow("SELECT * FROM rooms_models WHERE modeltype = @modeltype");
                    if (dRow != null)
                    {
                        roomModel New = roomModel.Parse(dRow);
                        if (New == null || dRow["modeltype"].ToString() != Type) // Not found / invalid case
                        {
                            Logging.Log("Room model '" + Type + "' was found but contained invalid data.", Logging.logType.commonWarning);
                            return null;
                        }
                        else
                            mModels.Add(Type, New);
                    }
                    else
                    {
                        Logging.Log("The requested room model '" + Type + "' was not found in the 'rooms_models' table of the database!", Logging.logType.commonWarning);
                        return null;
                    }
                }

                Logging.Log("Room model '" + Type + "' is initialized and added to cache.", Logging.logType.roomInstanceEvent);
            }

            return mModels[Type];
        }
        #endregion

        #region Room instances related
        /// <summary>
        /// Returns true if the given room has an instance running.
        /// </summary>
        /// <param name="roomID">The database ID of the room to check.</param>
        public bool roomInstanceRunning(int roomID)
        {
            return mRooms.ContainsKey(roomID);
        }
        /// <summary>
        /// Attempts to destroy the instance of a room given by ID.
        /// </summary>
        /// <param name="roomID">The database ID of the room to attempt to stop the instance of.</param>
        public void destroyRoomInstance(int roomID)
        {
            if (mRooms.ContainsKey(roomID))
            {
                mRooms[roomID].Destroy();
                mRooms.Remove(roomID);

                Logging.Log("Room instance of room " + roomID + " has successfully been destroyed.", Logging.logType.roomInstanceEvent);
            }
        }
        /// <summary>
        /// Returns the room instance of a room given by ID. If the room instance is already running, the instance is returned, if not, then the room instance is initialized and returned.
        /// </summary>
        /// <param name="roomID">The database ID to return the room instance of.</param>
        public roomInstance getRoomInstance(int roomID)
        {
            if (!mRooms.ContainsKey(roomID))
                mRooms.Add(roomID, new roomInstance(roomID));

            return mRooms[roomID];
        }
        /// <summary>
        /// Unloads and disposes all active room instances with no active users in them.
        /// </summary>
        public void destroyInactiveRoomInstances()
        {
            List<int> roomIDs = new List<int>();
            lock (mRooms)
            {
                foreach (roomInstance lInstance in mRooms.Values)
                {
                    if (lInstance.userAmount == 0 && lInstance.Information != null)
                    {
                        roomIDs.Add(lInstance.roomID);
                    }
                }
            }

            foreach (int lRoomID in roomIDs)
            {
                this.destroyRoomInstance(lRoomID);
            }
        }
        /// <summary>
        /// Updates the current amount of visitors in a given room instance in the database.
        /// </summary>
        /// <param name="roomID">The database ID of the room to update.</param>
        /// <param name="userAmount">The up to date amount of users in the instance of the given room.</param>
        public void updateRoomUserAmount(int roomID, int userAmount)
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("roomid", roomID);
            Database.addParameterWithValue("useramount", userAmount);
            Database.Open();

            if (Database.Ready)
                Database.runQuery("UPDATE rooms SET visitors_now = @useramount WHERE id = @roomid LIMIT 1");
        }
        /// <summary>
        /// Resets the amount of users for every room in the database to 0.
        /// </summary>
        public void resetRoomUserAmounts()
        {
            Database Database = new Database(true, true);
            if (Database.Ready)
            {
                Database.runQuery("UPDATE rooms SET visitors_now = '0'");
                Logging.Log("Current visitor amounts of rooms set to 0.");
            }
        }
        #endregion
        #endregion
    }
}
