using System;
using System.Data;
using System.Threading;
using System.Collections.Generic;

using Woodpecker.Storage;
using Woodpecker.Specialized.Text;
using Woodpecker.Net.Game.Messages;

using Woodpecker.Game.Items;
using Woodpecker.Game.Items.Pets;
using Woodpecker.Game.Rooms.Units;
using Woodpecker.Game.Rooms.Pathfinding;

namespace Woodpecker.Game.Rooms.Instances.Interaction
{
    public partial class flatReactor : roomReactor
    {
        /// <summary>
        /// 76 - "AL"
        /// </summary>
        public void THROW_DICE()
        {
            int itemID = int.Parse(Request.Content);
            Session.roomInstance.interactWithDice(itemID, Session.ID, true);
        }
        /// <summary>
        /// 77 - "AM"
        /// </summary>
        public void DICE_OFF()
        {
            int itemID = int.Parse(Request.Content);
            Session.roomInstance.interactWithDice(itemID, Session.ID, false);
        }
    }
}
namespace Woodpecker.Game.Rooms.Instances
{
    public partial class roomInstance
    {
        #region Methods
        public void interactWithDice(int itemID, uint sessionID, bool spinDice)
        {
            floorItem pItem = this.getFloorItem(itemID);
            if (pItem != null && pItem.Definition.Behaviour.isDice) // Valid item
            {
                roomUser pUser = this.getRoomUser(sessionID);
                if (pUser == null || !tilesTouch(pItem.X, pItem.Y, pUser.X, pUser.Y))
                    return; // Invalid position of room user and dice item
                pUser = null;

                serverMessage Message = new serverMessage(90); // "AZ"
                Message.Append(itemID);
                itemID *= 38;

                int randomNumber = 0;
                if (spinDice) // New spin
                {
                    this.sendMessage(Message); // Start spin animation for clients

                    // Generate random number
                    randomNumber = new Random(DateTime.Now.Millisecond).Next(1, 7); // 1-6
                    itemID += randomNumber;
                }

                Message.Append(" " + itemID.ToString()); // Append 'new' item ID
                if (spinDice) // New spin, send delayed message
                    this.sendMessage(Message.ToString(), 1000);
                else // Send message immediately
                    this.sendMessage(Message);

                pItem.customData = randomNumber.ToString(); // Set custom data
                pItem.requiresUpdate = true; // Request update of dice customdata
            }
        }
        #endregion
    }
}