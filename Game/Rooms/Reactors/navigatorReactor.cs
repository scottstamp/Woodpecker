using System;
using System.Collections.Generic;

using Woodpecker.Core;
using Woodpecker.Specialized.Text;
using Woodpecker.Specialized.Encoding;

using Woodpecker.Game.Rooms.Instances;
using Woodpecker.Game.Rooms.Instances.Interaction;

namespace Woodpecker.Game.Rooms
{
    /// <summary>
    /// Contains target methods regarding actions in the Navigator, such as viewing room lists, creating and modifying rooms and authenticating and entering rooms.
    /// </summary>
    public class navigatorReactor : Reactor
    {
        #region Get user flats
        /// <summary>
        /// 16 - "@P"
        /// </summary>
        public void SUSERF()
        {
            roomInformation[] Rooms = Engine.Game.Rooms.getFlatsForUser(Session.User);
            if (Rooms.Length > 0) // Rooms found!
            {
                Response.Initialize(16); // "@P"
                foreach (roomInformation Room in Rooms)
                {
                    Response.Append(Room.ToUserFlatString());
                }
            }
            else
            {
                Response.Initialize(57); // "@y"
                Response.Append(Session.User.Username);
            }

            sendResponse();
        }
        /// <summary>
        /// 17 - "@Q"
        /// </summary>
        public void SRCHF()
        {
            string Criteria = Request.Content.Split('%')[1]; // Get plain criteria
            roomInformation[] Rooms = Engine.Game.Rooms.getUserFlatsSearchResult(Criteria);
            if (Rooms.Length > 0) // Rooms found!
            {
                Response.Initialize(55); // "@w"
                foreach (roomInformation Room in Rooms)
                {
                    roomCategoryInformation pCategory = Engine.Game.Rooms.getRoomCategory(Room.categoryID);
                    if (pCategory != null && pCategory.userRoleHasAccess(Session.User.Role))
                    {
                        if (!Room.showOwner)
                        {
                            if (Room.ownerID != Session.User.ID && !Session.User.hasFuseRight("fuse_see_all_roomowners"))
                            {
                                if (Room.Owner == Criteria) // Was searching for room owners
                                    continue; // Can't view this room at all
                                else
                                    Room.Owner = "-";
                            }
                        }

                        Response.Append(Room.ToUserFlatString());
                    }
                }
            }
            else
                Response.Initialize(58); // "@z"

            sendResponse();
        }
        #endregion

        #region Favorite room list
        /// <summary>
        /// 18 - "@R"
        /// </summary>
        public void GETFVRF()
        {
            Response.Initialize(61); // "@}"
            Response.Append("H" + "H" + "J" + Convert.ToChar(2) + "HHH"); // Blegh, messy, but it's static as hedgehogs
            Response.Append(Engine.Game.Rooms.getFavoriteRooms(Session.User));

            sendResponse();
        }
        /// <summary>
        /// 19 - "@S"
        /// </summary>
        public void ADD_FAVORITE_ROOM()
        {
            if (Engine.Game.Rooms.getFavoriteRoomCount(Session.User.ID) < Configuration.getNumericConfigurationValue("users.favoriterooms.max"))
            {
                int[] Parameters = Request.getWiredParameters();
                int roomID = Parameters[1];

                roomInformation Room = Engine.Game.Rooms.getRoomInformation(roomID);
                if (Room != null && Engine.Game.Rooms.getRoomCategory(Room.categoryID).userRoleHasAccess(Session.User.Role)) // Room exist and user has access to room
                    Engine.Game.Rooms.addFavoriteRoom(Session.User.ID, roomID);
            }
            else
                Session.gameConnection.sendLocalizedError("nav_error_toomanyfavrooms");
        }
        /// <summary>
        /// 20 - "@T"
        /// </summary>
        public void DEL_FAVORITE_ROOM()
        {
            int[] Parameters = Request.getWiredParameters();
            int roomID = Parameters[1];

            Engine.Game.Rooms.removeFavoriteRoom(Session.User.ID, roomID);
        }
        #endregion

        #region Flat create and edit
        /// <summary>
        /// 21 - "@U"
        /// </summary>
        public void GETFLATINFO()
        {
            int roomID = int.Parse(Request.Content);
            roomInformation Room = Engine.Game.Rooms.getRoomInformation(roomID);
            if (Room == null || !Room.isUserFlat) // Room is not found / not valid
                return;

            Response.Initialize(54); // "@v"

            Response.appendWired(Room.superUsers);
            Response.appendWired((int)Room.accessType);
            Response.appendWired(roomID);

            if (Room.showOwner || Room.ownerID == Session.User.ID || Session.User.hasFuseRight("fuse_see_all_roomowners"))
                Response.appendClosedValue(Room.Owner);
            else
                Response.appendClosedValue("-");

            Response.appendClosedValue(Room.modelType);
            Response.appendClosedValue(Room.Name);
            Response.appendClosedValue(Room.Description);

            roomCategoryInformation Category = Engine.Game.Rooms.getRoomCategory(Room.categoryID);
            Response.appendWired(Room.showOwner);
            Response.appendWired(Category != null && Category.allowTrading);
            Response.appendWired(Room.currentVisitors);
            Response.appendWired(Room.maxVisitors);

            sendResponse();
        }
        /// <summary>
        /// 23 - "@W"
        /// </summary>
        public void DELETEFLAT()
        {
            int roomID = int.Parse(Request.Content);
            if (Engine.Game.Rooms.userOwnsRoom(Session.User.ID, roomID))
            {
                Engine.Game.Rooms.deleteFlat(roomID);
                Engine.Game.Rooms.destroyRoomInstance(roomID);
            }
        }
        /// <summary>
        /// 24 - "@X"
        /// </summary>
        public void UPDATEFLAT()
        {
            string[] Details = Request.Content.Split('/');
            int roomID = int.Parse(Details[0]);

            roomInformation Room = Engine.Game.Rooms.getRoomInformation(roomID);
            if (Room != null && Room.ownerID == Session.User.ID)
            {
                stringFunctions.filterVulnerableStuff(ref Details[1], true);
                Room.Name = Details[1];
                Room.accessType = (roomAccessType)Enum.Parse(typeof(roomAccessType), Details[2]);
                Room.showOwner = (Details[3] == "1");
                Room.updateSettings();
            }
        }
        /// <summary>
        /// 25 - "@Y"
        /// </summary>
        public void SETFLATINFO()
        {
            int roomID = 0;
            if (Request.Content[0] == '/') // Create room
                roomID = int.Parse(Request.Content.Split('/')[1]);
            else
                roomID = int.Parse(Request.Content.Split('/')[0]);

            roomInformation Room = Engine.Game.Rooms.getRoomInformation(roomID);
            if (Room != null && Room.ownerID == Session.User.ID) // Allowed to edit!
            {
                string[] Settings = Request.Content.Split(Convert.ToChar(13));
                for (int i = 1; i < Settings.Length; i++)
                {
                    int keyLength = Settings[i].IndexOf('=');
                    string Key = Settings[i].Substring(0, keyLength);
                    string Value = Settings[i].Substring(keyLength + 1);

                    switch (Key)
                    {
                        case "description":
                            {
                                stringFunctions.filterVulnerableStuff(ref Value, true);
                                Room.Description = Value;
                            }
                            break;

                        case "allsuperuser":
                            {
                                Room.superUsers = (Value == "1");
                            }
                            break;

                        case "maxvisitors":
                            {
                                Room.maxVisitors = int.Parse(Value);
                                if (Room.maxVisitors < 10 || Room.maxVisitors > 50)
                                    Room.maxVisitors = 25;
                            }
                            break;

                        case "password":
                            {
                                stringFunctions.filterVulnerableStuff(ref Value, false);
                                Room.Password = Value;
                            }
                            break;

                        default: // Hax
                            return;
                    }
                }
                Room.updateSettings();
            }
        }
        /// <summary>
        /// 29 - "@]"
        /// </summary>
        public void CREATEFLAT()
        {
            if (Engine.Game.Rooms.getUserRoomCount(Session.User.ID) >= Configuration.getNumericConfigurationValue("users.rooms.maxroomsperuser"))
            {
                Session.gameConnection.sendLocalizedError("Error creating a private room");
                return;
            }

            Response.Initialize(59); // "@{"

            string[] Settings = Request.Content.Split('/');
            roomInformation newRoom = new roomInformation();
            newRoom.ownerID = Session.User.ID;
            newRoom.Name = Settings[2];
            newRoom.modelType = Settings[3];
            stringFunctions.filterVulnerableStuff(ref newRoom.Name, true);
            
            // Verify room model
            roomModel testModel = Engine.Game.Rooms.getModel(newRoom.modelType);
            if (testModel == null
                || testModel.userType == roomModelUserType.PublicSpaceModel
                || testModel.userType == roomModelUserType.UserFlatSpecialModel && !Session.User.hasFuseRight("fuse_use_special_room_layouts"))
                return; // Model does not exist, model is for public spaces only or model requires certain FUSE right and user does not has this fuse right

            newRoom.accessType = (roomAccessType)Enum.Parse(typeof(roomAccessType), Settings[4]);
            newRoom.showOwner = (Settings[5] == "1");

            int newRoomID = Engine.Game.Rooms.createFlat(newRoom);
            Response.Append(newRoomID);
            Response.appendChar(13);
            Response.Append(newRoom.Name);

            sendResponse();
        }
        #endregion

        #region Navigator categories
        /// <summary>
        /// 150 - "BV"
        /// </summary>
        public void NAVIGATE()
        {
            int categoryID = Request.getWiredParameters()[1];
            Response.Initialize(220); // "C\"
            roomCategoryInformation pCategory = Engine.Game.Rooms.getRoomCategory(categoryID);

            if (pCategory != null && pCategory.userRoleHasAccess(Session.User.Role)) // Valid category
            {
                bool hideFull = wireEncoding.decodeBoolean(Request.Content.Substring(0, 1));

                #region Build category info
                Response.appendWired(hideFull);
                Response.appendWired(pCategory.ID);
                if (pCategory.forPublicSpaces)
                    Response.appendWired(false); // Public spaces
                else
                    Response.appendWired(2); // User flats
                Response.appendClosedValue(pCategory.Name);
                Response.appendWired(pCategory.currentVisitors);
                Response.appendWired(pCategory.maxVisitors);
                Response.appendWired(pCategory.parentID);

                List<roomInformation> Rooms = Engine.Game.Rooms.getCategoryRooms(categoryID, !pCategory.forPublicSpaces);
                if (!pCategory.forPublicSpaces) // User flats
                    Response.appendWired(Rooms.Count);
                #endregion

                #region Build rooms info
                foreach (roomInformation lRoom in Rooms)
                {
                    if (!hideFull || lRoom.currentVisitors <= lRoom.maxVisitors)
                        Response.Append(lRoom.ToString(Session.User));
                }
                #endregion

                #region Build subcategory info
                foreach (roomCategoryInformation lSubcategory in Engine.Game.Rooms.getChildRoomCategories(pCategory.ID))
                {
                    if (lSubcategory.userRoleHasAccess(Session.User.Role) && (!hideFull || lSubcategory.currentVisitors < lSubcategory.maxVisitors)) // User has access to this room category and requests to see this category
                    {
                        Response.appendWired(lSubcategory.ID);
                        Response.appendWired(false);
                        Response.appendClosedValue(lSubcategory.Name);
                        Response.appendWired(lSubcategory.currentVisitors);
                        Response.appendWired(lSubcategory.maxVisitors);
                        Response.appendWired(pCategory.ID);
                    }
                }
                #endregion
            }

            sendResponse();
        }
        /// <summary>
        /// 151 - "BW"
        /// </summary>
        public void GETUSERFLATCATS()
        {
            Response.Initialize(221); // "C]"

            List<roomCategoryInformation> Categories = Engine.Game.Rooms.getAvailableFlatCategories(Session.User.Role);
            Response.appendWired(Categories.Count);

            foreach (roomCategoryInformation lCategory in Categories)
            {
                Response.appendWired(lCategory.ID);
                Response.appendClosedValue(lCategory.Name);
            }

            sendResponse();
        }
        /// <summary>
        /// 152 - "BX"
        /// </summary>
        public void GETFLATCAT()
        {
            int roomID = Request.getNextWiredParameter();
            roomInformation Room = Engine.Game.Rooms.getRoomInformation(roomID);

            Response.Initialize(222); // "C^"
            if (Room != null)
            {
                Response.appendWired(Room.ID);
                Response.appendWired(Room.categoryID);
            }
            sendResponse();
        }
        /// <summary>
        /// 153 - "BY"
        /// </summary>
        public void SETFLATCAT()
        {
            int[] Parameters = Request.getWiredParameters();
            int roomID = Parameters[0];
            int categoryID = Parameters[1];

            roomCategoryInformation newCategory = Engine.Game.Rooms.getRoomCategory(categoryID);
            if (newCategory != null && newCategory.userRoleCanPutFlat(Session.User.Role)) // Valid category
            {
                roomInformation Room = Engine.Game.Rooms.getRoomInformation(roomID);
                if (Room != null && Room.ownerID == Session.User.ID && Room.categoryID != newCategory.ID) // Valid change
                {
                    Room.categoryID = newCategory.ID;
                    Room.updateSettings();
                    if (Engine.Game.Rooms.roomInstanceRunning(roomID))
                        Engine.Game.Rooms.getRoomInstance(roomID).Information = Room;
                }
            }
        }
        #endregion

        #region Enter room
        #region Target methods
        /// <summary>
        /// 2 - "@B"
        /// </summary>
        public void ROOM_DIRECTORY()
        {
            if (Session.inRoom)
                Session.leaveRoom(true);

            bool publicSpace = (Request.Content[0] == 'A'); // Base64 A = 1, @ = 0
            int roomID = wireEncoding.Decode(Request.Content.Substring(1));

            Response.Initialize(19); // "@S"
            sendResponse();

            if (publicSpace)// No flat checks
            {
                roomInstance pInstance = Engine.Game.Rooms.getRoomInstance(roomID);
                if(pInstance != null && !pInstance.Information.isUserFlat)
                    goToRoom(roomID);

                //pInstance.loadRoomBot(roomID, 0, 0);
            }
        }
        /// <summary>
        /// 53 - "@u"
        /// </summary>
        public void QUIT()
        {
            Session.leaveRoom(true);
        }
        /// <summary>
        /// 54 - "@v"
        /// </summary>
        public void GOVIADOOR()
        {
            string[] Data = Request.Content.Split('/');
            int roomID = int.Parse(Data[0]);
            int itemID = int.Parse(Data[1]);
            if(Session.authenticatedFlat == roomID && Session.authenticatedTeleporter == itemID) // Authenticated for flat and teleporter item
            {
                roomInstance pInstance = Engine.Game.Rooms.getRoomInstance(roomID);
                if(pInstance != null)
                {
                    pInstance.registerSession(Session.ID);
                    Response.Initialize(19); // "@S"
                    sendResponse();
                }
            }
        }
        /// <summary>
        /// 57 - "@y"
        /// </summary>
        public void TRYFLAT()
        {
            int roomID = 0;
            string givenPassword = null;
            if (Request.Content.Contains("/"))
            {
                string[] Details = Request.Content.Split('/');
                roomID = int.Parse(Details[0]);
                givenPassword = Details[1];
            }
            else
                roomID = int.Parse(Request.Content);

            roomInformation Room = Engine.Game.Rooms.getRoomInformation(roomID);
            if (Room == null)
                return;

            if (Session.authenticatedTeleporter > 0)
            {
                if (roomID != Session.authenticatedFlat)
                    return; // Attempting to enter different flat via authenticated teleporter
            }
            else
            {
                if (Room.accessType != roomAccessType.open && Room.ownerID != Session.User.ID && !Session.User.hasFuseRight("fuse_enter_locked_rooms")) // Can't override checks
                {
                    if (Room.accessType == roomAccessType.password) // Passworded room
                    {
                        if (givenPassword != Room.Password) // Incorrect password given
                        {
                            Session.gameConnection.sendLocalizedError("Incorrect flat password");
                            return;
                        }
                    }
                    else // Doorbell
                    {
                        int messageID = 131; // "BC" (no answer)
                        if (Engine.Game.Rooms.roomInstanceRunning(roomID))
                        {
                            roomInstance Instance = Engine.Game.Rooms.getRoomInstance(roomID);
                            if (Instance.castDoorbellUser(Session.User.Username))
                            {
                                messageID = 91; // "A["
                                Instance.registerSession(Session.ID);
                                Session.waitingFlat = roomID;
                            }
                        }

                        Response.Initialize(messageID);
                        sendResponse();
                        return; // Wait for the eventual response
                    }
                }
                Session.authenticatedFlat = roomID;
            }

            Response.Initialize(41); // "@i"
            sendResponse();
        }
        /// <summary>
        /// 59 - "@{"
        /// </summary>
        public void GOTOFLAT()
        {
            int roomID = int.Parse(Request.Content);
            if (roomID == Session.authenticatedFlat) // User has authenticated at TRYFLAT (message 57) and is trying to enter the room he/she authenticated for
                goToRoom(roomID);
        }
        /// <summary>
        /// 88 - "AX"
        /// </summary>
        public void STOP()
        {
            /* Putting this target method in the roomReactor will not work, put it here instead */
            if (Session.inRoom)
            {
                string Status = Request.Content;
                if (Status == "CarryItem")
                    Session.roomInstance.getRoomUser(Session.ID).removeStatus("handitem");
                else if (Status == "Dance")
                    Session.roomInstance.getRoomUser(Session.ID).removeStatus("dance");
            }
        }
        /// <summary>
        /// 154 - "BZ"
        /// </summary>
        public void GETSPACENODEUSERS()
        {
            Response.Initialize(223); // "C_"
            int roomID = Request.getNextWiredParameter();
            if (Engine.Game.Rooms.roomInstanceRunning(roomID))
                Response.Append(Engine.Game.Rooms.getRoomInstance(roomID).getUserList());

            sendResponse();
        }
        /// <summary>
        /// 182 - "Bv"
        /// </summary>
        public void GETINTERST()
        {
            Response.Initialize(258); // "DB"
            string adImage = ""; // TODO: configure this interstitial ad
            string adUri = ""; // TODO: idem dito
            if (adImage.Length > 0)
            {
                Response.appendTabbedValue(adImage);
                Response.Append(adUri);
            }
            else
                Response.Append(0); // No advertisement to be shown

            sendResponse();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tries to add this user to the room instance with a given ID.
        /// </summary>
        /// <param name="roomID">The database ID of the room to try to enter.</param>
        private void goToRoom(int roomID)
        {
            try
            {
                roomInstance Instance = Engine.Game.Rooms.getRoomInstance(roomID);
                if (Instance.Information == null || !Instance.userRoleHasAccess(Session.User.Role)) // Instance not OK for user
                    return;

                if (Instance.isFull) // Room is full
                {
                    if (!Session.User.hasFuseRight("fuse_enter_full_rooms") && Session.User.ID != Instance.Information.ownerID) // User has no FUSE right to enter full rooms and user is not the room owner
                    {
                        Response.Initialize(224); // "C`"
                        Response.appendWired(1); // 'User flat full'
                        sendResponse();
                        return;
                    }
                }

                Instance.registerSession(this.Session.ID);
                Session.roomInstance = Instance;

                Response.Initialize(166); // "Bf"
                Response.Append("interstitial advertisement goes here");
                sendResponse();

                Response.Initialize(69); // "AE"
                Response.Append(Instance.Information.modelType);
                Response.Append(" ");
                Response.Append(Instance.roomID);
                sendResponse();

                if (Instance.Information.isUserFlat)
                {
                    Response.Initialize(46); // "@n"
                    Response.Append("wallpaper/");
                    Response.Append(Instance.Information.Wallpaper);
                    sendResponse();

                    Response.Initialize(46); // "@n"
                    Response.Append("floor/");
                    Response.Append(Instance.Information.Floor);
                    sendResponse();

                    Session.gameConnection.reactorHandler.Register(new flatReactor());
                }
                else
                    Session.gameConnection.reactorHandler.Register(new roomReactor());
            }
            catch { }
        }
        #endregion
        #endregion

        #region Other
        /// <summary>
        /// "B[" - "REMOVEALLRIGHTS" 
        /// </summary>
        public void Listener155()
        {
            int roomID = Request.getNextWiredParameter();
            if (Engine.Game.Rooms.userOwnsRoom(Session.User.ID, roomID))
                Engine.Game.Rooms.removeRoomRights(roomID);
        }
        #endregion
    }
}