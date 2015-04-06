using System;

using Woodpecker.Game.Rooms.Units;

namespace Woodpecker.Game.Items
{
    public static class carryItemHelper
    {
        #region Fields
        public enum handItemType
        {
            drink = 0,
            eat = 1,
            item = 2
        }
        private static handItemType[] handItemTypes;
        public static void setDefaultHandItemTypes()
        {
            handItemTypes = new handItemType[26];
            handItemTypes[1] = handItemType.drink;  // Tea
            handItemTypes[2] = handItemType.drink;  // Juice
            handItemTypes[3] = handItemType.eat;    // Carrot
            handItemTypes[4] = handItemType.eat;    // Ice-cream
            handItemTypes[5] = handItemType.drink;  // Milk
            handItemTypes[6] = handItemType.drink;  // Blackcurrant
            handItemTypes[7] = handItemType.drink;  // Water
            handItemTypes[8] = handItemType.drink;  // Regular
            handItemTypes[9] = handItemType.drink;  // Decaff
            handItemTypes[10] = handItemType.drink; // Latte
            handItemTypes[11] = handItemType.drink; // Mocha
            handItemTypes[12] = handItemType.drink; // Macchiato
            handItemTypes[13] = handItemType.drink; // Espresso
            handItemTypes[14] = handItemType.drink; // Filter
            handItemTypes[15] = handItemType.drink; // Iced
            handItemTypes[16] = handItemType.drink; // Cappuccino
            handItemTypes[17] = handItemType.drink; // Java
            handItemTypes[18] = handItemType.drink; // Tap
            handItemTypes[19] = handItemType.drink; // H*bbo Cola
            handItemTypes[20] = handItemType.item;  // Camera
            handItemTypes[21] = handItemType.eat;   // Hamburger
            handItemTypes[22] = handItemType.drink; // Lime H*bbo Soda
            handItemTypes[23] = handItemType.drink; // Beetroot H*bbo Soda
            handItemTypes[24] = handItemType.drink; // Bubble juice from 1999
            handItemTypes[25] = handItemType.drink; // Lovejuice
        }
        #endregion

        #region Methods
        public static void setHandItem(ref roomUser User, string Item)
        {
            int ID = 0;
            if (int.TryParse(Item, out ID))
            {
                try
                {
                    string carryStatus = "";
                    string useStatus = "";
                    handItemType iType = handItemTypes[ID];
                    if(iType == handItemType.eat)
                    {
                        carryStatus = "carryf";
                        useStatus = "eat";
                    }
                    else if(iType == handItemType.drink)
                    {
                        carryStatus = "carryd";
                        useStatus = "drink";
                    }
                    else if(iType == handItemType.item)
                    {
                        carryStatus = "cri";
                        useStatus = "usei";
                    }

                    User.removeStatus("dance");
                    User.addStatus("handitem", carryStatus, ID.ToString(), 120, useStatus, 12, 1);
                }
                catch { }
            }
        }
        #endregion
    }
}
