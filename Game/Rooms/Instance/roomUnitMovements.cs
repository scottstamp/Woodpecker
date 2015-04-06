using System;
using System.Drawing;
using System.Globalization;
using System.Collections.Generic;

using Woodpecker.Specialized.Text;
using Woodpecker.Net.Game.Messages;

using Woodpecker.Game.Items;
using Woodpecker.Game.Rooms.Units;
using Woodpecker.Game.Rooms.Pathfinding;

namespace Woodpecker.Game.Rooms.Instances
{
    public partial class roomInstance
    {
        #region Methods
        private void refreshRoomUnitStatuses()
        {
            bool isUpdated = false;
            serverMessage Updates = new serverMessage(34); // "@b"

            if (hasPets)
            {
                lock (this.roomPets)
                {
                    foreach (roomPet lPet in this.roomPets)
                    {
                        if (!lPet.requiresUpdate)
                            continue;
                        lPet.requiresUpdate = false;

                        if (lPet.Moves)
                            moveRoomUnit(lPet);

                        Updates.Append(lPet.ToStatusString());
                        isUpdated = true;

                        if (lPet.nextX != 0 && lPet.nextY != 0)
                        {
                            lPet.X = lPet.nextX;
                            lPet.Y = lPet.nextY;
                            lPet.Z = lPet.nextZ;
                        }
                    }
                }
            }

            if (hasBots)
            {
                try
                {
                    lock (this.roomBots)
                    {
                        foreach (roomUser lBot in this.roomBots)
                        {
                            if (!lBot.requiresUpdate)
                                continue;
                            lBot.requiresUpdate = false;

                            if (lBot.Moves)
                                moveRoomUnit(lBot);

                            Updates.Append(lBot.ToStatusString());
                            isUpdated = true;

                            if (lBot.nextX != 0 && lBot.nextY != 0)
                            {
                                lBot.X = lBot.nextX;
                                lBot.Y = lBot.nextY;
                                lBot.Z = lBot.nextZ;
                                lBot.rotationHead = lBot.bInfo.rotation;
                                lBot.rotationBody = lBot.bInfo.rotation;
                            }
                        }
                    }
                }
                catch (Exception ex) { /*this.loadRoomBot(this.roomID, 0, 0);*/ }
            }

            lock (this.roomUsers)
            {
                foreach (roomUser lUser in this.roomUsers.Values)
                {
                    if (!lUser.requiresUpdate)
                        continue;
                    lUser.requiresUpdate = false;

                    if (lUser.Moves)
                        moveRoomUnit(lUser);

                    Updates.Append(lUser.ToStatusString());
                    isUpdated = true;

                    if(lUser.nextX != 0)
                    {
                        lUser.X = lUser.nextX;
                        lUser.Y = lUser.nextY;
                        lUser.Z = lUser.nextZ;
                    }
                }
            }

            if (isUpdated)
                this.sendMessage(Updates);
        }
        public void requestStartRoomUnitWalk(uint sessionID, byte goalX, byte goalY, bool allowOverideNextTile)
        {
            roomUser pUser = this.getRoomUser(sessionID);
            if (pUser != null && !pUser.Clamped) // Able to move
            {
                if(!allowOverideNextTile)
                    pUser.Path.Clear();
                if (tileExists(goalX, goalY) && !tileBlockedByRoomUnit(goalX, goalY))
                {
                    pUser.goalX = goalX;
                    pUser.goalY = goalY;
                    pUser.allowOverideNextTile = allowOverideNextTile;
                    pUser.Moves = true;
                }
                else
                    pUser.Moves = false;
                pUser.requiresUpdate = true;
            }
        }
        private void moveRoomUnit(roomUnit pUnit)
        {
            if (pUnit.Path.Count > 0)
            {
                shortenRoomUnitPath(pUnit); // Cut corners etc
                blisterMoleNode nextTile = pUnit.Path[0];

                bool nextTileNoRoomUnit = !this.gridUnit[nextTile.X, nextTile.Y];
                bool nextTileExists = this.tileExists(nextTile.X, nextTile.Y);

                if (pUnit.allowOverideNextTile || (nextTileNoRoomUnit && nextTileExists && this.gridState[nextTile.X, nextTile.Y] == roomTileState.Free))
                {
                    moveRoomUnitToTile(pUnit, ref nextTile);
                    pUnit.allowOverideNextTile = false;
                    return;
                }
                else
                {

                    bool workInteractiveTile = false;

                    if (this.gridState[pUnit.goalX, pUnit.goalY] == roomTileState.Interactive)
                    {
                        if (!this.gridUnit[pUnit.goalX, pUnit.goalY])
                        {
                            this.gridState[pUnit.goalX, pUnit.goalY] = roomTileState.Free;
                            workInteractiveTile = true;
                        }
                    }

                    List<blisterMoleNode> Path = this.findShortestPath(pUnit.X, pUnit.Y, pUnit.goalX, pUnit.goalY);
                    if (workInteractiveTile)
                        this.gridState[pUnit.goalX, pUnit.goalY] = roomTileState.Interactive;

                    if (Path.Count > 0)
                    {
                        nextTile = Path[0];
                        if (!this.gridUnit[nextTile.X, nextTile.Y] && tileExists(nextTile.X, nextTile.Y))
                        {
                            pUnit.Path = Path;
                            moveRoomUnitToTile(pUnit, ref nextTile);
                            return;
                        }
                    }
                }
            }
            else
            {
                bool workInteractiveTile = false;

                if (this.gridState[pUnit.goalX, pUnit.goalY] == roomTileState.Interactive)
                {
                    if (!this.gridUnit[pUnit.goalX, pUnit.goalY])
                    {
                        this.gridState[pUnit.goalX, pUnit.goalY] = roomTileState.Free;
                        workInteractiveTile = true;
                    }
                }

                List<blisterMoleNode> Path = this.findShortestPath(pUnit.X, pUnit.Y, pUnit.goalX, pUnit.goalY);
                if (workInteractiveTile)
                    this.gridState[pUnit.goalX, pUnit.goalY] = roomTileState.Interactive;

                if (Path.Count > 0)
                {
                    pUnit.Path = Path;
                    unsetRoomUnitTileState(pUnit);
                    moveRoomUnit(pUnit);
                    return;
                }
                setRoomUnitTileState(pUnit);
            }

            // Stop moving
            pUnit.Moves = false;
            pUnit.nextX = 0;
            pUnit.nextY = 0;
            pUnit.nextZ = 0;
            pUnit.requiresUpdate = false;
        }
        private void moveRoomUnitToTile(roomUnit pUnit, ref blisterMoleNode nextTile)
        {
            // Set next step
            pUnit.nextX = nextTile.X;
            pUnit.nextY = nextTile.Y;
            pUnit.nextZ = this.gridHeight[nextTile.X, nextTile.Y];
            pUnit.Path.Remove(nextTile);
            
            // Set unit map
            this.gridUnit[pUnit.X, pUnit.Y] = false;
            this.gridUnit[pUnit.nextX, pUnit.nextY] = true;

            // Calculate new rotation
            
            pUnit.rotationHead = rotationCalculator.calculateHumanMoveDirection(pUnit.X, pUnit.Y, pUnit.nextX, pUnit.nextY);
            pUnit.rotationBody = pUnit.rotationHead;

            pUnit.requiresUpdate = true;
        }
        private List<blisterMoleNode> findShortestPath(byte fromX, byte fromY, byte goalX, byte goalY)
        {
            List<blisterMoleNode> ret = null;
            if (fromX != goalX || fromY != goalY)
            {
                blisterMolePathfinder Pathfinder = new blisterMolePathfinder(this.gridState, this.gridUnit, this.gridHeight);
                ret = Pathfinder.findShortestPath(fromX, fromY, goalX, goalY);
            }
            if (ret == null)
                ret = new List<blisterMoleNode>();
            else
                ret.RemoveAt(0);

            return ret;
        }
        private void shortenRoomUnitPath(roomUnit pUnit)
        {
            if (pUnit.Path.Count > 1)
            {
                blisterMoleNode pNode = pUnit.Path[1];

                if (!(Math.Abs(pUnit.X - pNode.X) > 1 || Math.Abs(pUnit.Y - pNode.Y) > 1))
                {
                    if (tileFree(pNode.X, pNode.Y))
                    {
                        int X1 = 0;
                        int X2 = 0;
                        if (pNode.X > pUnit.X)
                        {
                            X1 = -1;
                            X2 = 1;
                        }
                        else
                        {
                            X1 = 1;
                            X2 = -1;
                        }

                        if (tileFree((byte)(pNode.X + X1), pNode.Y) && tileFree((byte)(pUnit.X + X2), pUnit.Y)) // Valid shortcut
                        {
                            pUnit.Path.RemoveAt(0);
                        }
                    }
                }
            }
        }
        private void setRoomUnitTileState(roomUnit pUnit)
        {
            pUnit.removeStatus("sit");
            pUnit.removeStatus("lay");

            List<floorItem> tileItems = this.getFloorItems(pUnit.X, pUnit.Y);
            foreach (Items.floorItem lItem in tileItems)
            {
                if (lItem.Definition.Behaviour.canSitOnTop)
                {
                    pUnit.Z = lItem.Z;
                    pUnit.rotationHead = lItem.Rotation;
                    pUnit.rotationBody = lItem.Rotation;

                    pUnit.removeStatus("dance");
                    pUnit.addStatus("sit", "sit", stringFunctions.formatFloatForClient(lItem.Definition.topHeight), 0, null, 0, 0);
                }
                else if (lItem.Definition.Behaviour.canLayOnTop)
                {
                    //pUnit.X = lItem.X;
                    //pUnit.Y = lItem.Y;
                    pUnit.Z = lItem.Z;
                    pUnit.rotationBody = lItem.Rotation;
                    pUnit.rotationHead = lItem.Rotation;

                    pUnit.removeStatus("dance");
                    pUnit.removeStatus("handitem");
                    pUnit.addStatus("lay", "lay", stringFunctions.formatFloatForClient(lItem.Definition.topHeight) + " null", 0, null, 0, 0);
                }
            }
        }

        private void unsetRoomUnitTileState(roomUnit pUnit)
        {
            if (this.gridState[pUnit.X, pUnit.Y] == roomTileState.Interactive)
            {
                List<floorItem> Items = this.getFloorItems(pUnit.X, pUnit.Y);
                foreach (floorItem lItem in Items)
                {
                    if (lItem.Definition.Behaviour.canSitOnTop) // User was seated, remove sit animation and reset height
                    {
                        pUnit.removeStatus("sit");
                        pUnit.Z = this.gridHeight[pUnit.X, pUnit.Y];
                    }
                }
            }
        }
        public void refreshRoomUnitOnTile(byte X, byte Y)
        {
            roomUnit pUnit = this.getRoomUnitOnTile(X, Y);
            if (pUnit != null)
            {
                setRoomUnitTileState(pUnit);
                this.sendMessage("@b" + pUnit.ToStatusString() + (char)1);
            }
        }
        #endregion
    }
}
