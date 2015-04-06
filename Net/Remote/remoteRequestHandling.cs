using System;

using Woodpecker.Specialized.Text;
using Woodpecker.Net.Game.Messages;

namespace Woodpecker.Net.Remote
{
    public partial class remoteConnectionManager
    {
        #region Methods
        /// <summary>
        /// Handles a given remote request and returns a boolean that indicates if the request has been successfully handled.
        /// </summary>
        /// <param name="messageID">The ID of the request to process.</param>
        /// <param name="args">The string array with the request content.</param>
        public bool handleRequest(int messageID, string[] args)
        {
            try
            {
                switch (messageID)
                {
                    #region System
                    case 0: // Stop Woodpecker
                        {
                            Engine.Program.Stop(args[1]);

                            return true;
                        }

                    case 1: // Hotel alert
                        {
                            stringFunctions.filterVulnerableStuff(ref args[1], false);
                            Engine.Game.Users.broadcastHotelAlert(args[1]);

                            return true;
                        }

                    case 2: // Offline in %x% minutes alert
                        {
                            int leftMinutes = 0;
                            if (int.TryParse(args[1], out leftMinutes))
                            {
                                serverMessage Message = new serverMessage(291); // "Dc"
                                Message.appendWired(leftMinutes);

                                Engine.Game.Users.broadcastMessage(Message);

                                return true;
                            }
                        }
                        break;

                    #endregion

                    #region Moderation
                    #region Single user events
                    case 31: // Remote user alert
                        {
                            int userID = 0;
                            if (int.TryParse(args[1], out userID))
                            {
                                int targetUserID = 0;
                                if (int.TryParse(args[2], out targetUserID))
                                {
                                    stringFunctions.filterVulnerableStuff(ref args[3], true); // Message
                                    stringFunctions.filterVulnerableStuff(ref args[4], true); // Extra info

                                    return Engine.Game.Moderation.requestAlert(userID, targetUserID, args[3], args[4]);
                                }
                            }
                        }
                        break;

                    case 32: // Remote user kick
                        {
                            int userID = 0;
                            if (int.TryParse(args[1], out userID))
                            {
                                int targetUserID = 0;
                                if (int.TryParse(args[2], out targetUserID))
                                {
                                    stringFunctions.filterVulnerableStuff(ref args[3], true); // Message
                                    stringFunctions.filterVulnerableStuff(ref args[4], true); // Extra info

                                    return Engine.Game.Moderation.requestKickFromRoom(userID, targetUserID, args[3], args[4]);
                                }
                            }
                        }
                        break;

                    case 33: // Remote user ban
                        {
                            int userID = 0;
                            if (int.TryParse(args[1], out userID))
                            {
                                int targetUserID = 0;
                                if (int.TryParse(args[2], out targetUserID))
                                {
                                    int Hours = 0;
                                    if (int.TryParse(args[3], out Hours))
                                    {
                                        bool banIP = (args[4] == "1");
                                        bool banMachine = (args[5] == "1");
                                        stringFunctions.filterVulnerableStuff(ref args[6], true);
                                        stringFunctions.filterVulnerableStuff(ref args[7], true);

                                        return Engine.Game.Moderation.requestBan(userID, targetUserID, Hours, banIP, banMachine, args[6], args[7]);
                                    }
                                }
                            }
                        }
                        break;
                    #endregion

                    #region Room instance events
                    case 34: // Remote room alert
                        {
                            int userID = 0;
                            if (int.TryParse(args[1], out userID))
                            {
                                int roomID = 0;
                                if (int.TryParse(args[2], out roomID))
                                {
                                    stringFunctions.filterVulnerableStuff(ref args[3], true); // Message
                                    stringFunctions.filterVulnerableStuff(ref args[4], true); // Extra info

                                    return Engine.Game.Moderation.requestRoomAlert(userID, roomID, args[3], args[4]);
                                }
                            }
                        }
                        break;

                    case 35: // Remote room kick
                        {
                            int userID = 0;
                            if (int.TryParse(args[1], out userID))
                            {
                                int roomID = 0;
                                if (int.TryParse(args[2], out roomID))
                                {
                                    stringFunctions.filterVulnerableStuff(ref args[3], true); // Message
                                    stringFunctions.filterVulnerableStuff(ref args[4], true); // Extra info

                                    return Engine.Game.Moderation.requestRoomKick(userID, roomID, args[3], args[4]);
                                }
                            }
                        }
                        break;
                    #endregion
                    #endregion
                }
            }
            catch { }

            Core.Logging.Log("Remote request handler: error ocurred, OR no remote request handler for " + messageID);
            return false; // Failed!
        }
        #endregion
    }
}
