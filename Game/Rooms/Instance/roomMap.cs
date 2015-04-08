using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Woodpecker.Game.Items;
using Woodpecker.Game.Rooms.Pathfinding;

namespace Woodpecker.Game.Rooms.Instances
{
    public partial class roomInstance
    {
        /* Mapping in Woodpecker has been based on the mapping used in Joe 'Joeh' Hegarty's V9 emulator 'Thor' */
        #region Fields
        private roomTileState[,] gridState;
        private float[,] gridHeight;
        private bool[,] gridUnit;
        private char[,] gridClientMap;
        public roomModel getModel()
        {
            if (this.Information != null)
                return Engine.Game.Rooms.getModel(this.Information.modelType);
            else
                return null;
        }
        #endregion

        #region Methods
        public void generateFloorMap()
        {
            roomModel Model = this.getModel();
            if (Model == null) // Invalid model
            {
                Engine.Game.Rooms.destroyRoomInstance(this.roomID);
                return;
            }

            #region Generate default map
            string[] Axes = Model.heightMapAxes;
            int maxX = Axes[0].Length;
            int maxY = Axes.Length;

            roomTileState[,] stateMap = new roomTileState[maxX, maxY];
            float[,] heightMap = new float[maxX, maxY];
            char[,] defaultMap = new char[maxX, maxY];
            char[,] clientMap = new char[maxX, maxY];

            // Create default floor map
            for (int Y = 0; Y < maxY; Y++)
            {
                for (int X = 0; X < maxX; X++)
                {
                    float H = 0;
                    char Tile = Axes[Y][X];

                    defaultMap[X, Y] = Tile;
                    if (Tile != 'x' && float.TryParse(Tile.ToString(), out H))
                    {
                        stateMap[X, Y] = roomTileState.Free;
                        heightMap[X, Y] = H;
                    }
                    else
                        stateMap[X, Y] = roomTileState.Blocked;
                }
            }

            clientMap = defaultMap; // Set client map to default map for now
            #endregion

            #region Main loop for tile states
            foreach (floorItem lItem in this.floorItems)
            {
                foreach (roomTile lTile in getAffectedTiles(lItem, true).Values)
                {
                    if(lItem.Z >= heightMap[lTile.X, lTile.Y])
                        heightMap[lTile.X, lTile.Y] = lItem.Z;

                    // If this item is the lowest tile on the tile, and if no items can be placed on top of this item and this item is not interactable, then set proper value for client map
                    /*if (false && heightMap[lTile.X, lTile.Y] == 0
                        && !lItem.Definition.Behaviour.canStackOnTop
                        && !lItem.Definition.Behaviour.isPublicSpaceObject
                        && !lItem.Definition.Behaviour.canSitOnTop
                        && !lItem.Definition.Behaviour.canLayOnTop)
                        clientMap[lTile.X, lTile.Y] = 'A'; // Tileselector doesn't hover over this tile in client
                    */

                    if (lItem.Z >= heightMap[lTile.X, lTile.Y])
                    {
                        if (lItem.Definition.Behaviour.canStandOnTop)
                        {
                            if (lItem.Z > heightMap[lTile.X, lTile.Y] || lItem.Definition.Behaviour.isRoller)
                            {
                                stateMap[lTile.X, lTile.Y] = roomTileState.Free;
                                heightMap[lTile.X, lTile.Y] = lItem.totalHeight;
                            }
                        }
                        else if (lItem.Definition.isInteractiveStance)
                        {
                            stateMap[lTile.X, lTile.Y] = roomTileState.Interactive;

                            if (lItem.Definition.Behaviour.canLayOnTop)
                            {
                                // REDIRECT
                            }
                        }
                        else
                            stateMap[lTile.X, lTile.Y] = roomTileState.Blocked;
                    }
                }
            }
            #endregion

            bool containsRollers = false;
            bool containsPets = false;
            bool containsBots = (Engine.Game.Items.getBotInformation(roomID) != null);
            #region Secondary loop for exceptions and special map additions
            foreach (floorItem lItem in this.floorItems)
            {
                if (lItem.Definition.Behaviour.isRoller)
                {
                    containsRollers = true;
                    continue;
                }
                if (lItem.Definition.Sprite == "nest")
                {
                    containsPets = true;
                    continue;
                }


                // If items can be placed on top of this item and this item is at the top of the root tile, set the clientmap height to the appropriate value
                char tileTopValue = '\x000';

                //if(lItem.Definition.Behaviour.canStackOnTop && lItem.totalHeight == heightMap[lItem.X, lItem.Y])
                //    tileTopValue = Convert.ToChar(((int)Math.Round(lItem.totalHeight)).ToString());

                // If this item is a non-public space seat object, then get the original tile height of the default map
                
                //if (lItem.Z == lItem.Definition.Behaviour.canSitOnTop && !lItem.Definition.Behaviour.isPublicSpaceObject) // Non-public space object seat
                //    defaultHeight = float.Parse(defaultMap[lItem.X, lItem.Y].ToString());

                roomTileState doorState = roomTileState.Blocked;
                if (lItem.Definition.Behaviour.isDoor) // Item acts as a door, determine state
                {
                    if (lItem.customData == "O") // Door is open
                        doorState = roomTileState.Free;
                    else
                        doorState = roomTileState.Blocked;
                }

                foreach (roomTile lTile in getAffectedTiles(lItem, true).Values)
                {
                    if (tileTopValue != '\x000') // Can stack on tile
                        clientMap[lTile.X, lTile.Y] = tileTopValue;

                    //if (defaultHeight != -1)
                    //    heightMap[lTile.X, lTile.Y] = defaultHeight;

                    if (lItem.Definition.Behaviour.isDoor)
                        stateMap[lTile.X, lTile.Y] = doorState;
                }
            }
            #endregion

            #region Reset tilestatuses for client for rollers
            if (containsRollers)
            {
                foreach (floorItem lItem in this.floorItems)
                {
                    if (lItem.Definition.Behaviour.isRoller)
                        clientMap[lItem.X, lItem.Y] = defaultMap[lItem.X, lItem.Y];
                }
            }
            #endregion

            heightMap[Model.doorX, Model.doorY] = Model.doorZ;
            stateMap[Model.doorX, Model.doorY] = roomTileState.Blocked;

            this.gridState = stateMap;
            this.gridHeight = heightMap;
            this.gridClientMap = clientMap;
            this.hasRollers = containsRollers;
            this.hasPets = containsPets;
            this.hasBots = containsBots;
        }

        private Dictionary<int, roomTile> getAffectedTiles(floorItem pItem, bool includeRootTile)
        {
            return getAffectedTiles(pItem.Definition.Length, pItem.Definition.Width, pItem.X, pItem.Y, pItem.Rotation, includeRootTile);
        }
        private Dictionary<int, roomTile> getAffectedTiles(byte itemLength, byte itemWidth, byte X, byte Y, byte Rotation, bool includeRootTile)
        {
            // 0,2,4,6
            int x = 0;
            Dictionary<int, roomTile> pointList = new Dictionary<int, roomTile>();

            if (itemLength != itemWidth) // Non-square item, account rotation
            {
                if(Rotation == 0 ||Rotation == 4)
                {
                    byte l = itemLength;
                    byte w = itemWidth;
                    itemLength = w;
                    itemWidth = l;
                }
            }

            for(byte lX = X; lX < X + itemWidth; lX++)
            {
                for(byte lY = Y; lY < Y + itemLength; lY++)
                {
                    pointList.Add(x++, new roomTile(lX, lY,  (X < Y) ? Y : X));
                }
            }

            return pointList;
        }

        public string getClientFloorMap()
        {
            StringBuilder sb = new StringBuilder();
            /*
            for (int j = 0; j <= this.gridClientMap.GetUpperBound(1); j++)
            {
                for (int i = 0; i <= this.gridClientMap.GetUpperBound(0); i++)
                {
                    sb.Append(this.gridClientMap[i, j]);
                }
                sb.Append(Convert.ToChar(13));
            } */


            for (int Y = 0; Y <= this.gridUnit.GetUpperBound(1); Y++)
            {
                for (int X = 0; X <= this.gridUnit.GetUpperBound(0); X++)
                {
                    sb.Append(this.gridClientMap[X, Y]);
                }
                sb.Append(Convert.ToChar(13));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns true if two tiles (given by their coordinates on the map) are exactly one tile removed from each other.
        /// </summary>
        /// <param name="X">The X position of tile A.</param>
        /// <param name="Y">The Y position of tile A.</param>
        /// <param name="X2">The X position of tile B.</param>
        /// <param name="Y2">The Y position of tile B.</param>
        /// <returns></returns>
        public bool tilesTouch(byte X, byte Y, byte X2, byte Y2)
        {
            return (Math.Abs(X - X2) <= 1 && Math.Abs(Y - Y2) <= 1);
        }
        /// <summary>
        /// Returns true if room is public and has a camera view
        /// </summary>
        public bool hasCamera()
        {
            String[] rooms = new string[] { "pool_b", "md_a" };
            return rooms.Any(this.Information.modelType.Contains);
        }
        /// <summary>
        /// Returns true if room is public and has a changing room
        /// </summary>
        public bool hasChangeRoom()
        {
            String[] rooms = new string[] { "pool_a", "md_a" };
            return rooms.Any(this.Information.modelType.Contains);
        }
        private bool tileBlockedByRoomUnit(byte X, byte Y)
        {
            return this.gridUnit[X, Y];
        }
        private bool tileFree(byte X, byte Y)
        {
            return (gridState[X, Y] == roomTileState.Free && !gridUnit[X, Y]);
        }
        private bool tileExists(byte X, byte Y)
        {
            return (X >= 0 && X <= gridUnit.GetUpperBound(0) && Y >= 0 && Y <= gridUnit.GetUpperBound(1));
        }
        #endregion
    }
}
