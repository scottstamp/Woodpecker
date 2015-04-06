namespace Woodpecker.Tools
{
    partial class itemDefinitionEditorMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(itemDefinitionEditorMainForm));
            this.lblHeadline = new System.Windows.Forms.Label();
            this.boxDefinitions = new System.Windows.Forms.GroupBox();
            this.tsModify = new System.Windows.Forms.ToolStrip();
            this.tsModifyAddDefinition = new System.Windows.Forms.ToolStripButton();
            this.tsModifyRemoveDefinition = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsModifySearchDefinitionText = new System.Windows.Forms.ToolStripTextBox();
            this.tsModifySearchDefinition = new System.Windows.Forms.ToolStripButton();
            this.lvItemDefinitions = new System.Windows.Forms.ListView();
            this.colDefinitionID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSprite = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colColor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colExtTextsName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.boxEditor = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lstBehaviour = new System.Windows.Forms.CheckedListBox();
            this.cmdSaveItemDefinition = new System.Windows.Forms.Button();
            this.numItemTopHeight = new System.Windows.Forms.NumericUpDown();
            this.numItemWidth = new System.Windows.Forms.NumericUpDown();
            this.numItemLength = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.txtExtTextsName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtItemColor = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSprite = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDirectoryID = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtID = new System.Windows.Forms.TextBox();
            this.boxDefinitions.SuspendLayout();
            this.tsModify.SuspendLayout();
            this.boxEditor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numItemTopHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numItemWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numItemLength)).BeginInit();
            this.SuspendLayout();
            // 
            // lblHeadline
            // 
            this.lblHeadline.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadline.Location = new System.Drawing.Point(10, 9);
            this.lblHeadline.Name = "lblHeadline";
            this.lblHeadline.Size = new System.Drawing.Size(499, 36);
            this.lblHeadline.TabIndex = 3;
            this.lblHeadline.Text = "Item Definition Editor";
            this.lblHeadline.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // boxDefinitions
            // 
            this.boxDefinitions.Controls.Add(this.tsModify);
            this.boxDefinitions.Controls.Add(this.lvItemDefinitions);
            this.boxDefinitions.Location = new System.Drawing.Point(10, 48);
            this.boxDefinitions.Name = "boxDefinitions";
            this.boxDefinitions.Size = new System.Drawing.Size(502, 327);
            this.boxDefinitions.TabIndex = 34;
            this.boxDefinitions.TabStop = false;
            this.boxDefinitions.Text = "Definitions";
            // 
            // tsModify
            // 
            this.tsModify.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tsModify.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsModify.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsModifyAddDefinition,
            this.tsModifyRemoveDefinition,
            this.toolStripSeparator1,
            this.tsModifySearchDefinitionText,
            this.tsModifySearchDefinition});
            this.tsModify.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.tsModify.Location = new System.Drawing.Point(3, 295);
            this.tsModify.Name = "tsModify";
            this.tsModify.Size = new System.Drawing.Size(496, 29);
            this.tsModify.TabIndex = 35;
            // 
            // tsModifyAddDefinition
            // 
            this.tsModifyAddDefinition.Image = global::Woodpecker.Properties.Resources.shape_square_add;
            this.tsModifyAddDefinition.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsModifyAddDefinition.Name = "tsModifyAddDefinition";
            this.tsModifyAddDefinition.Size = new System.Drawing.Size(103, 26);
            this.tsModifyAddDefinition.Text = "Add definition";
            this.tsModifyAddDefinition.Click += new System.EventHandler(this.tsModifyAddDefinition_Click);
            // 
            // tsModifyRemoveDefinition
            // 
            this.tsModifyRemoveDefinition.Image = global::Woodpecker.Properties.Resources.shape_square_delete;
            this.tsModifyRemoveDefinition.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsModifyRemoveDefinition.Name = "tsModifyRemoveDefinition";
            this.tsModifyRemoveDefinition.Size = new System.Drawing.Size(124, 26);
            this.tsModifyRemoveDefinition.Text = "Remove definition";
            this.tsModifyRemoveDefinition.Click += new System.EventHandler(this.tsModifyRemoveDefinition_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 29);
            // 
            // tsModifySearchDefinitionText
            // 
            this.tsModifySearchDefinitionText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tsModifySearchDefinitionText.Name = "tsModifySearchDefinitionText";
            this.tsModifySearchDefinitionText.Size = new System.Drawing.Size(100, 29);
            this.tsModifySearchDefinitionText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tsModifySearchDefinitionText_KeyDown);
            // 
            // tsModifySearchDefinition
            // 
            this.tsModifySearchDefinition.Image = global::Woodpecker.Properties.Resources.page_white_magnify;
            this.tsModifySearchDefinition.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsModifySearchDefinition.Name = "tsModifySearchDefinition";
            this.tsModifySearchDefinition.Size = new System.Drawing.Size(116, 26);
            this.tsModifySearchDefinition.Text = "Search definition";
            this.tsModifySearchDefinition.Click += new System.EventHandler(this.tsModifySearchDefinition_Click);
            // 
            // lvItemDefinitions
            // 
            this.lvItemDefinitions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lvItemDefinitions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDefinitionID,
            this.colSprite,
            this.colColor,
            this.colExtTextsName});
            this.lvItemDefinitions.Dock = System.Windows.Forms.DockStyle.Top;
            this.lvItemDefinitions.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvItemDefinitions.FullRowSelect = true;
            this.lvItemDefinitions.GridLines = true;
            this.lvItemDefinitions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvItemDefinitions.HideSelection = false;
            this.lvItemDefinitions.Location = new System.Drawing.Point(3, 16);
            this.lvItemDefinitions.Name = "lvItemDefinitions";
            this.lvItemDefinitions.Size = new System.Drawing.Size(496, 279);
            this.lvItemDefinitions.TabIndex = 34;
            this.lvItemDefinitions.UseCompatibleStateImageBehavior = false;
            this.lvItemDefinitions.View = System.Windows.Forms.View.Details;
            this.lvItemDefinitions.SelectedIndexChanged += new System.EventHandler(this.lvItemDefinitions_SelectedIndexChanged);
            // 
            // colDefinitionID
            // 
            this.colDefinitionID.Text = "ID";
            this.colDefinitionID.Width = 65;
            // 
            // colSprite
            // 
            this.colSprite.Text = "Sprite";
            this.colSprite.Width = 112;
            // 
            // colColor
            // 
            this.colColor.Text = "Color";
            this.colColor.Width = 116;
            // 
            // colExtTextsName
            // 
            this.colExtTextsName.Text = "In-game name";
            this.colExtTextsName.Width = 157;
            // 
            // boxEditor
            // 
            this.boxEditor.Controls.Add(this.label3);
            this.boxEditor.Controls.Add(this.lstBehaviour);
            this.boxEditor.Controls.Add(this.cmdSaveItemDefinition);
            this.boxEditor.Controls.Add(this.numItemTopHeight);
            this.boxEditor.Controls.Add(this.numItemWidth);
            this.boxEditor.Controls.Add(this.numItemLength);
            this.boxEditor.Controls.Add(this.label9);
            this.boxEditor.Controls.Add(this.label8);
            this.boxEditor.Controls.Add(this.label7);
            this.boxEditor.Controls.Add(this.label10);
            this.boxEditor.Controls.Add(this.txtExtTextsName);
            this.boxEditor.Controls.Add(this.label5);
            this.boxEditor.Controls.Add(this.txtItemColor);
            this.boxEditor.Controls.Add(this.label4);
            this.boxEditor.Controls.Add(this.txtSprite);
            this.boxEditor.Controls.Add(this.label2);
            this.boxEditor.Controls.Add(this.txtDirectoryID);
            this.boxEditor.Controls.Add(this.label1);
            this.boxEditor.Controls.Add(this.txtID);
            this.boxEditor.Enabled = false;
            this.boxEditor.Location = new System.Drawing.Point(10, 381);
            this.boxEditor.Name = "boxEditor";
            this.boxEditor.Size = new System.Drawing.Size(503, 227);
            this.boxEditor.TabIndex = 35;
            this.boxEditor.TabStop = false;
            this.boxEditor.Text = "Editor";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(258, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 41;
            this.label3.Text = "Behaviour";
            // 
            // lstBehaviour
            // 
            this.lstBehaviour.CheckOnClick = true;
            this.lstBehaviour.FormattingEnabled = true;
            this.lstBehaviour.Location = new System.Drawing.Point(261, 74);
            this.lstBehaviour.Name = "lstBehaviour";
            this.lstBehaviour.Size = new System.Drawing.Size(236, 109);
            this.lstBehaviour.TabIndex = 40;
            // 
            // cmdSaveItemDefinition
            // 
            this.cmdSaveItemDefinition.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdSaveItemDefinition.Image = global::Woodpecker.Properties.Resources.accept;
            this.cmdSaveItemDefinition.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmdSaveItemDefinition.Location = new System.Drawing.Point(7, 191);
            this.cmdSaveItemDefinition.Name = "cmdSaveItemDefinition";
            this.cmdSaveItemDefinition.Size = new System.Drawing.Size(490, 30);
            this.cmdSaveItemDefinition.TabIndex = 39;
            this.cmdSaveItemDefinition.Text = "Save item definition";
            this.cmdSaveItemDefinition.UseVisualStyleBackColor = true;
            this.cmdSaveItemDefinition.Click += new System.EventHandler(this.cmdSaveItemDefinition_Click);
            // 
            // numItemTopHeight
            // 
            this.numItemTopHeight.BackColor = System.Drawing.SystemColors.Window;
            this.numItemTopHeight.DecimalPlaces = 2;
            this.numItemTopHeight.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numItemTopHeight.Location = new System.Drawing.Point(9, 163);
            this.numItemTopHeight.Name = "numItemTopHeight";
            this.numItemTopHeight.Size = new System.Drawing.Size(246, 20);
            this.numItemTopHeight.TabIndex = 37;
            // 
            // numItemWidth
            // 
            this.numItemWidth.BackColor = System.Drawing.SystemColors.Window;
            this.numItemWidth.Location = new System.Drawing.Point(122, 122);
            this.numItemWidth.Name = "numItemWidth";
            this.numItemWidth.ReadOnly = true;
            this.numItemWidth.Size = new System.Drawing.Size(133, 20);
            this.numItemWidth.TabIndex = 36;
            // 
            // numItemLength
            // 
            this.numItemLength.BackColor = System.Drawing.SystemColors.Window;
            this.numItemLength.Location = new System.Drawing.Point(9, 122);
            this.numItemLength.Name = "numItemLength";
            this.numItemLength.ReadOnly = true;
            this.numItemLength.Size = new System.Drawing.Size(98, 20);
            this.numItemLength.TabIndex = 35;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 147);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(59, 13);
            this.label9.TabIndex = 34;
            this.label9.Text = "Item height";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(119, 106);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(35, 13);
            this.label8.TabIndex = 33;
            this.label8.Text = "Width";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 106);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 13);
            this.label7.TabIndex = 32;
            this.label7.Text = "Length";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(258, 16);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(74, 13);
            this.label10.TabIndex = 23;
            this.label10.Text = "In-game name";
            // 
            // txtExtTextsName
            // 
            this.txtExtTextsName.Enabled = false;
            this.txtExtTextsName.Location = new System.Drawing.Point(261, 32);
            this.txtExtTextsName.Name = "txtExtTextsName";
            this.txtExtTextsName.Size = new System.Drawing.Size(236, 20);
            this.txtExtTextsName.TabIndex = 22;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(121, 58);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Item color";
            // 
            // txtItemColor
            // 
            this.txtItemColor.Location = new System.Drawing.Point(122, 74);
            this.txtItemColor.Name = "txtItemColor";
            this.txtItemColor.Size = new System.Drawing.Size(133, 20);
            this.txtItemColor.TabIndex = 9;
            this.txtItemColor.TextChanged += new System.EventHandler(this.txtItemColor_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 58);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Item sprite";
            // 
            // txtSprite
            // 
            this.txtSprite.Location = new System.Drawing.Point(9, 74);
            this.txtSprite.Name = "txtSprite";
            this.txtSprite.Size = new System.Drawing.Size(98, 20);
            this.txtSprite.TabIndex = 7;
            this.txtSprite.TextChanged += new System.EventHandler(this.txtSprite_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(119, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Castfile directory ID";
            // 
            // txtDirectoryID
            // 
            this.txtDirectoryID.Location = new System.Drawing.Point(122, 32);
            this.txtDirectoryID.Name = "txtDirectoryID";
            this.txtDirectoryID.Size = new System.Drawing.Size(133, 20);
            this.txtDirectoryID.TabIndex = 2;
            this.txtDirectoryID.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Definition ID";
            // 
            // txtID
            // 
            this.txtID.Enabled = false;
            this.txtID.Location = new System.Drawing.Point(9, 32);
            this.txtID.Name = "txtID";
            this.txtID.Size = new System.Drawing.Size(100, 20);
            this.txtID.TabIndex = 0;
            this.txtID.Text = "0";
            // 
            // itemDefinitionEditorMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 620);
            this.Controls.Add(this.boxEditor);
            this.Controls.Add(this.boxDefinitions);
            this.Controls.Add(this.lblHeadline);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "itemDefinitionEditorMainForm";
            this.Text = "Woodpecker : item definition editor";
            this.Load += new System.EventHandler(this.itemDefinitionEditorMainForm_Load);
            this.boxDefinitions.ResumeLayout(false);
            this.boxDefinitions.PerformLayout();
            this.tsModify.ResumeLayout(false);
            this.tsModify.PerformLayout();
            this.boxEditor.ResumeLayout(false);
            this.boxEditor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numItemTopHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numItemWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numItemLength)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblHeadline;
        private System.Windows.Forms.GroupBox boxDefinitions;
        internal System.Windows.Forms.ListView lvItemDefinitions;
        private System.Windows.Forms.ColumnHeader colDefinitionID;
        private System.Windows.Forms.ColumnHeader colSprite;
        private System.Windows.Forms.ColumnHeader colColor;
        private System.Windows.Forms.GroupBox boxEditor;
        private System.Windows.Forms.ToolStrip tsModify;
        private System.Windows.Forms.ToolStripButton tsModifyAddDefinition;
        private System.Windows.Forms.ToolStripButton tsModifyRemoveDefinition;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsModifySearchDefinition;
        private System.Windows.Forms.ToolStripTextBox tsModifySearchDefinitionText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDirectoryID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtID;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtItemColor;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSprite;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtExtTextsName;
        private System.Windows.Forms.ColumnHeader colExtTextsName;
        private System.Windows.Forms.NumericUpDown numItemTopHeight;
        private System.Windows.Forms.NumericUpDown numItemWidth;
        private System.Windows.Forms.NumericUpDown numItemLength;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button cmdSaveItemDefinition;
        private System.Windows.Forms.CheckedListBox lstBehaviour;
        private System.Windows.Forms.Label label3;
    }
}