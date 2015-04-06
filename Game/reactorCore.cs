using System;

using Woodpecker.Sessions;
using Woodpecker.Net.Game;
using Woodpecker.Net.Game.Messages;

namespace Woodpecker.Game
{
    /// <summary>
    /// The root class of all *Reactor objects, containing the element parts of a reactor who are inherited in every reactor. This class is partial, the other part is inside reactorGenericMethods.cs
    /// </summary>
    public partial class Reactor
    {
        #region Properties
        /// <summary>
        /// The Session object to use by listeners in this reactor.
        /// </summary>
        protected Session Session;
        /// <summary>
        /// The clientMessage object, which contains the last sent message by the client.
        /// </summary>
        protected clientMessage Request;
        /// <summary>
        /// The serverMessage object, which will contain response messages before being sent to the client.
        /// </summary>
        protected serverMessage Response = new serverMessage();
        #endregion

        #region Methods
        /// <summary>
        /// Sends the current serverMessage object as a response and resets the response object.
        /// </summary>
        public void sendResponse()
        {
            this.Session.gameConnection.sendMessage(this.Response);
            this.Response = new serverMessage();
        }
        /// <summary>
        /// Sets the Woodpecker.Sessions.Session object for this reactor.
        /// </summary>
        /// <param name="Session">The Woodpecker.Sessions.Session to set.</param>
        public void setSession(Session Session)
        {
            this.Session = Session;
        }
        /// <summary>
        /// Nulls the Woodpecker.Sessions.Session object for this reactor.
        /// </summary>
        public void unsetRequest()
        {
            this.Request = null;
        }
        /// <summary>
        /// Sets the current request (Woodpecker.Net.Game.Messages.clientMessage) to this reactor.
        /// </summary>
        /// <param name="Request">The last sent Woodpecker.Net.Game.Messages.clientMessage.</param>
        public void setRequest(clientMessage Request)
        {
            this.Request = Request;
        }
        #endregion
    }
}
