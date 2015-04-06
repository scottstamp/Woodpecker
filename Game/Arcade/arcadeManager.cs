using System;
using System.Collections.Generic;

using Woodpecker.Core;

namespace Woodpecker.Game.Arcade
{
    public class arcadeManager
    {
        #region Fields
        private List<arcadeGameLobby> mLobbies = new List<arcadeGameLobby>();
        #endregion

        #region Methods
        /// <summary>
        /// Tries to return arcadeGameLobby instance of a given room ID from the game lobby collection. If the game lobby is not found, null is returned.
        /// </summary>
        /// <param name="roomID">The database ID of the room to get the lobby of.</param>
        public arcadeGameLobby getLobby(int roomID)
        {
            foreach (arcadeGameLobby lLobby in mLobbies)
            {
                if (lLobby.roomID == roomID)
                    return lLobby;
            }

            return null;
        }
        /// <summary>
        /// Iterates through all hooked game lobbies to find a game with a given ID. If the game is not found, null is returned.
        /// </summary>
        /// <param name="ID">The unique ID of the game to retrieve.</param>
        public arcadeGame getGame(int ID)
        {
            foreach (arcadeGameLobby lLobby in mLobbies)
            {
                arcadeGame pGame = lLobby.getGame(ID);
                if (pGame != null)
                    return pGame;
            }

            return null;
        }
        #endregion
    }
}
