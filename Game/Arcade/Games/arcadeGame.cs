using System;
using System.Collections.Generic;

using Woodpecker.Specialized.Text;

namespace Woodpecker.Game.Arcade
{
    /// <summary>
    /// Represents an arcade game.
    /// </summary>
    public class arcadeGame
    {
        #region Fields
        /// <summary>
        /// The unique database ID of this game, used for identification, arena allocation and score saving.
        /// </summary>
        public int ID;

        /// <summary>
        /// A value of the arcadeGameType enum representing the type of this game.
        /// </summary>
        private arcadeGameType mType;
        /// <summary>
        /// A value of the arcadeGameType enum representing the type of this game.
        /// </summary>
        public arcadeGameType Type
        {
            get { return mType; }
        }

        /// <summary>
        /// A value of the arcadeGameType enum representing the type of this game.
        /// </summary>
        private arcadeGameState mState;
        /// <summary>
        /// A value of the arcadeGameState enum representing the state of this game.
        /// </summary>
        public arcadeGameState State
        {
            get { return mState; }
        }

        /// <summary>
        /// The arcadeGameArena instance of this game. The arena is where all game action happens once the game has started. Null if the game is not started yet.
        /// </summary>
        private arcadeGameArena mArena;

        /// <summary>
        /// The database ID of the user that created this arcade game.
        /// </summary>
        private int creatorUserID;

        /// <summary>
        /// A List collection of the type arcadeGamePlayer, representing the players that have joined one of this game's teams.
        /// </summary>
        private List<arcadeGamePlayer> mPlayers = new List<arcadeGamePlayer>();
        /// <summary>
        /// A List collection of the type unsigned integer, holding the session IDs of the sessions that are currently spectating this game.
        /// </summary>
        private List<uint> mSpectatorSessionIDs = new List<uint>();

        #region Game details
        /// <summary>
        /// The title of this arcade game, this name is assigned by the game owner upon creating.
        /// </summary>
        public string Title;
        /// <summary>
        /// The ID of the map that is being played on in this game.
        /// </summary>
        public int mapID;
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Tries to return the gamePlayer object of this game's creator.
        /// </summary>
        /// <returns></returns>
        public arcadeGamePlayer getOwnerPlayer()
        {
            foreach (arcadeGamePlayer lPlayer in mPlayers)
            {
                if (lPlayer.isGameOwner)
                    return lPlayer;
            }

            return null;
        }

        /// <summary>
        /// Returns the string representation of this arcade game for display in the game list.
        /// </summary>
        public string ToSummaryString()
        {
            fuseStringBuilder FSB = new fuseStringBuilder();
            FSB.appendWired(this.ID);
            FSB.appendClosedValue(this.Title);

            arcadeGamePlayer pOwner = this.getOwnerPlayer();
            if (pOwner != null)
            {
                FSB.appendWired(pOwner.roomUnitID);
                FSB.appendClosedValue(pOwner.Username);
            }
            else
            {
                FSB.appendWired(-1);
                FSB.appendClosedValue("???");
            }

            FSB.appendWired(mapID);

            return FSB.ToString();
        }
        /// <summary>
        /// Returns the string representation of this arcade game.
        /// </summary>
        public override string ToString()
        {
            return "";
        }
        #endregion

        #region Nested
        /// <summary>
        /// A set of values for state of an arcade game.
        /// </summary>
        public enum arcadeGameState
        {
            Waiting = 0,
            Started = 1,
            Ended = 2
        }
        #endregion
    }
}
