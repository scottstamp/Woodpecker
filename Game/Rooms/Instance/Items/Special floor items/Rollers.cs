using System;
using System.Text;
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

namespace Woodpecker.Game.Rooms.Instances
{
    public partial class roomInstance
    {
        #region Fields
        private const int rollerDelaySeconds = 2;
        
        /// <summary>
        /// Holds true if there is atleast one roller item in the room.
        /// </summary>
        private bool hasRollers;

        private double mRollerTimer = DateTime.Now.TimeOfDay.TotalSeconds + rollerDelaySeconds;
        private int mRollerDay = DateTime.Today.DayOfYear;

        /// <summary>
        /// The total amount of 'roller' items in this room instance.
        /// </summary>
        public int rollerAmount
        {
            get
            {
                int i = 0;
                foreach (floorItem lItem in this.floorItems)
                {
                    if (lItem.Definition.Behaviour.isRoller)
                        i++;
                }

                return i;
            }
        }
        #endregion

        #region Methods
        private void runRollers()
        {
            if (mRollerTimer > DateTime.Now.TimeOfDay.TotalSeconds && mRollerDay == DateTime.Today.DayOfYear)
                return;

            StringBuilder Updates = new StringBuilder();
            List<int> movedRoomUnitIDs = new List<int>();
            List<int> movedItemIDs = new List<int>();

            try
            {
                foreach (floorItem lRoller in this.floorItems)
                {
                    if (!lRoller.Definition.Behaviour.isRoller)
                        continue; // Not a roller item

                    #region General
                    // Workout direction for roller
                    byte nextX = lRoller.X;
                    byte nextY = lRoller.Y;
                    if (lRoller.Rotation == 0)
                        nextY--;
                    else if (lRoller.Rotation == 2)
                        nextX++;
                    else if (lRoller.Rotation == 4)
                        nextY++;
                    else if (lRoller.Rotation == 6)
                        nextX--;

                    if (!tileExists(nextX, nextY) || tileBlockedByRoomUnit(nextX, nextY))
                        continue; // Can't roll off room map / on room unit
                    #endregion

                    #region Get objects on current tile and verify
                    roomUnit pUnit = this.getRoomUnitOnTile(lRoller.X, lRoller.Y); // Get room unit on this roller
                    List<floorItem> pItems = new List<floorItem>();
                    foreach (floorItem lItem in this.getFloorItems(lRoller.X, lRoller.Y))
                    {
                        if (!lItem.Definition.Behaviour.isRoller && !movedItemIDs.Contains(lItem.ID))
                            pItems.Add(lItem);
                    }

                    if (pUnit != null && (pUnit.Moves || movedRoomUnitIDs.Contains(pUnit.ID)))
                    {
                        pUnit = null; // Invalid room unit
                    }

                    if (pUnit == null && pItems.Count == 0)
                        continue; // No items on roller and no room unit aswell
                    #endregion

                    // Get items on next tile and perform some checks
                    bool nextTileIsRoller = false;
                    bool canMoveItems = (this.gridState[nextX, nextY] == roomTileState.Free);
                    bool canMoveUnit = canMoveItems;
                    
                    bool nextTileUnitInteractiveStance = false;

                    foreach (floorItem lItem in this.getFloorItems(nextX, nextY))
                    {
                        if (lItem.Definition.Behaviour.isRoller)
                        {
                            nextTileIsRoller = true;
                            continue;
                        }

                        if (lItem.Definition.Behaviour.isSolid)
                        {
                            canMoveItems = false;
                            canMoveUnit = false;

                            if (lItem.Definition.Behaviour.isDoor)
                            {
                                if (lItem.customData == "O") // Door is open
                                    canMoveUnit = true; // Can move unit in door
                            }
                        }
                        else if (pUnit != null && lItem.Definition.isInteractiveStance)
                        {
                            nextTileUnitInteractiveStance = true;
                            if(!nextTileIsRoller)
                                canMoveUnit = true;
                        }
                    }

                    if (!canMoveItems) // Can't move items
                    {
                        if (!canMoveUnit) // Can't move unit aswell
                            continue; // Can't run this roller

                        pItems.Clear(); // Clear items to move
                    }

                    #region Generate notification and move objects
                    serverMessage eventNotification = new serverMessage(230); // "Cf"
                    eventNotification.appendWired(lRoller.X);
                    eventNotification.appendWired(lRoller.Y);
                    eventNotification.appendWired(nextX);
                    eventNotification.appendWired(nextY);

                    eventNotification.appendWired(pItems.Count);
                    foreach (floorItem lItem in pItems)
                    {
                        float nextZ = lItem.Z;
                        if (!nextTileIsRoller)
                            nextZ -= lRoller.Definition.topHeight;

                        eventNotification.appendWired(lItem.ID);
                        eventNotification.appendClosedValue(stringFunctions.formatFloatForClient(lItem.Z));
                        eventNotification.appendClosedValue(stringFunctions.formatFloatForClient(nextZ));

                        lItem.X = nextX;
                        lItem.Y = nextY;
                        lItem.Z = nextZ;
                        lItem.requiresUpdate = true;

                        movedItemIDs.Add(lItem.ID);
                        generateFloorMap();
                    }
                    
                    eventNotification.appendWired(lRoller.ID);

                    if (pUnit != null) // Room unit lifting with roller
                    {
                        float nextZ = pUnit.Z;
                        if (!nextTileIsRoller)
                            nextZ -= lRoller.Definition.topHeight;

                        pUnit.X = nextX;
                        pUnit.Y = nextY;
                        pUnit.Z = nextZ;
                        pUnit.requiresUpdate = true;

                        if (!nextTileIsRoller || nextTileUnitInteractiveStance)
                            this.setRoomUnitTileState(pUnit);

                        this.gridUnit[lRoller.X, lRoller.Y] = false;
                        this.gridUnit[pUnit.X, pUnit.Y] = true;

                        eventNotification.appendWired(2);
                        eventNotification.appendWired(pUnit.ID);
                        eventNotification.appendClosedValue(stringFunctions.formatFloatForClient(pUnit.Z));
                        eventNotification.appendClosedValue(stringFunctions.formatFloatForClient(nextZ));

                        movedRoomUnitIDs.Add(pUnit.ID);
                    }
                    #endregion

                    Updates.Append(eventNotification.ToString());
                }

                if (Updates.Length > 0)
                {
                    this.sendMessage(Updates.ToString());
                }

                mRollerTimer = DateTime.Now.TimeOfDay.TotalSeconds + rollerDelaySeconds;
                mRollerDay = DateTime.Today.DayOfYear;
            }
            catch (Exception ex)
            {
                Core.Logging.Log("Rollers in room instance of room " + this.roomID + " crashed, exception: " + ex.Message);
                this.hasRollers = false;
            }
        }
        #endregion
    }
}
