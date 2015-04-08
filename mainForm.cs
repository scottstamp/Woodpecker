using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

using Woodpecker.Core;
using Woodpecker.Storage;
using Woodpecker.Tools;
using Woodpecker.Specialized.Fun;

namespace Woodpecker
{
    public partial class mainForm : Form
    {
        #region Fields
        public bool mShutdown = false;
        #region Delegates
        private delegate void textLogger(string Text, Logging.logType Type);
        private delegate void sacredLogger(string Text);
        private delegate void statusLabelPntr(string Caption);
        private static statusLabelPntr _statusLabelPointer;
        private static textLogger _Logger;
        private static sacredLogger _sacredLogger;
        #endregion
        private Thread statusUpdater;
        #endregion

        public mainForm()
        {
            InitializeComponent();
        }
        private void mainForm_Load(object sender, EventArgs e)
        {
            this.Show();
            this.stripMenu.Enabled = false;
            _Logger = new textLogger(logText);
            _sacredLogger = new sacredLogger(logSacredText);
            _statusLabelPointer = new statusLabelPntr(setStatusLabel);

            Engine.Program.GUI = this;
            Engine.Program.Start();

            statusUpdater = new Thread(this.updateStatusCycle);
            statusUpdater.Priority = ThreadPriority.Lowest;
            statusUpdater.Start();
        }
        #region Logging
        public void logSacredText(string Text)
        {
            try
            {
                if (this.InvokeRequired)
                    this.BeginInvoke(_sacredLogger, Text);
                else
                {
                    //lock (this.txtLog)
                    {
                        if (Text != null)
                        {
                            this.txtLog.AppendText("# ");
                            this.txtLog.AppendText(Text);
                        }
                        this.txtLog.AppendText(Environment.NewLine);
                        this.txtLog.SelectionStart = this.txtLog.Text.Length;
                        this.txtLog.ScrollToCaret();
                        this.txtLog.Refresh();
                    }
                }
            }
            catch (StackOverflowException) { }
        }
        /// <summary>
        /// Logs text to the textbox on the main form.
        /// </summary>
        /// <param name="Text">The text to log.</param>
        /// <param name="Type">A Logging.logType enum value, indicating the log's type.</param>
        public void logText(string Text, Logging.logType Type)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(_Logger, Text, Type);
            else
            {
                try
                {
                    lock (this.txtLog)
                    {
                        switch (Type)
                        {
                            case Logging.logType.commonWarning:
                                {
                                    this.txtLog.AppendText("Warning!" + Environment.NewLine);
                                    this.txtLog.AppendText("Time: " + DateTime.Now.ToShortTimeString() + Environment.NewLine);
                                    this.txtLog.AppendText("Description: " + Text + Environment.NewLine + Environment.NewLine);
                                }
                                break;

                            case Logging.logType.reactorError:
                            case Logging.logType.commonError:
                                {
                                    StackFrame st = new StackTrace().GetFrame(2);

                                    this.txtLog.AppendText("ERROR!" + Environment.NewLine);
                                    this.txtLog.AppendText("Time: " + DateTime.Now.ToShortTimeString() + Environment.NewLine);
                                    this.txtLog.AppendText("Object: " + st.GetMethod().ReflectedType.Name + Environment.NewLine);
                                    this.txtLog.AppendText("Method: " + st.GetMethod().Name + Environment.NewLine);
                                    this.txtLog.AppendText("Description: " + Text + Environment.NewLine + Environment.NewLine);
                                }
                                break;

                            default:
                                {
                                    this.txtLog.AppendText("# ");
                                    this.txtLog.AppendText(Text);
                                    this.txtLog.AppendText(Environment.NewLine);
                                }
                                break;
                        }
                    }
                }
                catch (Exception) { }
            }
        }
        private void setStatusLabel(string Caption)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(_statusLabelPointer, Caption);
            else
            {
                this.lblStatus.Text = Caption;
            }
        }
        #endregion

        private void updateStatusCycle()
        {
            while (true)
            {
                int sessionCount = Engine.Sessions.sessionCount;
                int userCount = Engine.Game.Users.userCount;
                int roomCount = Engine.Game.Rooms.roomCount;

                long memKB = GC.GetTotalMemory(false) / 1024;
                setStatusLabel(sessionCount + " session(s) running, " + userCount + " user(s) logged in, " + roomCount + " room instance(s) active. Memory usage: " + memKB + "KB");
                Thread.Sleep(30000); // 30 seconds
            }
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show
                (
                "Woodpecker\n" + 
                "Habbo Hotel V13 emulator\n" +
                "Platform: C#.NET 3.5\n" +
                "Written by Nils W.\n" +
                "Copyright (C) 2008\n\n" + 
                "",
                "About Woodpecker",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
                );
        }

        
        private void mainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
            if (mShutdown)
                Environment.Exit(-1);
            else if (MessageBox.Show("Are you sure you wish to shutdown Woodpecker?\nAll active information will be saved before closure.", "Server Shutdown", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                Engine.Program.Stop(true, "none");
            }
            else
            { 
                if (this.stripMenu.Enabled)
                    Logging.Log("Server > Shutdown aborted.");
                e.Cancel = true;
            }
        }

        private void shutdownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you wish to shutdown Woodpecker?\nAll active information will be saved before closure.", "Server Shutdown", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                Engine.Program.Stop("none");
        }

        private void blacklistUnknownPacketSendersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Configuration.blacklistBastards = !Configuration.blacklistBastards;
        }

        private void clearLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.txtLog.Clear();
            this.logSacredText("Log cleared.");
        }

        private void sessionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Engine.Sessions.listSessions();
        }

        private void usersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Engine.Game.Users.listUserStats();
        }

        private void clearConnectionBlacklistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Engine.Net.Game.clearBlacklist();
            Logging.Log("Connection blacklist cleared.");
        }

        private void logCommonWarningsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logging.toggleLogStatus(Logging.logType.commonWarning);
        }

        private void logCommonErrorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logging.toggleLogStatus(Logging.logType.commonError);
        }

        private void logDebugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logging.toggleLogStatus(Logging.logType.debugEvent);
        }

        private void logSessionCreatedestroyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logging.toggleLogStatus(Logging.logType.sessionConnectionEvent);
        }

        private void logClientserverMessagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logging.toggleLogStatus(Logging.logType.clientMessageEvent);
        }

        private void logServerclientMessagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logging.toggleLogStatus(Logging.logType.serverMessageEvent);
        }

        private void logReactorErrorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logging.toggleLogStatus(Logging.logType.reactorError);
        }

        private void loglistenerNotFoundErrorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logging.toggleLogStatus(Logging.logType.targetMethodNotFoundEvent);
        }

        private void logLogonlogoffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logging.toggleLogStatus(Logging.logType.userVisitEvent);
        }

        private void logModerationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logging.toggleLogStatus(Logging.logType.moderationEvent);
        }

        private void logChatlogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Engine.Game.Moderation.logChat = !Engine.Game.Moderation.logChat;
        }

        private void logInstanceCreatedestroyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logging.toggleLogStatus(Logging.logType.roomInstanceEvent);
        }

        private void logUserEnterleaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logging.toggleLogStatus(Logging.logType.roomUserRegisterEvent);
        }

        private void logHaxEventsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logging.toggleLogStatus(Logging.logType.haxEvent);
        }

        private void logConnectionBlacklistEventsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logging.toggleLogStatus(Logging.logType.connectionBlacklistEvent);
        }

        private void cmdBroadcoastMessage_Click(object sender, EventArgs e)
        {
            string Text = this.txtBroadcoastMessage.Text;
            Woodpecker.Specialized.Text.stringFunctions.filterVulnerableStuff(ref Text, true);
            Text = Text.Replace(Convert.ToChar(1), ' ');
            Text = Text.Trim();
            if (Text.Length > 0)
            {
                Engine.Game.Users.broadcastHotelAlert(Text);
            }
        }
        private void itemDefinitionEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            itemDefinitionEditorMainForm Editor = new itemDefinitionEditorMainForm();
            Editor.Show();
        }

        private void cmdSendVoice_Click(object sender, EventArgs e)
        {
            string Text = this.txtVoiceText.Text;
            Woodpecker.Specialized.Text.stringFunctions.filterVulnerableStuff(ref Text, true);
            Text = Text.Replace(Convert.ToChar(1), ' ');
            Text = Text.Trim();
            if (Text.Length > 0)
            {
                Engine.Game.Users.broadcastMessage(FunUtils.CreateVoiceSpeakMessage(Text));
                Logging.Log("Global voice message sent to all online sessions.");
            }
        }

        private void publicRoomItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            publicRoomItemMainForm pubItems = new publicRoomItemMainForm();
            pubItems.Show();
        }
    }
}
