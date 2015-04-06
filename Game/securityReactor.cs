using System;

using Woodpecker.Net.Game.Messages;
using Woodpecker.Game.Users;

namespace Woodpecker.Game
{
    /// <summary>
    /// Contains target methods for initializing encryption and various other security tasks.
    /// </summary>
    public class securityReactor : Reactor
    {
        /// <summary>
        /// 6 - "@E" 
        /// </summary>
        public void UNIQUEID()
        {
            string machineID = Request.getParameter(0);
			
            // TODO: verify
            string banReason = "";
            if(Engine.Game.Moderation.isBanned(Session.ipAddress, machineID, out banReason))
            {
                Session.isValid = false;
                Session.gameConnection.sendMessage(genericMessageFactory.createBanCast(banReason));
            }
            else
            {
                Session.Access = new userAccessInformation();
                Session.Access.sessionID = Session.ID;
                Session.Access.IP = Session.ipAddress;
                Session.Access.machineID = machineID;
            }
        }
        /// <summary>
        /// 181 - "Bu"
        /// </summary>
        public void GET_SESSION_PARAMETERS()
        {
            Session.gameConnection.reactorHandler.unRegister(new securityReactor().GetType()); // Kill this reactor
            Session.gameConnection.reactorHandler.Register(new loginReactor()); // Register login reactor

            Response.Initialize(257); // "DA"
            Response.appendClosedValue("RAHIIIKHJIPAIQAdd-MM-yyyy");
            sendResponse();

            Session.refreshFigureParts();
        }
        /// <summary>
        /// 202 - "CJ"
        /// </summary>
        public void GENERATEKEY()
        {
            string Key = Engine.Security.Ciphering.generateKey();
            Response.Initialize(1); // "@A"
            Response.Append(Key);
            sendResponse();

            Session.gameConnection.initializeEncryption(Key);
            Core.Logging.Log("Initialized RC4 instance for session " + Session.ID + ", public key: " + Key, Core.Logging.logType.debugEvent);
        }
        /// <summary>
        /// 206 - "CN"
        /// </summary>
        public void INIT_CRYPTO()
        {
            Response.Initialize(277); // "DU"
            Response.appendWired(false); // Force encryption
            sendResponse();
        }
    }
}
