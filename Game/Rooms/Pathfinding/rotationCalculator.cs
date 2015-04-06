using System;

namespace Woodpecker.Game.Rooms.Pathfinding
{
    public static class rotationCalculator
    {
        #region Methods
        /// <summary>
        /// Calculates the next body + head direction of a user/bot/pet. The rotation is calculated by comparing the current position to the tile to look to.
        /// </summary>
        /// <param name="X">The X position of the current tile.</param>
        /// <param name="Y">The Y position of the current tile.</param>
        /// <param name="toX">The X position of the tile to look to.</param>
        /// <param name="toY">The Y position of the tile to look to.</param>
        public static byte calculateHumanDirection(int X, int Y, int toX, int toY)
        {
            byte ret = 0;
            if (X > toX && Y > toY)
                ret = 7;
            else if (X < toX && Y < toY)
                ret = 3;
            else if (X > toX && Y < toY)
                ret = 5;
            else if (X < toX && Y > toY)
                ret = 1;
            else if (X > toX)
                ret = 6;
            else if (X < toX)
                ret = 2;
            else if (Y < toY)
                ret = 4;
            else if (Y > toY)
                ret = 0;

            return ret;
        }
        public static byte calculateHumanMoveDirection(int X, int Y, int toX, int toY)
        {
            if (X == toX)
            {
                if (Y < toY)
                    return 4;
                else
                    return 0;
            }
            else if (X > toX)
            {
                if (Y == toY)
                    return 6;
                else if (Y < toY)
                    return 5;
                else
                    return 7;
            }
            else if (X < toX)
            {
                if (Y == toY)
                    return 2;
                else if (Y < toY)
                    return 3;
                else
                    return 1;
            }

            return 0;
        } 
        #endregion
    }
}
