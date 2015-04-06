using System;
using System.Collections.Generic;
using System.Text;

namespace Woodpecker.Game.Messenger
{
    public class messengerBuddyRequest
    {
        #region Fields
        /// <summary>
        /// The database ID of the user that sent this buddy request.
        /// </summary>
        public int userID;
        /// <summary>
        /// The username of the user that sent this buddy request.
        /// </summary>
        public string Username;
        #endregion
    }
}
