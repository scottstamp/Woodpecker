using System;
using System.Collections.Generic;

using Woodpecker.Sessions;
using Woodpecker.Game.Users;
using Woodpecker.Game.Rooms.Units;
using Woodpecker.Game.Items.Bots;
using Woodpecker.Specialized.Fun;

namespace Woodpecker.Game.Rooms.Instances.Interaction
{
    public partial class roomReactor : Reactor
    {
        #region Methods
        /// <summary>
        /// Tries to process a given chat message and checks if the current room bot should respond.
        /// </summary>
        /// <param name="Text">The chat message to process.</param>
        private void handleBotResponse(string Text)
        {
            if (Session.roomInstance.getRoomBot(Session.roomID) != null)
            {
                roomUser Me = Session.roomInstance.getRoomUser(Session.ID);
                roomUser Bot = Session.roomInstance.getRoomBot(Session.roomID);
                if (Bot != null)
                {
                    BotResponse bResponse = Bot.bInfo.GetResponse(Text);
                    if (bResponse != null)
                    {
                        if (bResponse.ResponseText != null && bResponse.ResponseType != null)
                            switch (bResponse.ResponseType)
                            {
                                case "say":
                                    Session.roomInstance.sendTalk(Bot.ID, Bot.X, Bot.Y, bResponse.ResponseText);
                                    break;
                                case "shout":
                                    Session.roomInstance.sendShout(Bot.ID, Bot.X, Bot.Y, bResponse.ResponseText);
                                    break;
                            }
                        if (bResponse.ServeId != 0)
                        {
                            Items.carryItemHelper.setHandItem(ref Me, bResponse.ServeId.ToString());
                        }
                    }
                }
            }
                /*if (Bot != null)
                    if (Bot.bInfo.triggerResponse != null)
                        foreach (KeyValuePair<string, string> pair in Bot.bInfo.triggerResponse)
                            if (Text.ToLower().Contains(pair.Key))
                            {
                                if (pair.Value.Contains("}"))
                                {
                                    String[] pairArgs = pair.Value.Split('}');
                                    Items.carryItemHelper.setHandItem(ref Me, pairArgs[0]);
                                    Session.roomInstance.sendShout(Bot.ID, Bot.X, Bot.Y, pairArgs[1]);
                                }
                                else
                                {
                                    Session.roomInstance.sendShout(Bot.ID, Bot.X, Bot.Y, pair.Value);
                                }
                                break;
                            }
                 }*/
        }
        #endregion
    }
}
