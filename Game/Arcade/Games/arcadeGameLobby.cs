using System;
using System.Collections.Generic;

using Woodpecker.Core;
using Woodpecker.Specialized.Text;
using Woodpecker.Game.Rooms.Instances;

namespace Woodpecker.Game.Arcade
{
    public class arcadeGameLobby
    {
        #region Fields
        /// <summary>
        /// The database ID of the room this game lobby belongs to.
        /// </summary>
        public int roomID;
        /// <summary>
        /// A List of the type arcadeGames with all the games that are hosted in this lobby.
        /// </summary>
        private List<arcadeGame> mGames = new List<arcadeGame>();
        #endregion

        #region Methods
        /// <summary>
        /// Tries to return arcadeGame instance of a given game ID from the game collection in this lobby. If the game is not found, null is returned.
        /// </summary>
        /// <param name="ID">The unique ID of the game to retrieve.</param>
        /// <returns></returns>
        public arcadeGame getGame(int ID)
        {
            foreach(arcadeGame lGame in mGames)
            {
                if (lGame.ID == ID)
                    return lGame;
            }

            return null;
        }

        /// <summary>
        /// Builds the string for the client that displays the index of games and their details.
        /// </summary>
        public string buildGameIndex()
        {
            int[] gameAmounts = new int[3];
            fuseStringBuilder ptxtGames = new fuseStringBuilder();
            
            foreach(arcadeGame lGame in mGames)
            {
                ptxtGames.Append(lGame.ToSummaryString());
                gameAmounts[(int)lGame.State]++;
            }

            fuseStringBuilder FSB = new fuseStringBuilder();
            FSB.appendWired(gameAmounts[0]); // Amount of 'waiting' games
            if(gameAmounts[1] > 0 || gameAmounts[2] > 0)
            {
                FSB.appendWired(gameAmounts[1]); // Amount of started games
                FSB.appendWired(gameAmounts[2]); // Amount of ended games
            }
            FSB.Append(ptxtGames.ToString());

            return FSB.ToString();
        }
        #endregion
    }
}
