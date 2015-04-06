using System;
using System.Threading;

using Woodpecker.Core;
using Woodpecker.Net.Game.Messages;

using Woodpecker.Game.Rooms.Units;
using Woodpecker.Game.Rooms.Instances.Interaction;

namespace Woodpecker.Game.Rooms.Instances
{
    /// <summary>
    /// Represents an instance of a room.
    /// </summary>
    public partial class roomInstance
    {
        #region Fields
        /// <summary>
        /// The database ID of the room this room instance represents.
        /// </summary>
        public int roomID;
        /// <summary>
        /// The Woodpecker.Game.Rooms.roomInformation object containing information about the room.
        /// </summary>
        public roomInformation Information;
        private Thread roomWorker;
        #endregion

        #region Constructors
        /// <summary>
        /// Tries to initialize the room instance for a given room. The room is destroyed on errors.
        /// </summary>
        /// <param name="ID">The database ID of the room to construct an instance of.</param>
        public roomInstance(int ID)
        {
            this.roomID = ID;
            this.Information = Engine.Game.Rooms.getRoomInformation(ID);
            if (this.Information == null) // Invalid room
            {
                Engine.Game.Rooms.destroyRoomInstance(ID);
                return;
            }

            this.loadItems();
            this.generateFloorMap();
            this.gridUnit = new bool[this.gridState.GetUpperBound(0) + 1, this.gridState.GetUpperBound(1) + 1];
            
            if (this.Information.isUserFlat)
            {
                foreach (Items.floorItem lItem in this.floorItems)
                {
                    if (lItem.Definition.Behaviour.canStandOnTop && lItem.Definition.Sprite == "nest")
                        this.loadRoomPet(lItem.ID, 0, 0);
                }
            }

            this.loadRoomBot(roomID, 0, 0);

            this.roomWorker = new Thread(workerLoop);
            this.roomWorker.Start();
            Logging.Log("Initialized room instance for room " + this.roomID + ".", Logging.logType.roomInstanceEvent);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Removes all users from the room and nullifies all objects.
        /// </summary>
        public void Destroy()
        {
            this.roomWorker.Abort();
            this.roomWorker = null;

            if (this.userAmount > 0) // Users in room
            {
                // Broadcoast 'kick' message
                serverMessage Notify = new serverMessage(18); // "@R"
                sendMessage(Notify);
                Notify = null;

                Type pCommonReactor = typeof(roomReactor);// new roomReactor().GetType();
                Type pFlatReactor = typeof(flatReactor);
                
                lock (this.roomUsers)
                {
                    foreach (roomUser lRoomUser in this.roomUsers.Values)
                    {
                        if (this.Information.isUserFlat)
                            lRoomUser.Session.gameConnection.reactorHandler.unRegister(pFlatReactor);
                        else
                            lRoomUser.Session.gameConnection.reactorHandler.unRegister(pCommonReactor);
                        
                        lRoomUser.Session.roomID = 0;
                        lRoomUser.Session.roomInstance = null;
                    }
                }
                this.roomUsers.Clear();
            }
            this.updateUserAmount();
            this.roomUsers = null;

            this.roomUnitIdentifiers.Clear(); this.roomUnitIdentifiers = null;
            this.enteringSessions.Clear(); this.enteringSessions = null;

            this.saveItems();
            this.unloadItems();

            this.Information = null;
            this.gridState = null;
            this.gridHeight = null;
            this.gridUnit = null;
        }
        /// <summary>
        /// Sends a string with a server message to all active room users.
        /// </summary>
        /// <param name="Message">The message string to send. Use char1 for breaking messages.</param>
        public void sendMessage(string Message)
        {
            Logging.Log("Sending to room " + this.roomID + ": " + Message, Logging.logType.serverMessageEvent);
            lock (roomUsers)
            {
                foreach (roomUser lUser in this.roomUsers.Values)
                {
                    lUser.Session.gameConnection.sendMessage(Message);
                }
            }
        }
        /// <summary>
        /// Sends a serverMessage object to all active room users in the room.
        /// </summary>
        /// <param name="Message">The serverMessage object to send.</param>
        public void sendMessage(serverMessage Message)
        {
            this.sendMessage(Message.ToString());
        }

        #region Delayed message sending
        /// <summary>
        /// Sends a string with a server message to all active room users, after a given delay in milliseconds.
        /// </summary>
        /// <param name="Message">The message string to send. Use char1 for breaking messages.</param>
        /// <param name="delayMilliseconds">The amount of milliseconds to sleep before sending the message.</param>
        public void sendMessage(string Message, int delayMilliseconds)
        {
            new broadcoastDelayedMessage(this.sendDelayedMessage).BeginInvoke(Message, delayMilliseconds, null, null);
        }
        /// <summary>
        /// Sends a serverMessage object to all active room users in the room, after a given delay in milliseconds.
        /// </summary>
        /// <param name="Message">The serverMessage object to send.</param>
        /// <param name="delayMilliseconds">The amount of milliseconds to sleep before sending the message.</param>
        public void sendMessage(serverMessage Message, int delayMilliseconds)
        {
            this.sendMessage(Message.ToString(), delayMilliseconds);
        }
        private delegate void broadcoastDelayedMessage(string Message, int delayMilliseconds);
        /// <summary>
        /// Sends a string with a server message to all active room users, after a given delay in milliseconds. Invoke this method on a separate thread.
        /// </summary>
        /// <param name="Message">The message string to send. Use char1 for breaking messages.</param>
        /// <param name="delayMilliseconds">The amount of milliseconds to sleep before sending the message.</param>
        private void sendDelayedMessage(string Message, int delayMilliseconds)
        {
            Thread.Sleep(delayMilliseconds);
            this.sendMessage(Message);
        }
        #endregion

        private void workerLoop()
        {
            try
            {
                while (this.roomWorker != null)
                {
                    if (this.hasBots)
                        this.runBotActors();

                    if (this.hasPets)
                        this.runPetActors();

                    this.refreshRoomUnitStatuses();
                    if (this.hasRollers)
                        this.runRollers();

                    Thread.Sleep(460); // 460
                }
            }
            catch (ThreadAbortException) { }
        }
        #endregion
    }
}
