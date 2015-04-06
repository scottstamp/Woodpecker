using System;

namespace Woodpecker.Game.Rooms.Pathfinding
{
    /// <summary>
    /// Represents a single tile on the map of a virtual room.
    /// </summary>
    public class roomTile
    {
        #region Fields
        /// <summary>
        /// The X position of this tile.
        /// </summary>
        public byte X;
        /// <summary>
        /// The Y position of this tile.
        /// </summary>
        public byte Y;
        public int I;
        #endregion

        #region Constructors
        public roomTile(byte X, byte Y, int I)
        {
            this.X = X;
            this.Y = Y;
            this.I = I;
        }
        #endregion
    }
}
