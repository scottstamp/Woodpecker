using System;

using Woodpecker.Game;
using Woodpecker.Net.Game.Messages;

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
            if (Session.User.Tickets >= 1)
            {
                return true;
            }

            sendGameError(2);
            return false;
        }

        /// <summary>
        /// 105 - "Ai"
        /// </summary>
        public void BUY_TICKETS()
        {
            int bundleID = Request.getNextWiredParameter();
            int ticketsPurchased = (bundleID == 1) ? 2 : 20;
            Users.userInformation recipient =
                Engine.Game.Users.getUserInfo(Request.Content.Substring(3), true);

            if (recipient != null)
            {
                Sessions.Session session = Engine.Game.Users.getUserSession(recipient.ID);
                if (session != null)
                {
                    session.User.Tickets += ticketsPurchased;
                    session.refreshTickets();
                    if (session != this.Session)
                    {
                        serverMessage alert = new serverMessage(139); // "BK"
                        alert.Append($"{this.Session.User.Username} has sent you {ticketsPurchased} tickets.");
                        session.gameConnection.sendMessage(alert);
                    }
                    else
                    {
                        Response.Initialize(139); // "BK"
                        Response.Append("Tickets purchased.");
                        sendResponse();
                    }
                }
                else
                {
                    recipient.Tickets += ticketsPurchased;
                    recipient.updateValueables();
                }
            }
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
            arcadeGameLobby lobby = Engine.Game.Arcade.getLobby(Session.roomID);

            if (lobby == null) return;

            arcadeGame pGame = lobby.getGame(gameID);

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
            arcadeGameLobby lobby = Engine.Game.Arcade.getLobby(Session.roomID);

            if (lobby == null) return;

            arcadeGame pGame = lobby.getGame(mGameID);
            if (pGame != null && pGame.State == arcadeGame.arcadeGameState.Ended)
            {
                if (performTicketCheck() == true)
                {

                }
            }
        }
    }
}
