namespace Woodpecker
{
    partial class mainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(mainForm));
            this.stripMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shutdownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.serverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sessionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.securityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectionBlacklistEntriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearConnectionBlacklistToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blacklistUnknownPacketSendersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.usersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RoomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ItemsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loggingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.writeLogsToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logCommonWarningsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logCommonErrorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logDebugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sessionsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.logSessionCreatedestroyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logConnectionBlacklistEventsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logHaxEventsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logClientserverMessagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logServerclientMessagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reactorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logReactorErrorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logTargetMethodNotFoundErrorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.usersToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.logLogonlogoffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logModerationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.roomsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.logInstanceCreatedestroyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logUserEnterleaveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.broadcoastMessageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txtBroadcoastMessage = new System.Windows.Forms.ToolStripTextBox();
            this.cmdBroadcoastMessage = new System.Windows.Forms.ToolStripMenuItem();
            this.broadcoastVoiceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txtVoiceText = new System.Windows.Forms.ToolStripTextBox();
            this.cmdSendVoice = new System.Windows.Forms.ToolStripMenuItem();
            this.itemDefinitionEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.registerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stripStatus = new System.Windows.Forms.StatusStrip();
            this.lblStatusHeader = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.publicRoomItemsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stripMenu.SuspendLayout();
            this.stripStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // stripMenu
            // 
            this.stripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.serverToolStripMenuItem,
            this.loggingToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.stripMenu.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.stripMenu.Location = new System.Drawing.Point(0, 0);
            this.stripMenu.Name = "stripMenu";
            this.stripMenu.Size = new System.Drawing.Size(420, 24);
            this.stripMenu.TabIndex = 2;
            this.stripMenu.Text = "mnuMain";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.shutdownToolStripMenuItem});
            this.fileToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.application;
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // shutdownToolStripMenuItem
            // 
            this.shutdownToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.cancel;
            this.shutdownToolStripMenuItem.Name = "shutdownToolStripMenuItem";
            this.shutdownToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.shutdownToolStripMenuItem.Text = "Stop";
            this.shutdownToolStripMenuItem.Click += new System.EventHandler(this.shutdownToolStripMenuItem_Click);
            // 
            // serverToolStripMenuItem
            // 
            this.serverToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sessionsToolStripMenuItem,
            this.securityToolStripMenuItem,
            this.usersToolStripMenuItem,
            this.RoomsToolStripMenuItem,
            this.ItemsToolStripMenuItem});
            this.serverToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.server;
            this.serverToolStripMenuItem.Name = "serverToolStripMenuItem";
            this.serverToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.serverToolStripMenuItem.Text = "Server";
            // 
            // sessionsToolStripMenuItem
            // 
            this.sessionsToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.server_lightning;
            this.sessionsToolStripMenuItem.Name = "sessionsToolStripMenuItem";
            this.sessionsToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.sessionsToolStripMenuItem.Text = "Sessions";
            this.sessionsToolStripMenuItem.Click += new System.EventHandler(this.sessionsToolStripMenuItem_Click);
            // 
            // securityToolStripMenuItem
            // 
            this.securityToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectionBlacklistEntriesToolStripMenuItem,
            this.clearConnectionBlacklistToolStripMenuItem,
            this.blacklistUnknownPacketSendersToolStripMenuItem});
            this.securityToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.bullet_key;
            this.securityToolStripMenuItem.Name = "securityToolStripMenuItem";
            this.securityToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.securityToolStripMenuItem.Text = "Security";
            // 
            // connectionBlacklistEntriesToolStripMenuItem
            // 
            this.connectionBlacklistEntriesToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.book_open;
            this.connectionBlacklistEntriesToolStripMenuItem.Name = "connectionBlacklistEntriesToolStripMenuItem";
            this.connectionBlacklistEntriesToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.connectionBlacklistEntriesToolStripMenuItem.Text = "Connection blacklist entries";
            // 
            // clearConnectionBlacklistToolStripMenuItem
            // 
            this.clearConnectionBlacklistToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.bomb;
            this.clearConnectionBlacklistToolStripMenuItem.Name = "clearConnectionBlacklistToolStripMenuItem";
            this.clearConnectionBlacklistToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.clearConnectionBlacklistToolStripMenuItem.Text = "Clear connection blacklist";
            this.clearConnectionBlacklistToolStripMenuItem.Click += new System.EventHandler(this.clearConnectionBlacklistToolStripMenuItem_Click);
            // 
            // blacklistUnknownPacketSendersToolStripMenuItem
            // 
            this.blacklistUnknownPacketSendersToolStripMenuItem.Checked = true;
            this.blacklistUnknownPacketSendersToolStripMenuItem.CheckOnClick = true;
            this.blacklistUnknownPacketSendersToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.blacklistUnknownPacketSendersToolStripMenuItem.Name = "blacklistUnknownPacketSendersToolStripMenuItem";
            this.blacklistUnknownPacketSendersToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.blacklistUnknownPacketSendersToolStripMenuItem.Text = "Blacklist unknown packet senders";
            this.blacklistUnknownPacketSendersToolStripMenuItem.Click += new System.EventHandler(this.blacklistUnknownPacketSendersToolStripMenuItem_Click);
            // 
            // usersToolStripMenuItem
            // 
            this.usersToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.group;
            this.usersToolStripMenuItem.Name = "usersToolStripMenuItem";
            this.usersToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.usersToolStripMenuItem.Text = "Users";
            this.usersToolStripMenuItem.Click += new System.EventHandler(this.usersToolStripMenuItem_Click);
            // 
            // RoomsToolStripMenuItem
            // 
            this.RoomsToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.house;
            this.RoomsToolStripMenuItem.Name = "RoomsToolStripMenuItem";
            this.RoomsToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.RoomsToolStripMenuItem.Text = "Rooms";
            // 
            // ItemsToolStripMenuItem
            // 
            this.ItemsToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.bricks;
            this.ItemsToolStripMenuItem.Name = "ItemsToolStripMenuItem";
            this.ItemsToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.ItemsToolStripMenuItem.Text = "Furniture";
            // 
            // loggingToolStripMenuItem
            // 
            this.loggingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.writeLogsToFileToolStripMenuItem,
            this.logCommonWarningsToolStripMenuItem,
            this.logCommonErrorsToolStripMenuItem,
            this.logDebugToolStripMenuItem,
            this.sessionsToolStripMenuItem1,
            this.usersToolStripMenuItem1,
            this.roomsToolStripMenuItem1,
            this.clearLogToolStripMenuItem});
            this.loggingToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.script;
            this.loggingToolStripMenuItem.Name = "loggingToolStripMenuItem";
            this.loggingToolStripMenuItem.Size = new System.Drawing.Size(79, 20);
            this.loggingToolStripMenuItem.Text = "Logging";
            // 
            // writeLogsToFileToolStripMenuItem
            // 
            this.writeLogsToFileToolStripMenuItem.Checked = true;
            this.writeLogsToFileToolStripMenuItem.CheckOnClick = true;
            this.writeLogsToFileToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.writeLogsToFileToolStripMenuItem.Name = "writeLogsToFileToolStripMenuItem";
            this.writeLogsToFileToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.writeLogsToFileToolStripMenuItem.Text = "Save logs to file";
            // 
            // logCommonWarningsToolStripMenuItem
            // 
            this.logCommonWarningsToolStripMenuItem.Checked = true;
            this.logCommonWarningsToolStripMenuItem.CheckOnClick = true;
            this.logCommonWarningsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.logCommonWarningsToolStripMenuItem.Name = "logCommonWarningsToolStripMenuItem";
            this.logCommonWarningsToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.logCommonWarningsToolStripMenuItem.Text = "Log warnings";
            this.logCommonWarningsToolStripMenuItem.Click += new System.EventHandler(this.logCommonWarningsToolStripMenuItem_Click);
            // 
            // logCommonErrorsToolStripMenuItem
            // 
            this.logCommonErrorsToolStripMenuItem.Checked = true;
            this.logCommonErrorsToolStripMenuItem.CheckOnClick = true;
            this.logCommonErrorsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.logCommonErrorsToolStripMenuItem.Name = "logCommonErrorsToolStripMenuItem";
            this.logCommonErrorsToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.logCommonErrorsToolStripMenuItem.Text = "Log errors";
            this.logCommonErrorsToolStripMenuItem.Click += new System.EventHandler(this.logCommonErrorsToolStripMenuItem_Click);
            // 
            // logDebugToolStripMenuItem
            // 
            this.logDebugToolStripMenuItem.CheckOnClick = true;
            this.logDebugToolStripMenuItem.Name = "logDebugToolStripMenuItem";
            this.logDebugToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.logDebugToolStripMenuItem.Text = "Log debug";
            this.logDebugToolStripMenuItem.Click += new System.EventHandler(this.logDebugToolStripMenuItem_Click);
            // 
            // sessionsToolStripMenuItem1
            // 
            this.sessionsToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logSessionCreatedestroyToolStripMenuItem,
            this.logConnectionBlacklistEventsToolStripMenuItem,
            this.logHaxEventsToolStripMenuItem,
            this.logClientserverMessagesToolStripMenuItem,
            this.logServerclientMessagesToolStripMenuItem,
            this.reactorsToolStripMenuItem});
            this.sessionsToolStripMenuItem1.Image = global::Woodpecker.Properties.Resources.user_gray;
            this.sessionsToolStripMenuItem1.Name = "sessionsToolStripMenuItem1";
            this.sessionsToolStripMenuItem1.Size = new System.Drawing.Size(156, 22);
            this.sessionsToolStripMenuItem1.Text = "Sessions";
            // 
            // logSessionCreatedestroyToolStripMenuItem
            // 
            this.logSessionCreatedestroyToolStripMenuItem.Checked = true;
            this.logSessionCreatedestroyToolStripMenuItem.CheckOnClick = true;
            this.logSessionCreatedestroyToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.logSessionCreatedestroyToolStripMenuItem.Name = "logSessionCreatedestroyToolStripMenuItem";
            this.logSessionCreatedestroyToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.logSessionCreatedestroyToolStripMenuItem.Text = "Log session create/destroy";
            this.logSessionCreatedestroyToolStripMenuItem.Click += new System.EventHandler(this.logSessionCreatedestroyToolStripMenuItem_Click);
            // 
            // logConnectionBlacklistEventsToolStripMenuItem
            // 
            this.logConnectionBlacklistEventsToolStripMenuItem.Checked = true;
            this.logConnectionBlacklistEventsToolStripMenuItem.CheckOnClick = true;
            this.logConnectionBlacklistEventsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.logConnectionBlacklistEventsToolStripMenuItem.Name = "logConnectionBlacklistEventsToolStripMenuItem";
            this.logConnectionBlacklistEventsToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.logConnectionBlacklistEventsToolStripMenuItem.Text = "Log connection blacklist events";
            this.logConnectionBlacklistEventsToolStripMenuItem.Click += new System.EventHandler(this.logConnectionBlacklistEventsToolStripMenuItem_Click);
            // 
            // logHaxEventsToolStripMenuItem
            // 
            this.logHaxEventsToolStripMenuItem.Checked = true;
            this.logHaxEventsToolStripMenuItem.CheckOnClick = true;
            this.logHaxEventsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.logHaxEventsToolStripMenuItem.Name = "logHaxEventsToolStripMenuItem";
            this.logHaxEventsToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.logHaxEventsToolStripMenuItem.Text = "Log hax events";
            this.logHaxEventsToolStripMenuItem.Click += new System.EventHandler(this.logHaxEventsToolStripMenuItem_Click);
            // 
            // logClientserverMessagesToolStripMenuItem
            // 
            this.logClientserverMessagesToolStripMenuItem.CheckOnClick = true;
            this.logClientserverMessagesToolStripMenuItem.Name = "logClientserverMessagesToolStripMenuItem";
            this.logClientserverMessagesToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.logClientserverMessagesToolStripMenuItem.Text = "Log client>server messages";
            this.logClientserverMessagesToolStripMenuItem.Click += new System.EventHandler(this.logClientserverMessagesToolStripMenuItem_Click);
            // 
            // logServerclientMessagesToolStripMenuItem
            // 
            this.logServerclientMessagesToolStripMenuItem.CheckOnClick = true;
            this.logServerclientMessagesToolStripMenuItem.Name = "logServerclientMessagesToolStripMenuItem";
            this.logServerclientMessagesToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.logServerclientMessagesToolStripMenuItem.Text = "Log server>client messages";
            this.logServerclientMessagesToolStripMenuItem.Click += new System.EventHandler(this.logServerclientMessagesToolStripMenuItem_Click);
            // 
            // reactorsToolStripMenuItem
            // 
            this.reactorsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logReactorErrorsToolStripMenuItem,
            this.logTargetMethodNotFoundErrorsToolStripMenuItem});
            this.reactorsToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.bell;
            this.reactorsToolStripMenuItem.Name = "reactorsToolStripMenuItem";
            this.reactorsToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.reactorsToolStripMenuItem.Text = "Reactors";
            // 
            // logReactorErrorsToolStripMenuItem
            // 
            this.logReactorErrorsToolStripMenuItem.CheckOnClick = true;
            this.logReactorErrorsToolStripMenuItem.Name = "logReactorErrorsToolStripMenuItem";
            this.logReactorErrorsToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.logReactorErrorsToolStripMenuItem.Text = "Log reactor errors";
            this.logReactorErrorsToolStripMenuItem.Click += new System.EventHandler(this.logReactorErrorsToolStripMenuItem_Click);
            // 
            // logTargetMethodNotFoundErrorsToolStripMenuItem
            // 
            this.logTargetMethodNotFoundErrorsToolStripMenuItem.CheckOnClick = true;
            this.logTargetMethodNotFoundErrorsToolStripMenuItem.Name = "logTargetMethodNotFoundErrorsToolStripMenuItem";
            this.logTargetMethodNotFoundErrorsToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.logTargetMethodNotFoundErrorsToolStripMenuItem.Text = "Log \'target method not found\' errors";
            this.logTargetMethodNotFoundErrorsToolStripMenuItem.Click += new System.EventHandler(this.loglistenerNotFoundErrorsToolStripMenuItem_Click);
            // 
            // usersToolStripMenuItem1
            // 
            this.usersToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logLogonlogoffToolStripMenuItem,
            this.logModerationToolStripMenuItem});
            this.usersToolStripMenuItem1.Image = global::Woodpecker.Properties.Resources.group1;
            this.usersToolStripMenuItem1.Name = "usersToolStripMenuItem1";
            this.usersToolStripMenuItem1.Size = new System.Drawing.Size(156, 22);
            this.usersToolStripMenuItem1.Text = "Users";
            // 
            // logLogonlogoffToolStripMenuItem
            // 
            this.logLogonlogoffToolStripMenuItem.Checked = true;
            this.logLogonlogoffToolStripMenuItem.CheckOnClick = true;
            this.logLogonlogoffToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.logLogonlogoffToolStripMenuItem.Name = "logLogonlogoffToolStripMenuItem";
            this.logLogonlogoffToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.logLogonlogoffToolStripMenuItem.Text = "Log logon/logoff";
            this.logLogonlogoffToolStripMenuItem.Click += new System.EventHandler(this.logLogonlogoffToolStripMenuItem_Click);
            // 
            // logModerationToolStripMenuItem
            // 
            this.logModerationToolStripMenuItem.Checked = true;
            this.logModerationToolStripMenuItem.CheckOnClick = true;
            this.logModerationToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.logModerationToolStripMenuItem.Name = "logModerationToolStripMenuItem";
            this.logModerationToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.logModerationToolStripMenuItem.Text = "Log moderation";
            this.logModerationToolStripMenuItem.Click += new System.EventHandler(this.logModerationToolStripMenuItem_Click);
            // 
            // roomsToolStripMenuItem1
            // 
            this.roomsToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logInstanceCreatedestroyToolStripMenuItem,
            this.logUserEnterleaveToolStripMenuItem});
            this.roomsToolStripMenuItem1.Image = global::Woodpecker.Properties.Resources.building;
            this.roomsToolStripMenuItem1.Name = "roomsToolStripMenuItem1";
            this.roomsToolStripMenuItem1.Size = new System.Drawing.Size(156, 22);
            this.roomsToolStripMenuItem1.Text = "Rooms";
            // 
            // logInstanceCreatedestroyToolStripMenuItem
            // 
            this.logInstanceCreatedestroyToolStripMenuItem.Checked = true;
            this.logInstanceCreatedestroyToolStripMenuItem.CheckOnClick = true;
            this.logInstanceCreatedestroyToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.logInstanceCreatedestroyToolStripMenuItem.Name = "logInstanceCreatedestroyToolStripMenuItem";
            this.logInstanceCreatedestroyToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.logInstanceCreatedestroyToolStripMenuItem.Text = "Log instance create/destroy";
            this.logInstanceCreatedestroyToolStripMenuItem.Click += new System.EventHandler(this.logInstanceCreatedestroyToolStripMenuItem_Click);
            // 
            // logUserEnterleaveToolStripMenuItem
            // 
            this.logUserEnterleaveToolStripMenuItem.CheckOnClick = true;
            this.logUserEnterleaveToolStripMenuItem.Name = "logUserEnterleaveToolStripMenuItem";
            this.logUserEnterleaveToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.logUserEnterleaveToolStripMenuItem.Text = "Log user enter/leave";
            this.logUserEnterleaveToolStripMenuItem.Click += new System.EventHandler(this.logUserEnterleaveToolStripMenuItem_Click);
            // 
            // clearLogToolStripMenuItem
            // 
            this.clearLogToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.bomb;
            this.clearLogToolStripMenuItem.Name = "clearLogToolStripMenuItem";
            this.clearLogToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.clearLogToolStripMenuItem.Text = "Clear the logs";
            this.clearLogToolStripMenuItem.Click += new System.EventHandler(this.clearLogToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.broadcoastMessageToolStripMenuItem,
            this.broadcoastVoiceToolStripMenuItem,
            this.itemDefinitionEditorToolStripMenuItem,
            this.publicRoomItemsToolStripMenuItem});
            this.toolsToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.wand;
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(64, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // broadcoastMessageToolStripMenuItem
            // 
            this.broadcoastMessageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.txtBroadcoastMessage,
            this.cmdBroadcoastMessage});
            this.broadcoastMessageToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.transmit;
            this.broadcoastMessageToolStripMenuItem.Name = "broadcoastMessageToolStripMenuItem";
            this.broadcoastMessageToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.broadcoastMessageToolStripMenuItem.Text = "Broadcoast message";
            // 
            // txtBroadcoastMessage
            // 
            this.txtBroadcoastMessage.AutoSize = false;
            this.txtBroadcoastMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBroadcoastMessage.Name = "txtBroadcoastMessage";
            this.txtBroadcoastMessage.Size = new System.Drawing.Size(400, 23);
            this.txtBroadcoastMessage.Text = "This is a hotel message from the server GUI.";
            // 
            // cmdBroadcoastMessage
            // 
            this.cmdBroadcoastMessage.Image = global::Woodpecker.Properties.Resources.transmit_go;
            this.cmdBroadcoastMessage.Name = "cmdBroadcoastMessage";
            this.cmdBroadcoastMessage.Size = new System.Drawing.Size(460, 22);
            this.cmdBroadcoastMessage.Text = "Broadcoast message";
            this.cmdBroadcoastMessage.ToolTipText = "Click here to broadcoast the message above to all online users";
            this.cmdBroadcoastMessage.Click += new System.EventHandler(this.cmdBroadcoastMessage_Click);
            // 
            // broadcoastVoiceToolStripMenuItem
            // 
            this.broadcoastVoiceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.txtVoiceText,
            this.cmdSendVoice});
            this.broadcoastVoiceToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.transmit;
            this.broadcoastVoiceToolStripMenuItem.Name = "broadcoastVoiceToolStripMenuItem";
            this.broadcoastVoiceToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.broadcoastVoiceToolStripMenuItem.Text = "Broadcoast voice";
            // 
            // txtVoiceText
            // 
            this.txtVoiceText.AutoSize = false;
            this.txtVoiceText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtVoiceText.Name = "txtVoiceText";
            this.txtVoiceText.Size = new System.Drawing.Size(400, 23);
            this.txtVoiceText.Text = "This Hotel is running the Woodpecker server";
            // 
            // cmdSendVoice
            // 
            this.cmdSendVoice.Image = global::Woodpecker.Properties.Resources.transmit_go;
            this.cmdSendVoice.Name = "cmdSendVoice";
            this.cmdSendVoice.Size = new System.Drawing.Size(460, 22);
            this.cmdSendVoice.Text = "Broadcoast voice";
            this.cmdSendVoice.ToolTipText = "Click here to broadcoast the text above as a voice message to all online users";
            this.cmdSendVoice.Click += new System.EventHandler(this.cmdSendVoice_Click);
            // 
            // itemDefinitionEditorToolStripMenuItem
            // 
            this.itemDefinitionEditorToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.bricks;
            this.itemDefinitionEditorToolStripMenuItem.Name = "itemDefinitionEditorToolStripMenuItem";
            this.itemDefinitionEditorToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.itemDefinitionEditorToolStripMenuItem.Text = "Item Definition Editor";
            this.itemDefinitionEditorToolStripMenuItem.Click += new System.EventHandler(this.itemDefinitionEditorToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem,
            this.registerToolStripMenuItem});
            this.helpToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.help;
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Image = global::Woodpecker.Properties.Resources.vcard;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.aboutToolStripMenuItem.Text = "Informatie";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // registerToolStripMenuItem
            // 
            this.registerToolStripMenuItem.Enabled = false;
            this.registerToolStripMenuItem.Name = "registerToolStripMenuItem";
            this.registerToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.registerToolStripMenuItem.Text = "Register";
            // 
            // stripStatus
            // 
            this.stripStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatusHeader,
            this.lblStatus});
            this.stripStatus.Location = new System.Drawing.Point(0, 302);
            this.stripStatus.Name = "stripStatus";
            this.stripStatus.Size = new System.Drawing.Size(420, 22);
            this.stripStatus.SizingGrip = false;
            this.stripStatus.TabIndex = 4;
            // 
            // lblStatusHeader
            // 
            this.lblStatusHeader.Name = "lblStatusHeader";
            this.lblStatusHeader.Size = new System.Drawing.Size(42, 17);
            this.lblStatusHeader.Text = "Status:";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(67, 17);
            this.lblStatus.Text = "Not Started";
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(0, 24);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(420, 278);
            this.txtLog.TabIndex = 5;
            // 
            // publicRoomItemsToolStripMenuItem
            // 
            this.publicRoomItemsToolStripMenuItem.Name = "publicRoomItemsToolStripMenuItem";
            this.publicRoomItemsToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.publicRoomItemsToolStripMenuItem.Text = "Public Room Items";
            this.publicRoomItemsToolStripMenuItem.Click += new System.EventHandler(this.publicRoomItemsToolStripMenuItem_Click);
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 324);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.stripStatus);
            this.Controls.Add(this.stripMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "mainForm";
            this.Text = "Woodpecker";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Closing += new System.ComponentModel.CancelEventHandler(this.mainForm_Closing);
            this.Load += new System.EventHandler(this.mainForm_Load);
            this.stripMenu.ResumeLayout(false);
            this.stripMenu.PerformLayout();
            this.stripStatus.ResumeLayout(false);
            this.stripStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shutdownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem serverToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sessionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem usersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ItemsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loggingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem writeLogsToFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem broadcoastMessageToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox txtBroadcoastMessage;
        private System.Windows.Forms.ToolStripMenuItem cmdBroadcoastMessage;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem registerToolStripMenuItem;
        private System.Windows.Forms.StatusStrip stripStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusHeader;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        public System.Windows.Forms.MenuStrip stripMenu;
        private System.Windows.Forms.ToolStripMenuItem securityToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearConnectionBlacklistToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectionBlacklistEntriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blacklistUnknownPacketSendersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logDebugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sessionsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem logSessionCreatedestroyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logClientserverMessagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logServerclientMessagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem usersToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem logLogonlogoffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logModerationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem roomsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem logInstanceCreatedestroyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logUserEnterleaveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logCommonWarningsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logCommonErrorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reactorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logReactorErrorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logTargetMethodNotFoundErrorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logHaxEventsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logConnectionBlacklistEventsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem itemDefinitionEditorToolStripMenuItem;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.ToolStripMenuItem broadcoastVoiceToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox txtVoiceText;
        private System.Windows.Forms.ToolStripMenuItem cmdSendVoice;
        private System.Windows.Forms.ToolStripMenuItem publicRoomItemsToolStripMenuItem;
    }
}

