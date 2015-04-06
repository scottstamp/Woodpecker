using System;
using System.Data;
using System.Threading;
using System.Collections.Generic;

using Woodpecker.Storage;
using Woodpecker.Specialized.Text;
using Woodpecker.Net.Game.Messages;

using Woodpecker.Game.Items;
using Woodpecker.Game.Items.Pets;
using Woodpecker.Game.Rooms.Units;
using Woodpecker.Game.Rooms.Pathfinding;

namespace Woodpecker.Game.Rooms.Instances.Interaction
{
    public partial class flatReactor : roomReactor
    {
        /// <summary>
        /// 128 - "B@"
        /// </summary>
        public void GETPETSTAT()
        {
            int petID = int.Parse(Request.getParameter(0).Split(Convert.ToChar(4))[0]);

            roomPet pPet = Session.roomInstance.getRoomPet(petID);
            if (pPet != null)
            {
                Response.Initialize(210); // "CR"
                Response.appendWired(pPet.ID);
                Response.appendWired(pPet.Information.Age);
                Response.appendWired((int)pPet.Information.Hunger);
                Response.appendWired((int)pPet.Information.Thirst);
                Response.appendWired((int)pPet.Information.Happiness);
                Response.appendWired((int)pPet.Information.Energy);
                Response.appendWired((int)pPet.Information.Friendship);

                sendResponse();
            }
        }
    }
}

namespace Woodpecker.Game.Rooms.Instances
{
    public partial class roomInstance
    {
        #region Fields
        /// <summary>
        /// System.Collections.Generic.List (roomPet) collection with all the roomPet object of the active room pets in this room. This collection is not initialized if this room is a public space.
        /// </summary>
        private List<roomPet> roomPets;
        /// <summary>
        /// Holds true if there is atleast one room pet in the room.
        /// </summary>
        private bool hasPets;
        /// <summary>
        /// The total amount of room pets in this room instance as an integer.
        /// </summary>
        public int petAmount
        {
            get
            {
                if (hasPets && this.roomPets != null)
                    return this.roomPets.Count;
                else
                    return 0;
            }
        }
        #endregion

        #region Methods
        #region Room pet collection management
        /// <summary>
        /// Tries to load a pet from the database and makes it visible in the room. If the X and Y parameters are not both zero, then the pet will be placed at the new position.
        /// </summary>
        /// <param name="nestID">The database ID of the nest item of the pet to load.</param>
        /// <param name="newX">The new X position of the pet. Supply 0 to set the last saved X position of the pet.</param>
        /// <param name="newY">The new Y position of the pet. Supply 0 to set the last saved Y position of the pet.</param>
        public void loadRoomPet(int nestID, byte newX, byte newY)
        {
            if (this.roomPets != null)
            {
                virtualPetInformation pInfo = Engine.Game.Items.getPetInformation(nestID);
                if (pInfo != null)
                {
                    roomPet pPet = new roomPet(pInfo);
                    pPet.ID = this.getFreeRoomUnitIdentifier();

                    if (newX > 0 || newY > 0) // New placement from user hand
                    {
                        pPet.X = newX;
                        pPet.Y = newY;
                    }
                    else
                    {
                        pPet.X = pPet.Information.lastX;
                        pPet.Y = pPet.Information.lastY;
                    }

                    pPet.Z = this.gridHeight[pPet.X, pPet.Y];
                    this.gridUnit[pPet.X, pPet.Y] = true;
                    this.roomPets.Add(pPet);

                    if (newX > 0 || newY > 0) // New placement during instance lifetime
                        castRoomUnit(pPet.ToString());
                }
            }
        }
        /// <summary>
        /// Removes a pet with a given nest ID from the room pet collection, updates the pet information in the database, releases it's map spot and makes it disappear for clients.
        /// </summary>
        /// <param name="nestID">The database ID of the nest item of the pet.</param>
        /// <param name="removedFromRoom">Supply true if this pet is removed from the room by someone with flat admin, and it's coordinates should be reset.</param>
        public void unloadRoomPet(int nestID, bool removedFromRoom)
        {
            if (this.roomPets != null)
            {
                for (int x = 0; x < this.roomPets.Count; x++)
                {
                    roomPet pPet = this.roomPets[x];
                    if (pPet.Information.ID == nestID)
                    {
                        this.releaseRoomUnit(pPet.ID, pPet.X, pPet.Y);

                        if (removedFromRoom) // Removed from room, reset coordinates
                        {
                            pPet.X = 0;
                            pPet.Y = 0;
                            // TODO: less happiness?
                        }
                        pPet.Information.Update();

                        this.roomPets.RemoveAt(x);
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// Tries to return the roomPet object of a pet of a given pet/nest item ID. If the pet is not found in the collection, then null is returned.
        /// </summary>
        /// <param name="petID">The database ID of the pet and it's nest item.</param>
        public roomPet getRoomPet(int petID)
        {
            lock (this.roomPets)
            {
                foreach (roomPet lPet in this.roomPets)
                {
                    if (lPet.Information.ID == petID)
                        return lPet;
                }
            }

            return null;
        }
        #endregion

        #region Artificial intelligence
        private void runPetActors()
        {
            foreach (roomPet lPet in this.roomPets)
            {
                if (new Random().Next(0, 10) == 2)
                {
                    byte tileX = Convert.ToByte(new Random(DateTime.Now.Millisecond).Next(0, this.gridUnit.GetUpperBound(0)));
                    byte tileY = Convert.ToByte(new Random(DateTime.Now.Millisecond + lPet.ID).Next(0, this.gridUnit.GetUpperBound(1)));

                    lPet.goalX = tileX;
                    lPet.goalY = tileY;
                    lPet.Moves = true;
                    lPet.requiresUpdate = true;
                }
            }
        }
        #endregion
        #endregion
    }
}
