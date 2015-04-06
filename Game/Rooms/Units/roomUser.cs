using System;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;

using Woodpecker.Sessions;
using Woodpecker.Specialized.Text;
using Woodpecker.Game.Users;
using Woodpecker.Net.Game.Messages;
using Woodpecker.Game.Rooms.Pathfinding;

namespace Woodpecker.Game.Rooms.Units
{
    /// <summary>
    /// Represents a user in a room instance.
    /// </summary>
    public class roomUser : roomUnit
    {
        #region Fields
        #region User
        /// <summary>
        /// The Woodpecker.Sessions.Session object of this room user.
        /// </summary>
        public Session Session;
        /// <summary>
        /// The botID of this room user if user is bot.
        /// </summary>
        public Woodpecker.Game.Items.Bots.virtualBotInformation bInfo;
        /// <summary>
        /// True if this room user is the owner/staff member with fuse_all_rooms_controller in the room instance it currently belongs to.
        /// </summary>
        public bool isOwner;
        /// <summary>
        /// True if this room user has 'room rights' in the room instance it currently belongs to.
        /// </summary>
        public bool hasRights;
        #region Muting
        private bool _Muted;
        private DateTime _muteExpires;
        /// <summary>
        /// True if this user is muted.
        /// </summary>
        public bool isMuted
        {
            get
            {
                if (_Muted && _muteExpires > DateTime.Now)
                {
                    _Muted = false;
                    _muteExpires = new DateTime();
                }

                return _Muted;
            }
            set
            {
                _Muted = value;
                _muteExpires = DateTime.Now.AddMinutes(20); // TODO: Configure this
            }
        }
        #endregion
        #endregion

        #region Pathfinding
        /// <summary>
        /// True if this user currently can't move.
        /// </summary>
        public bool Clamped;
        #endregion
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a roomUser object for a given session.
        /// </summary>
        /// <param name="mySession">The Woodpecker.Sessions.Session object of the session where this room user belongs to.</param>
        public roomUser(Session mySession)
        {
            this.Session = mySession;
        }

        public roomUser(Items.Bots.virtualBotInformation bInfo)
        {
            this.bInfo = bInfo;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Converts this room user to a room user details string and returns it.
        /// </summary>
        public override string ToString()
        {
            if (this.Session != null && this.Session.User != null)
            {
                fuseStringBuilder FSB = new fuseStringBuilder();
                FSB.appendKeyValueParameter("i", this.ID);
                FSB.appendKeyValueParameter("a", Session.User.ID);
                FSB.appendKeyValueParameter("n", Session.User.Username);
                FSB.appendKeyValueParameter("f", Session.User.Figure);
                FSB.appendKeyValueParameter("s", Session.User.Sex);
                FSB.appendKeyValueParameter("l", this.X + " " + this.Y + " " + this.Z);
                if (Session.User.Motto.Length > 0)
                    FSB.appendKeyValueParameter("c", Session.User.Motto);
                if (Session.User.Badge.Length > 0)
                    FSB.appendKeyValueParameter("b", Session.User.Badge);
                // TODO: Swimoutfit

                return FSB.ToString();
            }
            else if (this.bInfo != null)
            {
                fuseStringBuilder FSB = new fuseStringBuilder();
                FSB.appendKeyValueParameter("i", this.ID);
                FSB.appendKeyValueParameter("a", -1);
                FSB.appendKeyValueParameter("n", bInfo.Name);
                FSB.appendKeyValueParameter("f", bInfo.Figure);
                //FSB.appendKeyValueParameter("s", "M");
                FSB.appendKeyValueParameter("l", this.X + " " + this.Y + " " + this.Z);
                //if (Session.User.Motto.Length > 0)
                FSB.appendKeyValueParameter("c", "I'm a Bot!");
                //if (Session.User.Badge.Length > 0)
                FSB.appendKeyValueParameter("b", "ADM");
                FSB.appendNewLineValue("[bot]");

                return FSB.ToString();
                
            }
            else
                return "";
        }

        public void refreshRights()
        {
            this.removeStatus("flatctrl");
            string flatControlValue = null;

            if (this.hasRights)
                this.Session.gameConnection.sendMessage(new serverMessage(42)); // "@j"
            if (this.isOwner)
            {
                this.Session.gameConnection.sendMessage(new serverMessage(47)); // "@o"
                flatControlValue = "useradmin";
            }

            if (this.hasRights || this.isOwner)
                this.addStatus("flatctrl", "flatctrl", flatControlValue, 0, null, 0, 0);

            this.requiresUpdate = true;
        }
        #endregion
    }
}
