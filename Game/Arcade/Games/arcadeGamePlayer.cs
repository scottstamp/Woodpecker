using System;

using Woodpecker.Core;
using Woodpecker.Sessions;
using Woodpecker.Game.Users;
using Woodpecker.Game.Rooms.Instances;
using Woodpecker.Game.Rooms.Pathfinding;

namespace Woodpecker.Game.Arcade
{
    /// <summary>
    /// Represents a player in an arcade game.
    /// </summary>
    public class arcadeGamePlayer
    {
        #region Fields
        /// <summary>
        /// The database ID of the user this arcade game player represents.
        /// </summary>
        public int userID;
        /// <summary>
        /// True if this game player has created the game it belongs to.
        /// </summary>
        public bool isGameOwner;
        /// <summary>
        /// The ID of the team this game player is currently in. Zero-based.
        /// </summary>
        public int teamID;

        /// <summary>
        /// The ID that represents the game player in the lobby/arena.
        /// </summary>
        public int roomUnitID;
        /// <summary>
        /// The username of this 
        /// </summary>
        public string Username
        {
            get
            {
                Session pSession = Engine.Game.Users.getUserSession(this.userID);
                if (pSession != null) // Users session is still active
                    return pSession.User.Username;
                else
                    return Engine.Game.Users.getUsername(this.userID);
            }
        }
        #endregion
    }
}
