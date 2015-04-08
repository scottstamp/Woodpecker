using System;
using System.Collections.Generic;

using Woodpecker.Core;
using Woodpecker.Sessions;
using Woodpecker.Game.Users.Roles;
using Woodpecker.Net.Game.Messages;
using Woodpecker.Specialized.Text;

using Woodpecker.Game.Rooms.Units;

namespace Woodpecker.Game.Rooms.Instances
{
    public partial class roomInstance
    {
        #region Fields
        /// <summary>
        /// List (uint) collection containing the session IDs of users that are entering the room.
        /// </summary>
        private List<uint> enteringSessions = new List<uint>();
        /// <summary>
        /// System.Collections.Generic.Dictionary (uint, roomUser) collection with the roomUser objects of this room's users, with their session ID as key.
        /// </summary>
        private Dictionary<uint, roomUser> roomUsers = new Dictionary<uint, roomUser>();
        /// <summary>
        /// A System.Collections.Generic.List (int) collection containing the registered room unit IDs.
        /// </summary>
        private List<int> roomUnitIdentifiers = new List<int>();

        /// <summary>
        /// The current amount of active room users in this room instance.
        /// </summary>
        public int userAmount
        {
            get 
            {
                if (this.roomUsers != null)
                    return this.roomUsers.Count;
                else
                    return 0;
            }
        }
        /// <summary>
        /// True if this room instance can't contain new room users.
        /// </summary>
        public bool isFull
        {
            get { return (this.userAmount >= this.Information.maxVisitors); }
        }
        /// <summary>
        /// Returns true if a given user role has access to this room.
        /// </summary>
        /// <param name="Role">A value of the Woodpecker.Game.Users.Roles.userRole enum representing the role to check.</param>
        public bool userRoleHasAccess(userRole Role)
        {
            roomCategoryInformation Category = Engine.Game.Rooms.getRoomCategory(this.Information.categoryID);
            if (Category != null)
                return Category.userRoleHasAccess(Role);
            else
                return (Role == userRole.Administrator); // Category does not exist, only administrator has access
        }
        #endregion

        #region Methods
        #region User registering & removal etc
        /// <summary>
        /// Tries to register a session's user with the room and prepares it for entry.
        /// </summary>
        /// <param name="sessionID">The ID of the session to register.</param>
        public void registerSession(uint sessionID)
        {
            if (!this.enteringSessions.Contains(sessionID))
            {
                this.enteringSessions.Add(sessionID);
                Logging.Log("Registered session " + sessionID + " at room " + this.roomID + ".", Logging.logType.roomUserRegisterEvent);
            }
        }
        /// <summary>
        /// Tries to unregister a session's user with the room, releasing the session's room user ID and broadcoasting the 'user has left' message to the remaining room users. If the leaving of this user results in abandoning of the room, the room instance is destroyed.
        /// </summary>
        /// <param name="sessionID">The ID of the registered session to unregister.</param>
        public void unRegisterSession(uint sessionID)
        {
            if (this.enteringSessions.Contains(sessionID)) // User was entering room
                this.enteringSessions.Remove(sessionID);
            else
            {
                if (this.roomUsers.ContainsKey(sessionID)) // User was in room
                {
                    lock (this.roomUsers)
                    {
                        roomUser leavingUser = this.roomUsers[sessionID];
                        this.roomUsers.Remove(sessionID);

                        if (this.userAmount == 0) // Last user leaves the room
                        {
                            Engine.Game.Rooms.destroyRoomInstance(this.roomID);
                            return;
                        }
                        else
                        {
                            releaseRoomUnit(leavingUser.ID, leavingUser.X, leavingUser.Y);

                            this.updateUserAmount();
                            Logging.Log("Unregistered session " + sessionID + " from room " + this.roomID + ".", Logging.logType.roomUserRegisterEvent);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Notifies all room users that a given room unit has left the room and releases the map spot for the room user.
        /// </summary>
        /// <param name="roomUnitID">The room unit ID of the room unit that has left the room.</param>
        /// <param name="mapX">The current X position of the room unit on the map.</param>
        /// <param name="mapY">The current Y position of the room unit on the map.</param>
        private void releaseRoomUnit(int roomUnitID, byte mapX, byte mapY)
        {
            serverMessage Message = new serverMessage(29); // "@]"
            Message.Append(roomUnitID);
            this.sendMessage(Message);

            this.gridUnit[mapX, mapY] = false;
        }
        /// <summary>
        /// Runs through the collection of active room unit IDs to find the next unregistered identifier, registers it and returns it as an integer.
        /// </summary>
        /// <returns></returns>
        private int getFreeRoomUnitIdentifier()
        {
            int Identifier = 0;
            while (this.roomUnitIdentifiers.Contains(Identifier))
                Identifier++;

            this.roomUnitIdentifiers.Add(Identifier);
            return Identifier;
        }
        /// <summary>
        /// Activates a room user and makes it appear in the door of the room.
        /// </summary>
        /// <param name="Session">The Woodpecker.Sessions.Session object of the user that is ready to interact with the room.</param>
        public void startUser(Session Session)
        {
            if (!enteringSessions.Contains(Session.ID)) // Invalid entry
                return;

            roomUser newUser = new roomUser(Session);
            if (Session.authenticatedTeleporter == 0)
            {
                roomModel Model = this.getModel();
                if (Model == null) return;
                newUser.X = Model.doorX;
                newUser.Y = Model.doorY;
                newUser.Z = Model.doorZ;
            }
            else
            {
                Items.floorItem pItem = this.getFloorItem(Session.authenticatedTeleporter);
                if(pItem != null && pItem.Definition.Behaviour.isTeleporter)
                {
                    newUser.X = pItem.X;
                    newUser.Y = pItem.Y;
                    newUser.Z = pItem.Z;
                    Session.authenticatedTeleporter = 0;

                    this.broadcoastTeleportActivity(pItem.ID, Session.User.Username, false);
                }
                else
                    return; // Invalid item used to enter flat
            }
            newUser.ID = this.getFreeRoomUnitIdentifier();

            if (this.Information.isUserFlat)
            {
                newUser.isOwner = (Session.User.Username == this.Information.Owner 
                    || Session.User.hasFuseRight("fuse_any_room_controller"));

                newUser.hasRights = (newUser.isOwner 
                    || this.Information.superUsers 
                    || Engine.Game.Rooms.userHasRightsInRoom(Session.User.ID, this.roomID));
                
                newUser.refreshRights();
            }
            
            // User has entered
            Session.roomID = this.roomID;

            this.enteringSessions.Remove(Session.ID);
            this.roomUsers.Add(Session.ID, newUser);

            this.castRoomUnit(newUser.ToString());
            this.updateUserAmount();
        }
        /// <summary>
        /// Updates the database row of this room with the current amount of visitors.
        /// </summary>
        private void updateUserAmount()
        {
            Engine.Game.Rooms.updateRoomUserAmount(this.roomID, this.userAmount);
        }
        #endregion

        #region Room unit details
        /// <summary>
        /// Makes a room unit (so a room user, a pet, or a bot) visible to all users in the room.
        /// </summary>
        /// <param name="szDetails"></param>
        private void castRoomUnit(string szDetails)
        {
            serverMessage Cast = new serverMessage(28); // "@\"
            Cast.Append(szDetails);
            this.sendMessage(Cast);
        }
        /// <summary>
        /// Gets a string with all the details about the active room units in this room instance.
        /// </summary>
        public string getRoomUnits()
        {
            fuseStringBuilder FSB = new fuseStringBuilder();
            if (this.roomPets != null)
            {
                lock (this.roomPets)
                {
                    foreach (roomPet lPet in this.roomPets)
                    {
                        FSB.Append(lPet.ToString());
                    }
                }
            }

            if (this.roomBots != null)
            {
                lock (this.roomBots)
                {
                    foreach (roomUser lBot in this.roomBots)
                    {
                        FSB.Append(lBot.ToString());
                    }
                }
            }

            lock (this.roomUsers)
            {
                foreach (roomUser lUser in this.roomUsers.Values)
                {
                    FSB.Append(lUser.ToString());
                }
            }

            return FSB.ToString();
        }
        /// <summary>
        /// Returns a string with all the statuses of all active room units.
        /// </summary>
        public string getRoomUnitStatuses()
        {
            fuseStringBuilder FSB = new fuseStringBuilder();
            if (this.roomPets != null)
            {
                lock (this.roomPets)
                {
                    foreach (roomPet lPet in this.roomPets)
                    {
                        FSB.Append(lPet.ToStatusString());
                    }
                }
            }

            if (this.roomBots != null)
            {
                lock (this.roomBots)
                {
                    foreach (roomUser lBot in this.roomBots)
                    {
                        lBot.rotationHead = lBot.bInfo.rotation;
                        lBot.rotationBody = lBot.bInfo.rotation;
                        FSB.Append(lBot.ToStatusString());
                    }
                }
            }

            lock (this.roomUsers)
            {
                foreach (roomUser lUser in this.roomUsers.Values)
                {
                    FSB.Append(lUser.ToStatusString());
                }
            }

            return FSB.ToString();
        }
        /// <summary>
        /// Returns a string with the usernames of the users in this room.
        /// </summary>
        public string getUserList()
        {
            fuseStringBuilder FSB = new fuseStringBuilder();
            if (!this.Information.isUserFlat)
            {
                FSB.appendWired(this.roomID);
                lock (this.roomUsers)
                {
                    FSB.appendWired(this.roomUsers.Count);
                    foreach (roomUser lRoomUser in this.roomUsers.Values)
                    {
                        FSB.appendClosedValue(lRoomUser.Session.User.Username);
                    }
                }
            }

            return FSB.ToString();
        }
        #endregion

        #region Room unit collection management
        /// <summary>
        /// Returns the roomUser object of a session. If there is no room user found, then null is returned.
        /// </summary>
        /// <param name="sessionID">The ID of the session to get the room user of.</param>
        public roomUser getRoomUser(uint sessionID)
        {
            if (roomUsers.ContainsKey(sessionID))
                return roomUsers[sessionID];
            else
                return null;
        }
        /// <summary>
        /// Returns the roomUser object that has a given room unti ID. If there is no room user found, then null is returned.
        /// </summary>
        /// <param name="roomUnitID">The room unit identifier that was assigned to the requested room user.</param>
        public roomUser getRoomUser(int roomUnitID)
        {
            lock (this.roomUsers)
            {
                foreach (roomUser lRoomUser in this.roomUsers.Values)
                {
                    if (lRoomUser.ID == roomUnitID)
                        return lRoomUser;
                }
            }

            return null;
        }
        /// <summary>
        /// Tries to return the room user with a given username from the room user collection. If the room user was not found, then null is returned.
        /// </summary>
        /// <param name="Username">The username of the room user to get. Case sensitive.</param>
        public roomUser getRoomUser(string Username)
        {
            lock (roomUsers)
            {
                foreach (roomUser lRoomUser in roomUsers.Values)
                {
                    if (lRoomUser.Session.User.Username == Username)
                        return lRoomUser;
                }
            }
            return null;
        }
        public roomUnit getRoomUnitOnTile(byte X, byte Y)
        {
            if (this.tileBlockedByRoomUnit(X, Y))
            {
                if (this.roomPets != null)
                {
                    foreach (roomPet lPet in this.roomPets)
                    {
                        if (lPet.X == X && lPet.Y == Y)
                            return lPet;
                    }
                }

                foreach (roomUser lUser in this.roomUsers.Values)
                {
                    if (lUser.X == X && lUser.Y == Y)
                        return lUser;
                }
            }

            return null;
        }
        /// <summary>
        /// Returns the room unit ID of the room user of a given session ID. -1 is returned if the room user is not found.
        /// </summary>
        /// <param name="sessionID">The session ID of the room user to get the room unit ID of.</param>
        /// <returns></returns>
        public int getSessionRoomUnitID(uint sessionID)
        {
            if (this.roomUsers.ContainsKey(sessionID))
                return this.roomUsers[sessionID].ID;
            else
                return -1;
        }
        /// <summary>
        /// Returns true if a given session has rights in this room.
        /// </summary>
        /// <param name="sessionID">The session ID of the session to check.</param>
        public bool sessionHasRights(uint sessionID)
        {
            roomUser pUser = this.getRoomUser(sessionID);
            return (pUser != null && pUser.hasRights);
        }
        /// <summary>
        /// Returns true if a given session has flat admin rights ('room owner') in this room.
        /// </summary>
        /// <param name="sessionID">The session ID of the session to check.</param>
        public bool sessionHasFlatAdmin(uint sessionID)
        {
            roomUser pUser = this.getRoomUser(sessionID);
            return (pUser != null && pUser.isOwner);
        }
        #endregion

        #region Chat
        public void sendTalk(int sourceID, byte sourceX, byte sourceY, string Text)
        {
            string sFullMessage = "";
            
            serverMessage Message = new serverMessage(24); // "@X"
            Message.appendWired(sourceID);
            Message.appendClosedValue(Text);
            sFullMessage = Message.ToString();
            Message = null;

            lock (roomUsers)
            {
                foreach (roomUser lRoomUser in roomUsers.Values)
                {
                    int distX = Math.Abs(sourceX - lRoomUser.X) - 1;
                    int distY = Math.Abs(sourceY - lRoomUser.Y) - 1;

                    if (distX < 9 && distY < 9) // User can hear
                    {
                        if (distX <= 6 && distY <= 6) // User can hear full message
                            lRoomUser.Session.gameConnection.sendMessage(sFullMessage);
                        else // User can hear garbled message
                        {
                            Message = new serverMessage(24); // "@X"
                            Message.appendWired(sourceID);

                            int garbleIntensity = distX;
                            if (distY < distX)
                                garbleIntensity = distY;
                            garbleIntensity -= 4;

                            string garbledText = Text;
                            stringFunctions.garbleText(ref garbledText, garbleIntensity);

                            Message.appendClosedValue(garbledText);
                            lRoomUser.Session.gameConnection.sendMessage(Message);
                            Message = null;
                        }
                    }
                }
            }
        }
        public void sendShout(int sourceID, byte sourceX, byte sourceY, string Text)
        {
            serverMessage Message = new serverMessage(26); // "@Z"
            Message.appendWired(sourceID);
            Message.appendClosedValue(Text);

            sendMessage(Message);
            // TODO: head rotation
        }
        public void animateTalkingUnit(roomUnit pUnit, ref string Text, bool applyEmotes)
        {
            string[] Words = Text.Split(' ');
            string Gesture = null;
                
            if (applyEmotes)
            {
                foreach (string Word in Words)
                {
                    if (Gesture != null)
                        break;

                    switch (Word.ToLower())
                    {
                        case ":)":
                        case ":-)":
                        case ":d":
                        case ":-d":
                        case ":p":
                        case ":-p":
                        case ";)":
                        case ";-)":
                            Gesture = "sml";
                            break;

                        case ":o":
                            Gesture = "srp";
                            break;

                        case ":s":
                        case ":-s":
                        case ":(":
                        case ":-(":
                        case ":'(":
                        case ":'-(":
                            Gesture = "sad";
                            break;

                        case ":@":
                        case ">:(":
                        case ">:-(":
                            Gesture = "agr";
                            break;
                    }
                }
            }

            int wordAmount = Words.Length;
            if (Gesture != null)
            {
                pUnit.addStatus("gest", "gest", Gesture, 5, null, 0, 0);
                if (wordAmount == 1) // Only word is the gesture emote
                    return;
            }


            int speakSecs = 1;
            if (wordAmount > 1)
                speakSecs = wordAmount / 2;
            else if (wordAmount > 5)
                speakSecs = 5;

            pUnit.addStatus("talk", "talk", null, speakSecs, null, 0, 0);
        }
        #endregion

        #region In-room actions (doorbell, give/take rights, kick etc)
        /// <summary>
        /// Broadcoasts the message that a user is waiting for a doorbell response. This message is only broadcoasted to users who have room rights, and True is returned if at least one room user with room rights has received it.
        /// </summary>
        /// <param name="Username">The username of the user.</param>
        public bool castDoorbellUser(string Username)
        {
            bool isHeard = false;
            
            serverMessage Notify = new serverMessage(91); // "A["
            Notify.Append(Username);

            lock (roomUsers)
            {
                foreach (roomUser lRoomUser in roomUsers.Values)
                {
                    if (lRoomUser.hasRights)
                    {
                        lRoomUser.Session.gameConnection.sendMessage(Notify);
                        isHeard = true;
                    }
                }
            }

            return isHeard;
        }
        /// <summary>
        /// Answers the doorbell of a ringing user. If the ringing user is allowed to go in, a roomReactor is registered and the required permissions are set.
        /// </summary>
        /// <param name="sessionID">The ID of the session that answers the doorbell.</param>
        /// <param name="ringingUsername">The username of the user that rings the doorbell.</param>
        /// <param name="letIn">True if the ringing user should be allowed to go in, false otherwise.</param>
        public void answerDoorbell(uint sessionID, string ringingUsername, bool letIn)
        {
            if (roomUsers[sessionID].hasRights) // Has permissions to let this user in
            {
                lock (enteringSessions)
                {
                    foreach (uint lSessionID in enteringSessions)
                    {
                        Session lSession = Engine.Sessions.getSession(lSessionID);
                        if (lSession != null && lSession.User.Username == ringingUsername)
                        {
                            int messageID = 0;
                            if (letIn)
                            {
                                lSession.waitingFlat = 0;
                                lSession.authenticatedFlat = this.roomID;
                                messageID = 41; // "@i"
                            }
                            else
                                messageID = 131; // "BC"

                            lSession.gameConnection.sendMessage(new serverMessage(messageID));
                            return;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Updates the badge status of a given room user in the room.
        /// </summary>
        /// <param name="sessionID">The session ID of the session where the target room user belongs to.</param>
        public void updateUserBadge(uint sessionID)
        {
            serverMessage Notify = new serverMessage(228); // "Cd"

            roomUser rUser = roomUsers[sessionID];
            Notify.appendWired(rUser.ID);
            if (rUser.Session.User.Badge.Length > 0)
                Notify.appendClosedValue(rUser.Session.User.Badge);

            sendMessage(Notify);
        }
        /// <summary>
        /// Casts a room kick at the room, removing & alerting all the users below the caster rank from the room.
        /// </summary>
        /// <param name="casterRole">The Woodpecker.Game.Users.userRole enum value representing the role of the caster of the roomkick. All room users with a lower role will be removed from the room.</param>
        /// <param name="Message">The alert to sent to users that will be removed. No alert will be sent if this argument is supplied as a blank string.</param>
        public void castRoomKick(userRole casterRole, string Message)
        {
            int iCasterRole = (int)casterRole;
            lock (roomUsers)
            {
                List<Session> Victims = new List<Session>();
                foreach (roomUser lRoomUser in roomUsers.Values)
                {
                    if ((int)lRoomUser.Session.User.Role < iCasterRole)
                        Victims.Add(lRoomUser.Session);
                }

                foreach (Session Victim in Victims)
                {
                    Victim.kickFromRoom(Message);
                }
            }
        }
        #endregion
        #endregion
    }
}
