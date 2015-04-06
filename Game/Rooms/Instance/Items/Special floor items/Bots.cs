using System;
using System.Data;
using System.Threading;
using System.Collections.Generic;

using Woodpecker.Storage;
using Woodpecker.Specialized.Text;
using Woodpecker.Net.Game.Messages;

using Woodpecker.Game.Items;
using Woodpecker.Game.Items.Bots;
using Woodpecker.Game.Rooms.Units;
using Woodpecker.Game.Rooms.Pathfinding;

namespace Woodpecker.Game.Rooms.Instances.Interaction
{
    public partial class flatReactor : roomReactor
    {
        /// <summary>
        /// 128 - "B@"
        /// </summary>
        /*public void GETPETSTAT()
        {
            int botID = int.Parse(Request.getParameter(0).Split(Convert.ToChar(4))[0]);

            roomBot pBot = Session.roomInstance.getRoomBot(botID);
            if (pBot != null)
            {
                Response.Initialize(210); // "CR"
                Response.appendWired(pBot.ID);
                Response.appendWired(pBot.Information.Age);
                Response.appendWired((int)pBot.Information.Hunger);
                Response.appendWired((int)pBot.Information.Thirst);
                Response.appendWired((int)pBot.Information.Happiness);
                Response.appendWired((int)pBot.Information.Energy);
                Response.appendWired((int)pBot.Information.Friendship);

                sendResponse();
            }
        }*/
    }
}

namespace Woodpecker.Game.Rooms.Instances
{
    public partial class roomInstance
    {
        #region Fields
        /// <summary>
        /// System.Collections.Generic.List (roomBot) collection with all the roomBot object of the active room bots in this room. This collection is not initialized if this room is a public space.
        /// </summary>
        private List<roomUser> roomBots;
        /// <summary>
        /// Holds true if there is atleast one room bot in the room.
        /// </summary>
        private bool hasBots;
        /// <summary>
        /// The total amount of room bots in this room instance as an integer.
        /// </summary>
        public int botAmount
        {
            get
            {
                if (hasBots && this.roomBots != null)
                    return this.roomBots.Count;
                else
                    return 0;
            }
        }

        virtualBotInformation bInfo;
        #endregion

        #region Methods
        #region Room bot collection management
        /// <summary>
        /// Tries to load a bot from the database and makes it visible in the room. If the X and Y parameters are not both zero, then the bot will be placed at the new position.
        /// </summary>
        /// <param name="nestID">The database ID of the nest item of the bot to load.</param>
        /// <param name="newX">The new X position of the bot. Supply 0 to set the last saved X position of the bot.</param>
        /// <param name="newY">The new Y position of the bot. Supply 0 to set the last saved Y position of the bot.</param>
        public void loadRoomBot(int botID, byte newX, byte newY)
        {
            if (this.roomBots != null)
            {
                bInfo = Engine.Game.Items.getBotInformation(botID);
                if (bInfo != null)
                {
                    roomUser bUser = new roomUser(bInfo);
                    bUser.ID = this.getFreeRoomUnitIdentifier();
                        
                    bUser.X = bInfo.startX;
                    bUser.Y = bInfo.startY;

                    bUser.Z = this.gridHeight[bUser.X, bUser.Y];
                    this.gridUnit[bUser.X, bUser.Y] = true;
                    this.roomBots.Add(bUser);
                    //this.roomUsers.Add((uint)bUser.ID, bUser);
                    Woodpecker.Core.Logging.Log("We tried jeeves.");
                    castRoomUnit(bUser.ToString());
                    refreshRoomUnitOnTile((byte)0, (byte)0);
                    refreshRoomUnitOnTile(bInfo.startX, bInfo.startY);
                }
            }
        }

        /// <summary>
        /// Removes a bot with a given nest ID from the room bot collection, updates the bot information in the database, releases it's map spot and makes it disappear for clients.
        /// </summary>
        /// <param name="nestID">The database ID of the nest item of the bot.</param>
        /// <param name="removedFromRoom">Supply true if this bot is removed from the room by someone with flat admin, and it's coordinates should be reset.</param>
        public void unloadRoomBot(int botID, bool removedFromRoom)
        {
            roomUser pBot = roomBots[botID];
            foreach (roomUser bUser in this.roomBots)
            {
                if (bUser.bInfo.ID == botID)
                {
                    this.releaseRoomUnit(pBot.ID, pBot.X, pBot.Y);

                    this.roomBots.Remove(bUser);
                }
            }
        }
        /// <summary>
        /// Tries to return the roomBot object of a bot of a given bot/nest item ID. If the bot is not found in the collection, then null is returned.
        /// </summary>
        /// <param name="botID">The database ID of the bot and it's nest item.</param>
        public roomUser getRoomBot(int botID)
        {
            lock (this.roomBots)
            {
                foreach (roomUser lBot in this.roomBots)
                {
                    if (lBot.bInfo.roomID == botID)
                        return lBot;
                }
            }

            return null;
        }
        #endregion

        #region Artificial intelligence
        private void runBotActors()
        {
            try
            {
                foreach (roomUser lBot in this.roomBots)
                {
                    if (new Random().Next(0, 10) == 2)
                    {
                        int tempX;
                        int tempY;

                        if (lBot.bInfo.minX == (byte)99 || lBot.bInfo.maxX == (byte)99) // Free Roam
                            tempX = new Random(DateTime.Now.Millisecond).Next(0, this.gridUnit.GetUpperBound(0));
                        else
                            tempX = new Random(DateTime.Now.Millisecond).Next(lBot.bInfo.minX, lBot.bInfo.maxX + 1);

                        if (lBot.bInfo.minY == (byte)99 || lBot.bInfo.maxY == (byte)99) // Free Roam
                            tempY = new Random(DateTime.Now.Millisecond).Next(0, this.gridUnit.GetUpperBound(1));
                        else
                            tempY = new Random(DateTime.Now.Millisecond).Next(lBot.bInfo.minY, lBot.bInfo.maxY + 1);

                        byte tileX = Convert.ToByte(tempX);
                        byte tileY = Convert.ToByte(tempY);

                        lBot.goalX = (byte)tempX;
                        lBot.goalY = (byte)tempY;
                        lBot.rotationBody = lBot.bInfo.rotation;
                        lBot.rotationHead = lBot.bInfo.rotation;
                        lBot.Moves = true;
                        lBot.requiresUpdate = true;
                    }
                }
            }
            catch (Exception ex) { /*this.loadRoomBot(this.roomID, 0, 0);*/ }
        }
        #endregion
        #endregion
    }
}
