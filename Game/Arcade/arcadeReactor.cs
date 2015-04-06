using System;

using Woodpecker.Game;

namespace Woodpecker.Game.Arcade
{
    public class arcadeReactor : Reactor
    {
        private int mGameID = -1;
        /// <summary>
        /// Sends a given gamesys error message to the client. (mesasge 236)
        /// </summary>
        /// <param name="errorID">The ID of the error message.</param>
        private void sendGameError(int errorID)
        {
            Response.Initialize(236); // "Cl"
            Response.appendWired(errorID);

            sendResponse();
        }
        /// <summary>
        /// Performs a ticket check, to check if the user has enough game tickets to play a game, if not, it alerts the session user. The result is returned.
        /// </summary>
        private bool performTicketCheck()
        {
            if (Session.User.Tickets >= 2)
            {
                return true;
            }

            sendGameError(2);
            return false;
        }
        
        /// <summary>
        /// 159 - "B_"
        /// </summary>
        public void GET_GAME_LIST()
        {
            Response.Initialize(232); // "Ch"

            arcadeGameLobby pLobby = Engine.Game.Arcade.getLobby(Session.roomID);
            if (pLobby != null)
                Response.Append(pLobby.buildGameIndex());

            sendResponse();
        }
        /// <summary>
        /// 160 - "B`"
        /// </summary>
        public void START_OBSERVING_GAME()
        {
            int gameID = Request.getNextWiredParameter();
            arcadeGame pGame = Engine.Game.Arcade.getLobby(Session.roomID).getGame(gameID);

            Response.Initialize(233); // "Ci"
            if(pGame != null)
            {
                mGameID = gameID;
                // TODO: ADD

                Response.Append(pGame.ToString());
            }
            sendResponse();
        }
        /// <summary>
        /// 162 - "Bb"
        /// </summary>
        public void GET_CREATE_GAME_INFO()
        {
            Response.Initialize(171); // "Bk"

            arcadeGameLobby pLobby = Engine.Game.Arcade.getLobby(Session.roomID);
            if (pLobby != null)
            {
                // TODO: append info keys
            }

            sendResponse();
        }
        /// <summary>
        /// 163 - "Bc"
        /// </summary>
        public void CREATE_GAME()
        {
            arcadeGameLobby pLobby = Engine.Game.Arcade.getLobby(Session.roomID);
            if (pLobby != null)
            {
                if (performTicketCheck() == true)
                {

                }
            }
        }
        /// <summary>
        /// 165 - "Be"
        /// </summary>
        public void JOIN_GAME()
        {

        }
        /// <summary>
        /// 166 - "Bf"
        /// </summary>
        public void SPECTATE_GAME()
        {

        }
        /// <summary>
        /// 167 - "Bg"
        /// </summary>
        public void LEAVE_GAME()
        {

        }
        /// <summary>
        /// 168 - "Bh"
        /// </summary>
        public void KICK_USER()
        {

        }
        /// <summary>
        /// 170 - "Bj"
        /// </summary>
        public void START_GAME()
        {

        }
        /// <summary>
        /// 171 - "Bk"
        /// </summary>
        public void GAME_INTERACT()
        {

        }
        /// <summary>
        /// 172 - "Bl"
        /// </summary>
        public void PLAY_AGAIN()
        {
            arcadeGame pGame = Engine.Game.Arcade.getLobby(Session.roomID).getGame(mGameID);
            if (pGame != null && pGame.State == arcadeGame.arcadeGameState.Ended)
            {
                if (performTicketCheck() == true)
                {

                }
            }
        }
    }
}
