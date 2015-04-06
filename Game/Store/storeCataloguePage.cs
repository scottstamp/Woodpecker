using System;
using System.Data;
using System.Collections.Generic;

using Woodpecker.Storage;
using Woodpecker.Specialized.Enhancement;
using Woodpecker.Specialized.Text;
using Woodpecker.Game.Items;
using Woodpecker.Game.Users.Roles;

namespace Woodpecker.Game.Store
{
    /// <summary>
    /// Represents a 'catalogue page' in the virtual item store.
    /// </summary>
    public class storeCataloguePage : AttributeSet
    {
        #region Fields
        /// <summary>
        /// The database ID of this catalogue page.
        /// </summary>
        public int ID;
        /// <summary>
        /// The minimum access role required to access this catalogue page as a value of the Woodpecker.Game.Users.Roles.userRole enum.
        /// </summary>
        public userRole minimumAccessRole;
        /// <summary>
        /// A List (string), containing the sale codes of the sales that are sold on this page.
        /// </summary>
        private List<string> saleCodes = new List<string>();
        /// <summary>
        /// The cached string representation of this page.
        /// </summary>
        private string _szObj = null;
        #endregion

        #region Methods
        /// <summary>
        /// Sets the minimum access role for this page.
        /// </summary>
        /// <param name="Role">A value of the Woodpecker.Game.Users.Roles.userRole enum.</param>
        public void setMinimumAccessRole(userRole Role)
        {
            minimumAccessRole = Role;
        }
        /// <summary>
        /// Returns a boolean indicating if a given user role has access to this catalogue page.
        /// </summary>
        /// <param name="Role">The user role to check as a value of the Woodpecker.Game.Users.Roles.userRole enum.</param>
        public bool roleHasAccess(userRole Role)
        {
            return ((int)Role >= (int)minimumAccessRole);
        }
        /// <summary>
        /// Initialize the sale codes of the sales that are sold on this page and puts them in the correct order.
        /// </summary>
        public void initializeSales()
        {
            if (this.saleCodes != null)
                this.saleCodes.Clear();
            this._szObj = null;
            this.saleCodes = new List<string>();

            Database dbClient = new Database(false, true);
            dbClient.addParameterWithValue("pageid", this.ID);
            dbClient.Open();

            if (dbClient.Ready)
            {
                foreach (DataRow dRow in dbClient.getTable("SELECT salecode FROM store_catalogue_sales WHERE pageid = @pageid ORDER BY orderid ASC").Rows)
                {
                    this.saleCodes.Add((string)dRow["salecode"]);
                }
            }
        }
        /// <summary>
        /// Tries to return the storeCatalogueSale object of a sale (given by it's sale code) on this catalogue page. If the sale is not on this page, or the sale does not exist (anymore), then null is returned.
        /// </summary>
        /// <param name="saleCode">The salecode of the sale to get.</param>
        public storeCatalogueSale getSale(string saleCode)
        {
            if (this.saleCodes.Contains(saleCode)) // Sale is on this page
                return Engine.Game.Store.getSale(saleCode);
            else
                return null;
        }
        /// <summary>
        /// Returns an array of the type storeCatalogueSale with all the store catalogue sales that are being sold on this page.
        /// </summary>
        /// <returns></returns>
        public storeCatalogueSale[] getSales()
        {
            List<storeCatalogueSale> Sales = new List<storeCatalogueSale>();
            foreach (string lSaleCode in this.saleCodes)
            {
                storeCatalogueSale pSale = Engine.Game.Store.getSale(lSaleCode);
                if (pSale != null)
                    Sales.Add(pSale);
            }

            return Sales.ToArray();
        }

        /// <summary>
        /// Converts this store page representation to a string and returns it.
        /// </summary>
        public override string ToString()
        {
            if (this._szObj == null) // Not made yet!
            {
                fuseStringBuilder FSB = new fuseStringBuilder();
                FSB.appendKeyValueParameter("i", base.getStringAttribute("name_index")); // Index name of page
                FSB.appendKeyValueParameter("n", base.getStringAttribute("name")); // Display name of page
                FSB.appendKeyValueParameter("l", base.getStringAttribute("layout")); // Layout type of page
                FSB.appendKeyValueParameter("g", base.getStringAttribute("img_headline")); // Name of headline image in c_images/catalogue/ directory OR internal cast files of client
                FSB.appendKeyValueParameter("e", base.getStringAttribute("img_teasers")); // List of teaser image names, separated by commas
                FSB.appendKeyValueParameter("h", base.getStringAttribute("body")); // Body text of page
                if (base.hasSetAttribute("label_pick")) // 'Click for more information' label
                    FSB.appendKeyValueParameter("w", base.getStringAttribute("label_pick"));
                if (base.hasSetAttribute("label_extra_s")) // Extra information
                    FSB.appendKeyValueParameter("s", base.getStringAttribute("label_extra_s"));

                // Custom data (t1:, t2: etc)
                for (int attID = 1; attID < 11; attID++)
                {
                    string szExtraAttribute = "label_extra_t_" + attID;
                    if (!base.hasSetAttribute(szExtraAttribute))
                        break;

                    FSB.appendKeyValueParameter("t" + attID, base.getStringAttribute(szExtraAttribute));
                }

                foreach (storeCatalogueSale lSale in this.getSales())
                {
                    FSB.appendKeyValueParameter("p", lSale.ToString());
                }
                this._szObj = FSB.ToString();
            }

            return this._szObj;
        }
        #endregion
    }
}
