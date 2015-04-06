using System;

using Woodpecker.Specialized.Text;
using Woodpecker.Net.Game.Messages;

using Woodpecker.Game.Rooms.Instances;
using Woodpecker.Game.Rooms.Units;
using Woodpecker.Game.Items;

namespace Woodpecker.Game.Rooms.Instances.Interaction
{
    /// <summary>
    /// Expands the roomReactor reactor with target methods that are for user flats only.
    /// </summary>
    public partial class flatReactor : roomReactor
    {
        /// <summary>
        /// 65 - "AA"
        /// </summary>
        public void GETSTRIP()
        {
            Session.sendHandStrip(Request.Content);
        }
        /// <summary>
        /// 66 - "AB"
        /// </summary>
        public void FLATPROPBYITEM()
        {
            if (Session.roomInstance.sessionHasRights(Session.ID)) // Permission
            {
                int itemID = int.Parse(Request.Content.Split('/')[1]);
                stripItem pItem = Session.itemStripHandler.getHandItem(itemID);
                if (pItem != null && pItem.Definition.Behaviour.isDecoration)
                {
                    Session.itemStripHandler.removeHandItem(itemID, true);

                    int decorationValue = int.Parse(pItem.customData);
                    if (pItem.Definition.Sprite == "wallpaper")
                        Session.roomInstance.Information.Wallpaper = decorationValue;
                    else // Floor
                        Session.roomInstance.Information.Floor = decorationValue;

                    serverMessage msgUpdate = new serverMessage(46); // "@n"
                    msgUpdate.Append(pItem.Definition.Sprite);
                    msgUpdate.Append("/");
                    msgUpdate.Append(decorationValue);
                    Session.roomInstance.sendMessage(msgUpdate);

                    Session.roomInstance.Information.updateFlatProperties();
                }
            }
        }

        /// <summary>
        /// 68 - "AD"
        /// </summary>
        public void TRADE_DECLINE()
        {
            if (Session.itemStripHandler.tradeAccept)
            {
                Session.itemStripHandler.tradeAccept = false;
                Session.refreshTradeBoxes();

                if (Engine.Sessions.getSession(Session.itemStripHandler.tradePartnerSessionID).itemStripHandler.tradeAccept)
                {
                    Session.roomInstance.sendMessage(genericMessageFactory.createMessageBoxCast("Okay, you will trade!"));
                }
            }
        }
        /// <summary>
        /// 69 - "AE"
        /// </summary>
        public void TRADE_ACCEPT()
        {
            if (Session.itemStripHandler.isTrading && !Session.itemStripHandler.tradeAccept)
            {
                Session.itemStripHandler.tradeAccept = true;
                Session.refreshTradeBoxes();

                Woodpecker.Sessions.Session partnerSession = Engine.Sessions.getSession(this.Session.itemStripHandler.tradePartnerSessionID);
                if (partnerSession.itemStripHandler.tradeAccept)
                {
                    Session.itemStripHandler.swapTradeOffers(partnerSession.itemStripHandler);
                    System.Threading.Thread.Sleep(150); // Wait to display the refresh before the window disappears
                    Session.abortTrade();
                    // Trade complete!
                }
            }
        }
        /// <summary>
        /// 70 - "AF"
        /// </summary>
        public void TRADE_CLOSE()
        {
            if (Session.itemStripHandler.isTrading)
            {
                Session.abortTrade();
            }
        }
        /// <summary>
        /// 71 - "AG"
        /// </summary>
        public void TRADE_OPEN()
        {
            if (Session.itemStripHandler.isTrading || !Session.User.hasFuseRight("fuse_trade")) // Can't trade
                return;

            int tradePartnerRoomUserID = int.Parse(Request.Content);
            roomUser tradePartner = Session.roomInstance.getRoomUser(tradePartnerRoomUserID);
            if (tradePartner == null || tradePartner.Session.itemStripHandler.isTrading) // Can't trade
                return;

            Session.roomInstance.getRoomUser(Session.ID).addStatus("trd", "trd", null, 0, null, 0, 0);
            tradePartner.addStatus("trd", "trd", null, 0, null, 0, 0);

            Session.itemStripHandler.initTrade(tradePartner.Session.ID);
            tradePartner.Session.itemStripHandler.initTrade(Session.ID);

            Session.refreshTradeBoxes();
        }
        /// <summary>
        /// 72 - "AH"
        /// </summary>
        public void TRADE_ADDITEM()
        {
            if (Session.itemStripHandler.isTrading)
            {
                int itemID = int.Parse(Request.Content);
                stripItem pItem = Session.itemStripHandler.getHandItem(itemID);
                if (pItem != null && !Session.itemStripHandler.itemIsInTradeOffer(itemID)) // Item tradeable check here todo
                {
                    Session.itemStripHandler.addItemToTradeOffer(itemID);
                    Session.refreshTradeBoxes();
                }
            }
        }

        /// <summary>
        /// 95 - "A_"
        /// </summary>
        public void KICKUSER()
        {
            roomUser Me = Session.roomInstance.getRoomUser(Session.ID);
            if (Me.hasRights)
            {
                roomUser Target = Session.roomInstance.getRoomUser(Request.Content);
                if (Target == null || (Target.hasRights && !Me.isOwner) || (Target.Session.User.Role > Session.User.Role)) // Invalid
                    return;

                Target.Session.kickFromRoom("");
            }
        }
        /// <summary>
        /// 96 - "A`"
        /// </summary>
        public void ASSIGNRIGHTS()
        {
            if (Session.roomInstance.sessionHasFlatAdmin(Session.ID))
            {
                roomUser Target = Session.roomInstance.getRoomUser(Request.Content);
                if (Target == null || Target.hasRights) // Invalid
                    return;

                Target.hasRights = true;
                Target.refreshRights();
                Engine.Game.Rooms.addRoomRights(Session.roomID, Target.Session.User.ID);
            }
        }
        /// <summary>
        /// 97 - "Aa"
        /// </summary>
        public void REMOVERIGHTS()
        {
            if (Session.roomInstance.sessionHasFlatAdmin(Session.ID))
            {
                roomUser Target = Session.roomInstance.getRoomUser(Request.Content);
                if (Target == null || !Target.hasRights || Target.isOwner) // Invalid
                    return;

                Target.hasRights = false;
                Target.refreshRights();
                Target.Session.gameConnection.sendMessage(new Net.Game.Messages.serverMessage(43)); // "@k"
                Engine.Game.Rooms.removeRoomRights(Session.roomID, Target.Session.User.ID);
            }
        }
        /// <summary>
        /// 98 - "Ab"
        /// </summary>
        public void LETUSERIN()
        {
            string ringingUsername = Request.getParameter(0);
            bool Decision = (Request.Content[Request.Content.Length - 1] == 'A'); // Base64 boolean
            Session.roomInstance.answerDoorbell(Session.ID, ringingUsername, Decision);
        }
    }
}