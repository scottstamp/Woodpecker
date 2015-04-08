using System;
using System.Collections.Generic;

using Woodpecker.Sessions;
using Woodpecker.Game.Users;
using Woodpecker.Game.Rooms.Units;

using Woodpecker.Specialized.Fun;

namespace Woodpecker.Game.Rooms.Instances.Interaction
{
    public partial class roomReactor : Reactor
    {
        #region Methods
        /// <summary>
        /// Tries to process a given chat message as a chat command and returns true if the operation has succeeded.
        /// </summary>
        /// <param name="Text">The chat message to process.</param>
        private bool handleSpecialChatCommand(string Text)
        {
            roomUser Me = Session.roomInstance.getRoomUser(Session.ID);
            switch (Text)
            {
                case "o/":
                case "\\o":
                case "\\o/":
                    Me.addStatus("wave", "wave", null, 2, null, 0, 0);
                    break;
            }

            if (Text[0] == ':' && Text.Length > 3) // Could be a chat command (emotes too btw)
            {
                try
                {
                    string Command = Text.Substring(1).Split(' ')[0];
                    string Body = "";
                    if (Text.Contains(" "))
                        Body = Text.Substring(Command.Length + 2);

                    switch (Command)
                    {
                        #region Common commands for all users
                        case "about":
                            {
                                Response.Initialize(139); // "BK"
                                Response.Append("About Woodpecker");
                                Response.Append("<br>");
                                Response.Append("Woodpecker is a V13 Habbo Hotel emulator written in C# .NET by Nils (Nillus).");
                                Response.Append("<br><br>");
                                Response.Append("There is currently ");
                                Response.Append(Engine.Game.Users.userCount);
                                Response.Append(" user(s) online.");
                                sendResponse();
                            }
                            break;
                        case "glow":
                            {
                                Response.Initialize(Specialized.Encoding.base64Encoding.Decode("AG"));
                                Response.Append("lamp setlamp " + Body);
                                sendResponse();
                            }
                            break;
                        #endregion

                        #region Fancy commands
                        case "descenditem":
                            {
                                if (!Session.User.hasFuseRight("fuse_funchatcommands"))
                                    return false;

                                int itemID = int.Parse(Body);
                                if (Session.roomInstance.containsFloorItem(itemID))
                                {
                                    Woodpecker.Game.Items.floorItem pItem = Session.roomInstance.getFloorItem(itemID);
                                    Woodpecker.Net.Game.Messages.serverMessage Message = new Woodpecker.Net.Game.Messages.serverMessage(230); // "Cf"
                                    Message.appendWired(pItem.X - 1);
                                    Message.appendWired(pItem.Y + 1);
                                    Message.appendWired(pItem.X);
                                    Message.appendWired(pItem.Y);
                                    Message.appendWired(true);
                                    Message.appendWired(itemID);
                                    Message.appendClosedValue("12.0");
                                    Message.appendClosedValue(Woodpecker.Specialized.Text.stringFunctions.formatFloatForClient(pItem.Z));
                                    Message.appendWired(new Random(DateTime.Now.Millisecond).Next(0, 10000));
                                    Session.roomInstance.sendMessage(Message);
                                }
                            }
                            break;

                        case "lingo":
                            {
                                if (!Session.User.hasFuseRight("fuse_funchatcommands"))
                                    return false;

                                Session.roomInstance.getRoomUser(Session.ID).addStatus("lingo", "cri", Body, 10, null, 0, 0);
                            }
                            break;

                        case "voice":
                            {
                                if (!Session.User.hasFuseRight("fuse_funchatcommands"))
                                    return false;

                                Session.roomInstance.sendMessage(FunUtils.CreateVoiceSpeakMessage(Body));
                            }
                            break;

                        case "ufos":
                            {
                                if (!Session.User.hasFuseRight("fuse_funchatcommands"))
                                    return false;

                                // Hide windows?
                                bool hideWindows = (Body == "1");
                                if (hideWindows)
                                    Session.roomInstance.getRoomUser(Session.ID).addStatus("lingo", "cri", "hideWindows()", 1, null, 0, 0);

                                Random rnd = new Random(DateTime.Now.Millisecond * DateTime.Now.Second);
                                int ufoAmount = 50 + rnd.Next(0, 45);
                                Woodpecker.Net.Game.Messages.serverMessage Message = new Woodpecker.Net.Game.Messages.serverMessage();

                                // Send voice
                                Message = FunUtils.CreateVoiceSpeakMessage("Help, unknown flying objects! The aliens! There's a swarm of " + ufoAmount + " ufos coming this way! UFOS! Help! The hotel is attacked! Zap zap zap... Houston, we have a problem! Aliens! soi soi soi soi soi. The aliens are coming! We didn't listen! The end of the world! Aargh! Help, Aliens everywhere! I see ufos! I dream about cheese! I mean, beep beep beep! Meep meep meep! Code Red! Code Red! Area 51! Marihuana! Cape Canaveral! Aaron is a fag! Ufos! " + ufoAmount + " of them! I see them everywhere! Oh and I see dead people! UFOS! UFOs from Mars! Or from the Moon! Fuck knows! Whatever! Oh my god! They look like fucking weirdos! Space monsters! They look even worse than Rick Astley! UFOs! It's the end of the world! Ufos! Ufos! Ufos!");
                                Session.roomInstance.sendMessage(Message);

                                for (int ufoID = 0; ufoID < ufoAmount; ufoID++)
                                {
                                    rnd.Next(); rnd.Next();

                                    // Create ufo
                                    Woodpecker.Game.Items.floorItem pUfo = new Woodpecker.Game.Items.floorItem();
                                    pUfo.ID = Int32.MinValue - (ufoID + 1);
                                    pUfo.Definition = new Items.itemDefinition();
                                    pUfo.Definition.Sprite = "nest";
                                    pUfo.Definition.Length = 1;
                                    pUfo.Definition.Width = 1;
                                    pUfo.Definition.Color = "0,0,0";
                                    pUfo.X = (byte)rnd.Next(0, 45);
                                    pUfo.Y = (byte)rnd.Next(0, 45);
                                    pUfo.Z = rnd.Next(-3, 10);
                                    int destX = rnd.Next(-(20 + (pUfo.X * 2)), 20 + (pUfo.Y * 2));
                                    int destY = rnd.Next(-(20 + (pUfo.Y * 2)), 20 + (pUfo.X * 2)) + rnd.Next(-10, -20);
                                    float destZ = rnd.Next(-9, 10);

                                    // Send ufo
                                    Message.Initialize(93); // "A]"
                                    Message.Append(pUfo.ToString());
                                    Session.roomInstance.sendMessage(Message);

                                    // Move ufo
                                    Message.Initialize(230);
                                    Message.appendWired(destX);
                                    Message.appendWired(destY);
                                    Message.appendWired(pUfo.X);
                                    Message.appendWired(pUfo.Y);
                                    Message.appendWired(true);
                                    Message.appendWired(pUfo.ID);
                                    Message.appendClosedValue(Woodpecker.Specialized.Text.stringFunctions.formatFloatForClient(destZ));
                                    Message.appendClosedValue(Woodpecker.Specialized.Text.stringFunctions.formatFloatForClient(pUfo.Z));
                                    Message.appendWired(rnd.Next(0, 100000));
                                    Session.roomInstance.sendMessage(Message);

                                    System.Threading.Thread.Sleep(10);

                                    Core.Logging.Log("Ufo " + pUfo.ID + ": [" + pUfo.X + "," + pUfo.Y + "] >> [" + destX + "," + destY + "];", Woodpecker.Core.Logging.logType.debugEvent);
                                }
                            }
                            break;

                        case "handitem":
                            {
                                Items.carryItemHelper.setHandItem(ref Me, Body);
                            }
                            break;
                        #endregion

                        #region Moderation commands
                        case "alert":
                            {
                                if (!Session.User.hasFuseRight("fuse_alert"))
                                    return false;

                                string Username = Body.Substring(0, Body.IndexOf(' '));
                                string Message = Body.Substring(Body.IndexOf(' ') + 1);
                                if (Username.Length > 0 && Message.Length > 0)
                                {
                                    if (Engine.Game.Moderation.requestAlert(Session.User.ID, Username, Message, ""))
                                        Session.castWhisper("User has been alerted.");
                                }
                            }
                            break;

                        case "kick":
                            {
                                if (!Session.User.hasFuseRight("fuse_kick"))
                                    return false;

                                string Username = Body.Substring(0, Body.IndexOf(' '));
                                string Message = Body.Substring(Body.IndexOf(' ') + 1);
                                if (Username.Length > 0)
                                {
                                    if (Engine.Game.Moderation.requestKickFromRoom(Session.User.ID, Username, Message, ""))
                                        Session.castWhisper("User has been kicked.");
                                }
                            }
                            break;

                        case "ban":
                            {
                                if (!Session.User.hasFuseRight("fuse_ban"))
                                    return false;

                                string Username = Body.Substring(0, Body.IndexOf(' '));
                                Body = Body.Substring(Body.IndexOf(' ') + 1);
                                int Hours = int.Parse(Body.Substring(0, Body.IndexOf(' ')));
                                string Message = Body.Substring(Body.IndexOf(' ') + 1);

                                if (Message.Length > 0)
                                {
                                    if (Engine.Game.Moderation.requestBan(Session.User.ID, Username, Hours, false, false, Message, ""))
                                        Session.castWhisper("Leet verbannen voor " + Hours + " uur(en).");
                                }
                            }
                            break;

                        case "unban":
                            {
                                if (!Session.User.hasFuseRight("fuse_superban")) // Only mods can unban
                                    return false;

                                if (Body.Length > 0)
                                {
                                    if (Engine.Game.Moderation.requestUnban(Session.User.ID, Body, null, null))
                                    {
                                        Session.castWhisper("The ban has been lifted.");
                                        Engine.Game.Moderation.logModerationAction(Session.User.ID, "unban", Engine.Game.Users.getUserID(Body), "", "Unban via :unban %username%");
                                    }
                                }
                            }
                            break;

                        case "blacklist":
                            {
                                if (!Session.User.hasFuseRight("fuse_administrator_access"))
                                    return false;

                                if (Body.Length > 0)
                                {
                                    int userID = Engine.Game.Users.getUserID(Body);
                                    if (userID > 0)
                                    {
                                        userAccessInformation lastAccess = Engine.Game.Users.getLastAccess(userID);
                                        if (lastAccess != null)
                                        {
                                            Engine.Net.Game.blackListIpAddress(lastAccess.IP);
                                            Engine.Sessions.destroySessions(lastAccess.IP);
                                            Session.castWhisper("IP " + lastAccess.IP + " has been added to the blacklist.");
                                        }
                                    }
                                }
                            }
                            break;

                        case "roomalert":
                            {
                                if (!Session.User.hasFuseRight("fuse_room_alert"))
                                    return false;

                                if (Body.Length > 0)
                                {
                                    if (Engine.Game.Moderation.requestRoomAlert(Session.User.ID, Session.roomID, Body, ""))
                                        Session.castWhisper("Room Alerted.");
                                }
                            }
                            break;

                        case "roomkick":
                            {
                                if (!Session.User.hasFuseRight("fuse_room_kick"))
                                    return false;

                                if (Body.Length > 0)
                                {
                                    if (Engine.Game.Moderation.requestRoomKick(Session.User.ID, Session.roomID, Body, ""))
                                        Session.castWhisper("The room has been emptied.");
                                }
                            }
                            break;

                        case "ha":
                        case "hotelalert":
                            {
                                if (!Session.User.hasFuseRight("fuse_hotelalert"))
                                    return false;

                                if (Body.Length > 0)
                                {
                                    Engine.Game.Moderation.castHotelAlert(Session.User.ID, Body);
                                    Session.castWhisper("Hotel Alert has been sent!");
                                }
                            }
                            break;

                        #endregion

                        #region Debug commands

                        case "debug":
                            {
                                if (!Session.User.hasFuseRight("fuse_debug"))
                                    return false;

                                switch (Body)
                                {
                                    case "reloadstore":
                                        Engine.Game.Items.loadDefinitions();
                                        Engine.Game.Store.loadSales();
                                        Engine.Game.Store.loadCataloguePages();
                                        Session.castWhisper("Re-loaded item definitions, store sales and catalogue pages.");
                                        break;

                                    case "handitems":
                                        Session.itemStripHandler.loadHandItems();
                                        Session.castWhisper("Re-loaded handitems.");
                                        break;
                                }
                            }
                            break;

                        case "createitem":
                            {
                                if (!Session.User.hasFuseRight("fuse_debug"))
                                    return false;

                                string[] args = Body.Split(' ');

                                // Parse data
                                int definitionID = int.Parse(args[0]);
                                int Amount = int.Parse(args[1]);
                                string customData = null;
                                if (args.Length >= 3)
                                    customData = args[2];

                                for (int x = 1; x <= Amount; x++)
                                {
                                    Engine.Game.Items.createItemInstance(definitionID, Session.User.ID, customData);
                                }
                                Session.itemStripHandler.loadHandItems();

                                Session.sendHandStrip("last");
                            }
                            break;

                        case "woodpecker":
                            {
                                if (Body.Length > 0)
                                {
                                    //Engine.Game.Items.createItemInstance(messageID, Session.User.ID, customMessage);
                                    Engine.Game.Moderation.castHotelAlert(Session.User.ID, Body);
                                    Session.castWhisper("Bedankt Nillus voor de test <3");
                                    //Session.sendHandStrip("last");
                                }
                            }
                            break;

                        case "multiplier":
                            int.TryParse(Body, out this.Session.PurchaseMultiplier);
                            break;

                        case "stack":
                            this.Session.StackAnything = float.TryParse(Body, out this.Session.StackHeight);
                            break;
                        #endregion

                        default:
                            return false;
                    }
                }
                catch { Session.castWhisper("Error occurred during processing of the chat command, check your parameters."); }
                return true;
            }
            return false;
        }
        #endregion
    }
}
