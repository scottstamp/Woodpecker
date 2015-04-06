using System;
using System.Drawing;

using Woodpecker.Specialized.Text;
using Woodpecker.Specialized.Encoding;
using Woodpecker.Game.Items;

namespace Woodpecker.Game.Store
{
    /// <summary>
    /// Contains target methods for store related features for logged in users.
    /// </summary>
    public class storeReactor : Reactor
    {
        /// <summary>
        /// 127 - "A"
        /// </summary>
        public void GETUSERCREDITLOG()
        {
            Engine.Game.Store.sendTransactions(ref this.Session);
        }
        /// <summary>
        /// 129 - "BA"
        /// </summary>
        public void REDEEM_VOUCHER()
        {
            string Code = Request.getParameter(0);
            Engine.Game.Store.redeemVoucher(ref this.Session, Code);
        }
        /// <summary>
        /// 190 - "B~"
        /// </summary>
        public void SCR_BUY()
        {
            string[] args = Request.getMixedParameters();
            if (args[0] != "club_habbo")
                return; // Non-existing subscription (Sulake appearantly planned more subscriptions than just 'club_habbo')

            if (Engine.Game.Store.purchaseSubscription(ref this.Session, args[0], int.Parse(args[1])))
            {
                Session.refreshCredits();
                Session.refreshFuseRights();
                Session.refreshBadgeList();
                Session.refreshFigureParts();
                Session.refreshClubStatus();
            }
            else // Failed!
            {
                Response.Initialize(68); // "AD"
                sendResponse();
            }
        }

        /// <summary>
        /// 100 - "Ad"
        /// </summary>
        public void GRPC()
        {
            string[] purchaseArguments = Request.Content.Split(Convert.ToChar(13));
            storeCataloguePage pPage = Engine.Game.Store.getCataloguePage(purchaseArguments[1]);
            if (!pPage.roleHasAccess(Session.User.Role)) // No access
                return;

            storeCatalogueSale pSale = pPage.getSale(purchaseArguments[3]);
            
            #region Credits balance + valid item check
            if (pSale == null || pSale.Price > Session.User.Credits) // Sale not found/not able to purchase
            {
                Response.Initialize(68); // "AD"
                sendResponse();
                return;
            }
            #endregion

            int receivingUserID = Session.User.ID;
            string customData = null;
            string presentBoxNote = null;

            if (!pSale.isPackage)
            {
                #region Handle custom data
                if (pSale.pItem.Behaviour.isDecoration)
                {
                    int decorationValue = 0;
                    if (int.TryParse(purchaseArguments[4], out decorationValue) && decorationValue > 0)
                    {
                        customData = decorationValue.ToString();
                    }
                    else
                    {
                        return;
                    }
                }
                else if (pSale.pItem.Behaviour.isPrizeTrophy)
                {
                    stringFunctions.filterVulnerableStuff(ref purchaseArguments[4], true);
                    fuseStringBuilder sb = new fuseStringBuilder();
                    sb.appendTabbedValue(Session.User.Username);
                    sb.appendTabbedValue(DateTime.Today.ToShortDateString());
                    sb.Append(purchaseArguments[4]); // Inscription

                    customData = sb.ToString();
                }
                if (pSale.saleCode.Length == 4 && pSale.saleCode.IndexOf("pet") == 0) // Pet sale
                {
                    // Verify petname
                    string[] petData = purchaseArguments[4].Split(Convert.ToChar(2));
                    if (petData[0].Length > 15) // Name too long, truncate name
                        petData[0] = petData[0].Substring(0, 15);
                    stringFunctions.filterVulnerableStuff(ref petData[0], true);

                    // Verify pet type
                    int petType = int.Parse(pSale.saleCode.Substring(3));

                    // Verify race
                    byte Race = 0;
                    try { Race = byte.Parse(petData[1]); }
                    catch { }
                    petData[1] = Race.ToString();

                    // Verify color
                    try { ColorTranslator.FromHtml("#" + petData[2]); }
                    catch { return; } // Hax!

                    customData =
                        petData[0] // Name
                        + Convert.ToChar(2)
                        + petType.ToString() // Type 
                        + Convert.ToChar(2)
                        + petData[1] // Race
                        + Convert.ToChar(2)
                        + petData[2]; // Color
                }
                #endregion
            }

            #region Handle presents
            if (purchaseArguments[5] == "1")
            {
                if (purchaseArguments[6].ToLower() != Session.User.Username.ToLower()) // Receiving user is different than buyer
                {
                    receivingUserID = Engine.Game.Users.getUserID(purchaseArguments[6]);
                    if (receivingUserID == 0) // Receiving user was not found
                    {
                        Response.Initialize(76); // "AL"
                        Response.Append(purchaseArguments[6]);
                        sendResponse();
                        return;
                    }
                }

                presentBoxNote = purchaseArguments[7];
                stringFunctions.filterVulnerableStuff(ref presentBoxNote, true);
            }
            #endregion

            // Charge buyer
            Session.User.Credits -= pSale.Price;
            Session.refreshCredits();

            // Create items and deliver
            Engine.Game.Store.requestSaleShipping(receivingUserID, pSale.saleCode, true, purchaseArguments[5] == "1", presentBoxNote, customData);

            // Update user valueables and log transaction
            Session.User.updateValueables();
            Engine.Game.Store.logTransaction(Session.User.ID, "stuff_store", -pSale.Price);
        }
        /// <summary>
        /// 101 - "Ae"
        /// </summary>
        public void GCIX()
        {
            Response.Initialize(126); // "A~"

            storeCataloguePage[] pPages = Engine.Game.Store.getCataloguePages();
            foreach (storeCataloguePage lPage in pPages)
            {
                if (lPage.roleHasAccess(Session.User.Role))
                {
                    Response.appendTabbedValue(lPage.getStringAttribute("name_index"));
                    Response.Append(lPage.getStringAttribute("name"));
                    Response.appendChar(13);
                }
            }
            sendResponse();
        }
        /// <summary>
        /// 102 - "Af"
        /// </summary>
        public void GCAP()
        {
            Response.Initialize(127); // "A"
            string pageName = Request.Content.Split('/')[1];
            storeCataloguePage pPage = Engine.Game.Store.getCataloguePage(pageName);

            if(pPage != null && pPage.roleHasAccess(Session.User.Role))
                Response.Append(pPage.ToString());

            sendResponse();
        }
    }
}
