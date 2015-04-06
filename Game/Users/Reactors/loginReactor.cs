using System;
using System.Data;

using Woodpecker.Core;
using Woodpecker.Storage;
using Woodpecker.Specialized.Text;
using Woodpecker.Net.Game.Messages;

using Woodpecker.Game;
using Woodpecker.Game.Store;
using Woodpecker.Game.Rooms;
using Woodpecker.Game.Moderation;
using Woodpecker.Game.Items;

namespace Woodpecker.Game.Users
{
    /// <summary>
    /// Contains target methods for actions that happen before logging in, such as setting up encryption, logging in and registering.
    /// </summary>
    public class loginReactor : Reactor
    {
        #region Target methods
        /// <summary>
        /// 4 - "@D"
        /// </summary>
        public void TRY_LOGIN()
        {
            string Username = Request.getParameter(0);
            userInformation userDetails = Engine.Game.Users.getUserInfo(Username, true);
            if (userDetails == null) // User not found
                Session.gameConnection.sendLocalizedError("login incorrect: Wrong username");
            else
            {
                string Password = Request.getParameter(1);
                Password = Engine.Security.Cryptography.MD5.Hash(Password, userDetails.Username.ToLower()); // Hash the password

                if (userDetails.Password == Password) // All details match!
                {
                    Session.User = userDetails;
                    userDetails = null;

                    string banReason = "";
                    if (Engine.Game.Moderation.isBanned(Session.User.ID, out banReason))
                    {
                        Session.isValid = false;
                        Session.gameConnection.sendMessage(genericMessageFactory.createBanCast(banReason));
                        return;
                    }

                    Engine.Sessions.destroySessions(Session.User.ID); // Destroy previous sessions

                    Session.User.sessionID = Session.ID;
                    Session.Access.userID = Session.User.ID;
                    Session.Access.Update();
                    Session.User.updateLastActivity();
                    Session.User.updateClub(false);

                    Session.gameConnection.reactorHandler.unRegister(new loginReactor().GetType()); // Unregister the login reactor
                    Engine.Game.Users.addUserSession(this.Session);
                    Session.gameConnection.reactorHandler.Register(new userReactor()); // Register a userReactor
                    Session.gameConnection.reactorHandler.Register(new storeReactor()); // Register a storeReactor
                    Session.gameConnection.reactorHandler.Register(new navigatorReactor());
                    if (Session.User.hasFuseRight("fuse_moderator_access"))
                        Session.gameConnection.reactorHandler.Register(new moderationReactor());

                    Session.refreshFuseRights();

                    Response.Initialize(3); // "@C" (login OK)
                    sendResponse();

                    if (Session.User.hasClub)
                        Session.refreshFigureParts();

                    Session.itemStripHandler = new itemStripHandler(Session.User.ID); // Load hand items etc

                    Logging.Log("User '" + Session.User.Username + "' [id: " + Session.User.ID + "] with role '" + Session.User.Role.ToString() + "' logged in.", Logging.logType.userVisitEvent);
                    return;
                }
                else
                    Session.gameConnection.sendLocalizedError("login incorrect: Wrong password");
            }
            //Session.gameConnection.sendLocalizedError("Login through the client is not currently supported.");
            //Engine.Sessions.destroySession(Session.ID);
        }
        /// <summary>
        /// 5 - "@E"
        /// </summary>
        public void VERSIONCHECK()
        {

        }
        /// <summary>
        /// 43 - "@k"
        /// </summary>
        public void REGISTER()
        {
            // The 'receive campaign mail etc' uses a Base64 boolean which is not supported by Woodpecker in this context, fix the request content by raw string replace
            Request.Content = Request.Content.Replace("@JA@A@@IA", ""); // Receive mails ('A' = true)
            Request.Content = Request.Content.Replace("@JA@A@@I@", ""); // Do not receive mails ('@' = false)

            userInformation newUser = new userInformation();

            newUser.Username = Request.getStructuredParameter(2);
            if (Engine.Game.Users.getNameCheckError(false, newUser.Username) > 0)
                return;

            newUser.Password = Request.getStructuredParameter(3);
            if (!stringFunctions.passwordIsValid(newUser.Username, newUser.Password))
                return;
            newUser.Password = Engine.Security.Cryptography.MD5.Hash(newUser.Password, newUser.Username.ToLower()); // Byebye password

            newUser.Figure = Request.getStructuredParameter(4);
            if (newUser.Figure.Length != 25 || !stringFunctions.isNumeric(newUser.Figure))
                return;

            newUser.Sex = 'M';
            if (Request.getStructuredParameter(5) == "F")
                newUser.Sex = 'F';

            newUser.Email = Request.getStructuredParameter(7);
            if (!stringFunctions.emailIsValid(newUser.Email))
                return;

            newUser.DateOfBirth = Request.getStructuredParameter(8);
            if (newUser.DateOfBirth.Split('.').Length != 3)
                return;

            Engine.Game.Users.registerUser(this.Session, newUser);
            //Session.gameConnection.sendLocalizedError("Registration through the client is not currently supported.");
            //Engine.Sessions.destroySession(Session.ID);
        }
        /// <summary>
        /// 46 - "@n"
        /// </summary>
        public void AC()
        {
            Response.Initialize(271); // "DO"
            Response.Append(1); // It's fine
            sendResponse();
        }
        /// <summary>
        /// 146 - "BR"
        /// </summary>
        public void PARENT_EMAIL_REQUIRED()
        {
            Response.Initialize(217); // "CY"
            Response.Append(1); // You don't need to send your parents a mail...
            sendResponse();
        }
        /// <summary>
        /// 147 - "BS"
        /// </summary>
        public void VALIDATE_PARENT_EMAIL()
        {
            Response.Initialize(139); // "BK"
            Response.Append("Oops, something went wrong!\rWoodpecker doesn't support mom 'n dad stuff, please re-start the registration!");
            sendResponse();
        }
        /// <summary>
        /// 204 - "CL" - [b64 gibberish][sso]
        /// </summary>
    }
        #endregion
}