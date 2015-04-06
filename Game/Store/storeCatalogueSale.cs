using System;
using System.Collections.Generic;

using Woodpecker.Specialized.Text;
using Woodpecker.Game.Items;
using Woodpecker.Game.Items.Pets;

namespace Woodpecker.Game.Store
{
    /// <summary>
    /// Represents an item (package) that is being sold in the catalogue.
    /// </summary>
    public class storeCatalogueSale
    {
        #region Fields
        /// <summary>
        /// The purchase name/code of this sale, used to identify this sale.
        /// </summary>
        private string _saleCode;
        /// <summary>
        /// The purchase name/code of this sale, used to identify this sale.
        /// </summary>
        public string saleCode
        {
            get { return _saleCode; }
        }

        /// <summary>
        /// Holds true if this sale object is a package.
        /// </summary>
        private bool _isPackage;
        /// <summary>
        /// True if this sale object is a package. (multiple items in one buy)
        /// </summary>
        public bool isPackage
        {
            get { return _isPackage; }
        }

        /// <summary>
        /// The amount of Credits this sale costs.
        /// </summary>
        private int _Price;
        /// <summary>
        /// The amount of Credits this sale costs.
        /// </summary>
        public int Price
        {
            get { return _Price; }
        }


        #region Single-item sale
        /// <summary>
        /// The item definition of the item that is sold with this single-item sale. Null if this sale is a package of items.
        /// </summary>
        public itemDefinition pItem;
        /// <summary>
        /// The special sprite ID that holds the sprite ID of items such as 'poster'.
        /// </summary>
        public int pItemSpecialSpriteID;
        #endregion

        /// <summary>
        /// An array of storeCataloguePackageItem objects, representing the package items that are included and their quantity.
        /// </summary>
        private List<storeCataloguePackageItem> pPackageItems;

        /// <summary>
        /// The sale item type character ('d', 'i' or 's') as a string.
        /// </summary>
        public string saleItemType
        {
            get
            {
                if(this.isPackage)
                    return "d";
                else
                {
                    if(this.pItem.Behaviour.isWallItem)
                        return "i";
                    else
                        return "s";
                }
            }
        }
        /// <summary>
        /// The icon of item in this sale. If this sale is a package, then there is no special icon, and the client will display the package icon.
        /// </summary>
        private string itemIcon
        {
            get
            {
                if (this.isPackage)
                    return null;
                else
                    return Engine.Game.Items.generateSpecialSprite(ref this.pItem.Sprite, this.pItemSpecialSpriteID);
            }
        }
        /// <summary>
        /// The size of the item in this sale. If this sale is a package or is a wall item, then there is no size.
        /// </summary>
        private string itemSize
        {
            get
            {
                if (this.isPackage || this.pItem.Behaviour.isWallItem)
                    return null;
                else
                    return "0";
            }
        }
        /// <summary>
        /// The dimensions of the item in this sale. If this sale is a package or is a wall item, then there are no dimensions.
        /// </summary>
        private string itemDimensions
        {
            get
            {
                if (this.isPackage || this.pItem.Behaviour.isWallItem)
                    return null;
                else
                    return pItem.Length + "," + pItem.Width;
            }
        }

        /// <summary>
        /// The name of this sale. If this sale is a package, the internal name value will be returned. Otherwise, the matching value from the external_texts will be returned.
        /// </summary>
        public string Name
        {
            get
            {
                if (this.isPackage)
                    return _packageName;
                else
                    return Engine.Game.Items.getItemName(pItem, pItemSpecialSpriteID);
            }
        }
        /// <summary>
        /// The description of this sale. If this sale is a package, the internal description value will be returned. Otherwise, the matching value from the external_texts will be returned.
        /// </summary>
        public string Description
        {
            get
            {
                if (this.isPackage)
                    return _packageDescription;
                else
                    return Engine.Game.Items.getItemDescription(pItem, pItemSpecialSpriteID);
            }
        }
        /// <summary>
        ///Holds the name of this package as a string. If this sale is not a package, then this field holds null.
        /// </summary>
        private string _packageName = null;
        /// <summary>
        /// Holds the description of this package as a string. If this sale is not a package, then this field holds null.
        /// </summary>
        private string _packageDescription = null;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a blank catalogue sale with a given sale code and a given price.
        /// </summary>
        /// <param name="saleCode">The sale code of this sale.</param>
        /// <param name="Price">The amount of Credits this sale costs.</param>
        public storeCatalogueSale(string saleCode, int Price)
        {
            this._saleCode = saleCode;
            this._Price = Price;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Set the item definition that ships with this sale.
        /// </summary>
        /// <param name="Item">The itemDefinition of the item to set.</param>
        public void setItem(itemDefinition Item, int specialSpriteID)
        {
            if(!this.isPackage)
            {
                this.pItem = Item;
                this.pItemSpecialSpriteID = specialSpriteID;
            }
        }
        /// <summary>
        /// Marks this sale as a package, sets the given name and description and constructs an empty List of the type storeCataloguePackageItem.
        /// </summary>
        /// <param name="packageName">The name of this package.</param>
        /// <param name="packageDescription">The description of this package.</param>
        public void setPackage(string packageName, string packageDescription)
        {
            this._isPackage = true;
            this._packageName = packageName;
            this._packageDescription = packageDescription;
            this.pPackageItems = new List<storeCataloguePackageItem>();
        }
        /// <summary>
        /// Adds a given itemDefinition item with a given quantity to the package sale.
        /// </summary>
        /// <param name="Item">The itemDefinition object of the item to add to the package.</param>
        /// <param name="Amount">The amount of times this item should be added to the package.</param>
        public void addPackageItem(itemDefinition Item, int Amount, int specialSpriteID)
        {
            if (!this.isPackage)
                return;

            storeCataloguePackageItem pPackageItem = new storeCataloguePackageItem(Item, Amount, specialSpriteID);
            this.pPackageItems.Add(pPackageItem);
        }

        public void factorizeItems(int receivingUserID, string extraInformation)
        {

        }
        public stripItem[] getItemInstances()
        {
            List<stripItem> Items = new List<stripItem>();
            if (!isPackage)
            {
                Items.Add(Engine.Game.Items.createItemFromDefinition(this.pItem, this.pItemSpecialSpriteID));
            }
            else
            {
                foreach(storeCataloguePackageItem lPackageItem in this.pPackageItems)
                {
                    for(int i = 0; i < lPackageItem.pAmount; i++)
                    {
                        Items.Add(Engine.Game.Items.createItemFromDefinition(lPackageItem.pItem, lPackageItem.specialSpriteID));
                    }
                }
            }

            return Items.ToArray();
        }
        public List<storeCataloguePackageItem> getPackageItems()
        {
            if (this.pPackageItems == null)
                return new List<storeCataloguePackageItem>();
            else
                return this.pPackageItems;
        }
        /// <summary>
        /// Returns 'true' if two storeCatalogueSale objects have the same salecode.
        /// </summary>
        /// <param name="obj">The storeCatalogueSale object to compare against this storeCatalogueSale.</param>
        public override bool Equals(object obj)
        {
            return (this.saleCode == ((storeCatalogueSale)obj).saleCode);
        }
        /// <summary>
        /// Returns the hashcode of l u l z.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// Converts this catalogue page sale representation to a string and returns it.
        /// </summary>
        public override string ToString()
        {
            try
            {
                fuseStringBuilder szSaleDetails = new fuseStringBuilder();
                szSaleDetails.appendTabbedValue(this.Name);
                szSaleDetails.appendTabbedValue(this.Description);
                szSaleDetails.appendTabbedValue(this.Price.ToString());
                szSaleDetails.appendTabbedValue(null);
                szSaleDetails.appendTabbedValue(this.saleItemType); // 'i', 's', 'd' etc
                szSaleDetails.appendTabbedValue(this.itemIcon);
                szSaleDetails.appendTabbedValue(this.itemSize);
                szSaleDetails.appendTabbedValue(this.itemDimensions);
                szSaleDetails.appendTabbedValue(this.saleCode);

                if (this.isPackage || this.pItem.Sprite == "poster")
                    szSaleDetails.appendTabbedValue(null);
                if (this.isPackage)
                {
                    szSaleDetails.appendTabbedValue(this.pPackageItems.Count.ToString());
                    foreach (storeCataloguePackageItem lPackageItem in this.pPackageItems)
                    {
                        szSaleDetails.appendTabbedValue(Engine.Game.Items.generateSpecialSprite(ref lPackageItem.pItem.Sprite, lPackageItem.specialSpriteID));
                        szSaleDetails.appendTabbedValue(lPackageItem.pAmount.ToString());
                        szSaleDetails.appendTabbedValue(lPackageItem.pItem.Color);
                    }
                }
                else
                {
                    if (!this.pItem.Behaviour.isWallItem) // Wall items do not have colors
                        szSaleDetails.appendTabbedValue(this.pItem.Color);
                }

                return szSaleDetails.ToString();
            }
            catch { return ""; }
        }
        #endregion
    }
    /// <summary>
    /// Represents an item + a quantity, of an item sold in a package sale.
    /// </summary>
    public struct storeCataloguePackageItem
    {
        #region Fields
        /// <summary>
        /// The item that is for sale.
        /// </summary>
        public itemDefinition pItem;
        /// <summary>
        /// The amount of instances this item ships with.
        /// </summary>
        public int pAmount;
        /// <summary>
        /// The ID for items with sprites such as 'poster' and 'wallpaper'.
        /// </summary>
        public int specialSpriteID;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a storeCataloguePackageItem instance with a given item and quantity.
        /// </summary>
        /// <param name="pItemDefinition">The itemDefinition object of the item.</param>
        /// <param name="pItemAmount">The amount of times this item appears in the package.</param>
        public storeCataloguePackageItem(itemDefinition pItemDefinition, int pItemAmount, int specialSpriteID)
        {
            this.pItem = pItemDefinition;
            this.pAmount = pItemAmount;
            this.specialSpriteID = specialSpriteID;
        }
        #endregion
    }
}
