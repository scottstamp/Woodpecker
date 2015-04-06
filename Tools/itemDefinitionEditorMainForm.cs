using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Woodpecker.Storage;
using Woodpecker.Game.Items;
using Woodpecker.Game.Items.Pets;

namespace Woodpecker.Tools
{
    public partial class itemDefinitionEditorMainForm : Form
    {
        public itemDefinitionEditorMainForm()
        {
            InitializeComponent();
        }

        private void itemDefinitionEditorMainForm_Load(object sender, EventArgs e)
        {
            displayItemDefinitionBehaviour(itemDefinition.itemBehaviourContainer.Parse(""));
            displayItemDefinitions();
        }
        #region Own methods
        private void displayItemDefinitionBehaviour(itemDefinition.itemBehaviourContainer Behaviour)
        {
            lstBehaviour.Items.Clear();
            lstBehaviour.Items.Add("Wallitem", Behaviour.isWallItem);
            lstBehaviour.Items.Add("Solid", Behaviour.isSolid);
            lstBehaviour.Items.Add("Can sit on top", Behaviour.canSitOnTop);
            lstBehaviour.Items.Add("Can lay on top", Behaviour.canLayOnTop);
            lstBehaviour.Items.Add("Can stand on top", Behaviour.canStandOnTop);
            lstBehaviour.Items.Add("Can stack on top", Behaviour.canStackOnTop);
            lstBehaviour.Items.Add("Roller", Behaviour.isRoller);
            lstBehaviour.Items.Add("Public space object", Behaviour.isPublicSpaceObject);

            lstBehaviour.Items.Add("Requires rights for interaction", Behaviour.requiresRightsForInteraction);
            lstBehaviour.Items.Add("Requires touching for interaction", Behaviour.requiresTouchingForInteraction);

            lstBehaviour.Items.Add("Customdata: TRUE/FALSE", Behaviour.customDataTrueFalse);
            lstBehaviour.Items.Add("Customdata: ON/OFF", Behaviour.customDataOnOff);
            lstBehaviour.Items.Add("Customdata: numeric on/off", Behaviour.customDataNumericOnOff);
            lstBehaviour.Items.Add("Customdata: numeric state", Behaviour.customDataNumericState);

            lstBehaviour.Items.Add("Invisible", Behaviour.isInvisible);
            lstBehaviour.Items.Add("Decoration", Behaviour.isDecoration);
            lstBehaviour.Items.Add("Post.it", Behaviour.isPostIt);
            lstBehaviour.Items.Add("Door", Behaviour.isDoor);
            lstBehaviour.Items.Add("Teleporter", Behaviour.isTeleporter);
            lstBehaviour.Items.Add("Dice", Behaviour.isDice);
            lstBehaviour.Items.Add("Prizetrophy", Behaviour.isPrizeTrophy);
            lstBehaviour.Items.Add("Redeemable", Behaviour.isRedeemable);
            lstBehaviour.Items.Add("Soundmachine", Behaviour.isSoundMachine);
            lstBehaviour.Items.Add("Soundmachine sample set", Behaviour.isSoundMachineSampleSet);
        }
        private void setNonWallItemPropertiesEnabledState(bool newState)
        {
            numItemLength.Enabled = newState;
            numItemWidth.Enabled = newState;
            numItemTopHeight.Enabled = newState;
            // pub
        }
        private void displayItemDefinitions()
        {
            this.boxEditor.Enabled = false;
            this.lvItemDefinitions.Items.Clear();
            Engine.Game.Items.loadDefinitions();

            if (this.tsModifySearchDefinitionText.Text == "")
            {
                foreach (itemDefinition lDefinition in Engine.Game.Items.getItemDefinitionCollection().Values)
                {
                    this.addItemDefinitionToList(lDefinition);
                }
            }
            else
            {
                Database dbClient = new Database(false, true);
                dbClient.addParameterWithValue("criteria", "%" + this.tsModifySearchDefinitionText.Text + "%");
                dbClient.Open();

                if (dbClient.Ready)
                {
                    foreach (DataRow dRow in dbClient.getTable("SELECT id FROM items_definitions WHERE sprite LIKE @criteria ORDER BY id ASC").Rows)
                    {
                        int ID = (int)dRow["id"];
                        this.addItemDefinitionToList(Engine.Game.Items.getItemDefinition(ID));
                    }
                }
            }
            this.lvItemDefinitions.Refresh();
        }
        private void addItemDefinitionToList(itemDefinition pDefinition)
        {
            ListViewItem pItem = new ListViewItem(pDefinition.ID.ToString());
            pItem.SubItems.Add(pDefinition.Sprite);
            pItem.SubItems.Add(pDefinition.Color);
            if (pDefinition.Behaviour.isPublicSpaceObject)
                pItem.SubItems.Add("none");
            else
                pItem.SubItems.Add(pDefinition.Name);

            this.lvItemDefinitions.Items.Add(pItem);
        }
        public itemDefinition selectedItemDefinition
        {
            get
            {
                itemDefinition pDefinition = null;
                try
                {
                    int ID = int.Parse(this.lvItemDefinitions.SelectedItems[0].SubItems[0].Text);
                    pDefinition = Engine.Game.Items.getItemDefinition(ID);
                }
                catch { }

                return pDefinition;
            }
        }
        public int[] selectedItemDefinitionIDs
        {
            get
            {
                List<int> ret = new List<int>();
                foreach (ListViewItem lItem in this.lvItemDefinitions.SelectedItems)
                {
                    ret.Add(int.Parse(lItem.SubItems[0].Text));
                }
                if (ret.Count == 0)
                    ret.Add(int.Parse(this.txtID.Text));

                return ret.ToArray();
            }
        }
        private void displayItemDefinition(itemDefinition pDefinition)
        {
            if (pDefinition == null)
                return;

            txtID.Text = pDefinition.ID.ToString();
            if (pDefinition.Behaviour.isPublicSpaceObject)
            {
                txtDirectoryID.Text = "0";
                txtExtTextsName.Text = "none";
            }
            else
            {
                txtDirectoryID.Text = pDefinition.directoryID.ToString();
                txtExtTextsName.Text = pDefinition.Name;
            }

            txtSprite.Text = pDefinition.Sprite;
            txtItemColor.Text = pDefinition.Color;

            numItemLength.Value = (decimal)pDefinition.Length;
            numItemWidth.Value = (decimal)pDefinition.Width;
            numItemTopHeight.Value = (decimal)pDefinition.topHeight;

            this.setNonWallItemPropertiesEnabledState(!pDefinition.Behaviour.isWallItem);
            this.displayItemDefinitionBehaviour(pDefinition.Behaviour);
            this.boxEditor.Enabled = true;
        }
        private bool bhvChkSelected(string Name)
        {
            return this.lstBehaviour.CheckedItems.Contains(Name);
        }
        #endregion

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void tsModifyAddDefinition_Click(object sender, EventArgs e)
        {
            itemDefinition newDefinition = new itemDefinition();
            Database dbClient = new Database(true, true);
            newDefinition.ID = dbClient.getInteger("SELECT MAX(id) + 1 FROM items_definitions"); 
            displayItemDefinition(newDefinition);

            displayMessageBox("Fill in the details for this new item definition below and click the button to save.", MessageBoxIcon.Information);
        }

        private void lvItemDefinitions_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.displayItemDefinition(this.selectedItemDefinition);
        }

        private void cmbItemType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cmdSaveItemDefinition_Click(object sender, EventArgs e)
        {
            int[] selectedItemDefinitionIDs = this.selectedItemDefinitionIDs;

            int dirID = 0;
            if (!int.TryParse(txtDirectoryID.Text, out dirID) || dirID < 0)
            {
                displayMessageBox("Field 'directory ID' holds no valid value. The value has to be numeric and atleast zero.", MessageBoxIcon.Exclamation);
                return;
            }
            if (dirID == 0)
                displayMessageBox("Field 'directory ID' holds the value 0. Most furniture items will appear as PlaceHolder boxes if their cast file cannot be resolved from the DCR directory.\r\nIf you are unsure about the directory ID of this item definition, you might want to use the helper tool Chestnut that will index the directory IDs of all item cast files in your DCR directory.", MessageBoxIcon.Information);

            if (txtSprite.Text == "")
            {
                displayMessageBox("The field 'Sprite' cannot be empty.", MessageBoxIcon.Exclamation);
                return;
            }

            itemDefinition editedDefinition = new itemDefinition();
            editedDefinition.directoryID = dirID;
            editedDefinition.Sprite = txtSprite.Text;
            editedDefinition.Color = txtItemColor.Text;
            editedDefinition.Length = (byte)numItemLength.Value;
            editedDefinition.Width = (byte)numItemWidth.Value;
            editedDefinition.topHeight = (float)numItemTopHeight.Value;

            itemDefinition.itemBehaviourContainer newBehaviourContainer = new itemDefinition.itemBehaviourContainer();
            foreach (string s in lstBehaviour.CheckedItems)
            {
                switch (s)
                {
                    #region Primitive types
                    case "Wallitem":
                        newBehaviourContainer.isWallItem = true;
                        break;
                    case "Solid":
                        newBehaviourContainer.isSolid = true;
                        break;
                    case "Can sit on top":
                        newBehaviourContainer.canSitOnTop = true;
                        break;
                    case "Can lay on top":
                        newBehaviourContainer.canLayOnTop = true;
                        break;
                    case "Can stand on top":
                        newBehaviourContainer.canStandOnTop = true;
                        break;
                    case "Can stack on top":
                        newBehaviourContainer.canStackOnTop = true;
                        break;
                    case "Roller":
                        newBehaviourContainer.isRoller = true;
                        break;
                    case "Public space object":
                        newBehaviourContainer.isPublicSpaceObject = true;
                        break;
                    case "Invisible":
                        newBehaviourContainer.isInvisible = true;
                        break;
                    #endregion

                    #region Interaction requirements
                    case "Requires rights for interaction":
                        newBehaviourContainer.requiresRightsForInteraction = true;
                        break;
                    case "Requires touching for interaction":
                        newBehaviourContainer.requiresTouchingForInteraction = true;
                        break;
                    #endregion

                    #region Custom data usage
                    case "Customdata: TRUE/FALSE":
                        newBehaviourContainer.customDataTrueFalse = true;
                        break;
                    case "Customdata: ON/OFF":
                        newBehaviourContainer.customDataOnOff = true;
                        break;
                    case "Customdata: numeric on/off":
                        newBehaviourContainer.customDataNumericOnOff = true;
                        break;
                    case "Customdata: numeric state":
                        newBehaviourContainer.customDataNumericState = true;
                        break;
                    #endregion

                    #region Item usage
                    case "Decoration":
                        newBehaviourContainer.isDecoration = true;
                        break;
                    case "Post.it":
                        newBehaviourContainer.isPostIt = true;
                        break;
                    case "Door":
                        newBehaviourContainer.isDoor = true;
                        break;
                    case "Teleporter":
                        newBehaviourContainer.isTeleporter = true;
                        break;
                    case "Dice":
                        newBehaviourContainer.isDice = true;
                        break;
                    case "Prizetrophy":
                        newBehaviourContainer.isPrizeTrophy = true;
                        break;
                    case "Redeemable":
                        newBehaviourContainer.isRedeemable = true;
                        break;
                    case "Soundmachine":
                        newBehaviourContainer.isSoundMachine = true;
                        break;
                    case "Soundmachine sample set":
                        newBehaviourContainer.isSoundMachineSampleSet = true;
                        break;
                    #endregion
                }
            }

            editedDefinition.Behaviour = newBehaviourContainer;
            foreach (int definitionID in selectedItemDefinitionIDs)
            {
                editedDefinition.ID = definitionID;
                if (selectedItemDefinitionIDs.Length > 1) // More than one item selected, keep original directory ID/sprite/color/length/width intact
                {
                    itemDefinition oldDefinition = Engine.Game.Items.getItemDefinition(definitionID);
                    if (oldDefinition == null)
                        continue;

                    editedDefinition.directoryID = oldDefinition.directoryID;
                    editedDefinition.Sprite = oldDefinition.Sprite;
                    editedDefinition.Color = oldDefinition.Color;
                    editedDefinition.Length = oldDefinition.Length;
                    editedDefinition.Width = oldDefinition.Width;
                }
                this.saveItemDefinition(editedDefinition);
            }

            this.displayItemDefinitions();
            Engine.Game.Store.loadCataloguePages();

            displayMessageBox("Selected item definitions saved successfully.", MessageBoxIcon.Information);
        }
        private void setInputFieldsToItemDefinition(ref itemDefinition pDefinition)
        {
            pDefinition.directoryID = int.Parse(txtDirectoryID.Text);
            pDefinition.Sprite = txtSprite.Text;
            pDefinition.Color = txtItemColor.Text;
            pDefinition.Length = (byte)numItemLength.Value;
            pDefinition.Width = (byte)numItemWidth.Value;
        }
        private void saveItemDefinition(itemDefinition pDefinition)
        {
            Database dbClient = new Database(false, false);
            dbClient.addParameterWithValue("definitionid", pDefinition.ID);
            dbClient.addParameterWithValue("directoryid", pDefinition.directoryID);
            dbClient.addParameterWithValue("sprite", pDefinition.Sprite);
            dbClient.addParameterWithValue("color", pDefinition.Color);
            dbClient.addParameterWithValue("length", pDefinition.Length);
            dbClient.addParameterWithValue("width", pDefinition.Width);
            dbClient.addParameterWithValue("topheight", pDefinition.topHeight);
            dbClient.addParameterWithValue("behaviour", pDefinition.Behaviour.ToString());
            
            dbClient.Open();
            if (dbClient.Ready)
            {
                dbClient.runQuery("DELETE FROM items_definitions WHERE id = @definitionid LIMIT 1"); // Drop old definition
                dbClient.runQuery( // Insert new/edited definition
                    "INSERT INTO items_definitions " +
                    "VALUES " +
                    "(@definitionid,@directoryid,@sprite,@color,@length,@width,@topheight,@behaviour)");
                dbClient.Close();
            }
        }
        private void displayMessageBox(string Text, MessageBoxIcon Icon)
        {
            MessageBox.Show(Text, "Woodpecker : Item Definition Editor", MessageBoxButtons.OK, Icon);
        }

        private void tsModifyRemoveDefinition_Click(object sender, EventArgs e)
        {
            int[] toDeleteIDs = this.selectedItemDefinitionIDs;
            if (toDeleteIDs.Length > 0)
            {
                if (MessageBox.Show("Are you sure you want to delete the entries you have selected?\r\nDeleting item definitions will remove this item from all catalogue sales, and already created instances of this item will not function anymore.\r\nAre you sure?", "Woodpecker : Item Definition Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    return;

                foreach (int toDeleteID in toDeleteIDs)
                {
                    this.deleteItemDefinition(toDeleteID);
                }
                displayMessageBox("Successfully deleted selected item definitions from definitions table and from all catalogue sales.", MessageBoxIcon.Information);

                this.displayItemDefinitions();
                Engine.Game.Store.loadCataloguePages();
            }
        }
        private void deleteItemDefinition(int ID)
        {
            Database dbClient = new Database(false, false);
            dbClient.addParameterWithValue("definitionid", ID);

            dbClient.Open();
            if (dbClient.Ready)
            {
                dbClient.runQuery("DELETE FROM items_definitions WHERE id = @definitionid LIMIT 1"); // Drop definition
                dbClient.runQuery("DELETE FROM store_catalogue_sales WHERE item_definitionid = @definitionid"); // Drop single-item sales with this item definition
                dbClient.runQuery("DELETE FROM store_catalogue_sales_packages WHERE definitionid = @definitionid"); // Drop this item definition from packages
                dbClient.Close();
            }
        }

        private void txtSprite_TextChanged(object sender, EventArgs e)
        {
            /*int ID = int.Parse(txtID.Text);
            itemDefinition Resolver = new itemDefinition();
            Resolver.ID = int.Parse(txtID.Text);
            Resolver.Type = (itemDefinitionType)cmbItemType.SelectedIndex;
            Resolver.Sprite = txtSprite.Text;
            Resolver.Color = txtItemColor.Text;

            txtExtTextsName.Text = Resolver.Name; */
        }

        private void txtItemColor_TextChanged(object sender, EventArgs e)
        {
            this.txtSprite_TextChanged(null, null);
        }

        private void tsModifySearchDefinition_Click(object sender, EventArgs e)
        {
            this.displayItemDefinitions();
        }

        private void tsModifySearchDefinitionText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                this.displayItemDefinitions();
        }
    }
}
