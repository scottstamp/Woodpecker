using System;

using Woodpecker.Specialized.Text;
using Woodpecker.Sessions;

using Woodpecker.Net.Game.Messages;

using Woodpecker.Game.Users;
using Woodpecker.Game.Rooms.Instances;
using Woodpecker.Specialized.Fun;

namespace Woodpecker.Game.Moderation
{
    /// <summary>
    /// Contains target methods for various functions regarding the 'Call for Help' feature and the 'moderation tool'. This reactor is hooked to moderator+ sessions only.
    /// </summary>
    public class moderationReactor : Reactor
    {
        #region Target methods
        /// <summary>
        /// 200 - "CH"
        /// </summary>
        public void MODERATORACTION()
        {
            string[] args = Request.getMixedParameters();
            int requestAction = int.Parse(args[1]);

            // 2 = message, 3 = extra info, 4 = target username, 5 = banhours, 6 = ban machine, 7 = ban IP
            stringFunctions.filterVulnerableStuff(ref args[2], false);
            stringFunctions.filterVulnerableStuff(ref args[3], false);
            if(args.Length >= 5) // Filter target username?
                stringFunctions.filterVulnerableStuff(ref args[4], true);

            string postBackMessage = null;
            if (args[0] == "0") // Single-user action
            {
                if (args[4].Length > 0) // Username given
                {
                    if (requestAction == 0 || requestAction == 1) // Alert or ban
                    {
                        if (requestAction == 0) // Alert
                        {
                            if (args[2].Length > 0) // Message given
                            {
                                if (Engine.Game.Moderation.requestAlert(Session.User.ID, args[4], args[2], args[3]))
                                    postBackMessage = "Alert sent to user.";
                            }
                        }
                        else if (requestAction == 1) // Kick
                        {
                            if (Engine.Game.Moderation.requestKickFromRoom(Session.User.ID, args[4], args[2], args[3]))
                                postBackMessage = "Kick sent to user.";
                        }
                    }
                    else if (requestAction == 2) // Ban
                    {
                        if (args[2].Length > 0) // Message given
                        {
                            int banHours = int.Parse(args[5]);
                            bool banMachine = (args[6] == "1");
                            bool banIP = (args[7] == "1");

                            if (Engine.Game.Moderation.requestBan(Session.User.ID, args[4], banHours, banIP, banMachine, args[2], args[3]))
                                postBackMessage = "User banned for " + banHours + " hours, ip banned: " + banIP.ToString().ToLower() + ", machine banned: " + banMachine.ToString().ToLower() + ".";
                        }
                    }
                }
            }
            else // Room action
            {
                if (Session.inRoom)
                {
                    if (requestAction == 0) // Alert
                    {
                        if (args[2].Length > 0) // Message given
                        {
                            if (args[2].IndexOf("/voice") == 0)
                            {
                                args[2] = args[2].Substring(6);
                                Session.roomInstance.sendMessage(FunUtils.CreateVoiceSpeakMessage(args[2]));

                                postBackMessage = "Voice sent.";
                            }
                            else
                            {
                                if (Engine.Game.Moderation.requestRoomAlert(Session.User.ID, Session.roomID, args[2], args[3]))
                                    postBackMessage = "Alert sent to room.";
                            }
                        }
                    }
                    else if (requestAction == 1) // Kick
                    {
                        if (Engine.Game.Moderation.requestRoomKick(Session.User.ID, Session.roomID, args[2], args[3]))
                            postBackMessage = "Kick sent to room.";
                    }
                }
            }

            if (postBackMessage == null)
                postBackMessage = "Make sure that the target user is online (if any) and you have the correct permissions.";

            Session.castWhisper(postBackMessage);
        }
        #endregion
    }
}
