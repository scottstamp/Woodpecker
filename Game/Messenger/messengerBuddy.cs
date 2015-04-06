using System;
using System.Data;

using Woodpecker.Sessions;
using Woodpecker.Specialized.Text;
using Woodpecker.Game.Users;
using Woodpecker.Game.Rooms.Instances;

namespace Woodpecker.Game.Messenger
{
    /// <summary>
    /// Represents a buddy on the in-game messenger. ('Console')
    /// </summary>
    public class messengerBuddy : basicUserInformation
    {
        #region Methods
        /// <summary>
        /// Parses the fields id,username,figure,sex,motto_messenger and lastactivity from a System.Data.DataRow object to a messengerBuddy object and returns it. An empty messengerBuddy object is returned on errors.
        /// </summary>
        /// <param name="dRow">The System.Data.DataRow with the required fields.</param>
        public static messengerBuddy Parse(DataRow dRow)
        {
            try
            {
                messengerBuddy Buddy = new messengerBuddy();
                Buddy.ID = (int)dRow["id"];
                Buddy.Username = (string)dRow["username"];
                Buddy.Figure = (string)dRow["figure"];
                Buddy.Sex = Convert.ToChar(dRow["sex"].ToString());
                Buddy.messengerMotto = (string)dRow["motto_messenger"];
                Buddy.lastActivity = (DateTime)dRow["lastactivity"];

                return Buddy;
            }
            catch { return new messengerBuddy(); }
        }
        /// <summary>
        /// Creates the messenger buddy string of this user information and returns it.
        /// </summary>
        public override string ToString()
        {
            fuseStringBuilder FSB = new fuseStringBuilder();
            FSB.appendWired(this.ID);
            FSB.appendClosedValue(this.Username);
            FSB.appendWired(this.Sex == 'M');
            FSB.appendClosedValue(messengerMotto);

            bool isOnline = Engine.Game.Users.userIsLoggedIn(this.ID);
            FSB.appendWired(isOnline);

            if (isOnline) // User is online
            {
                Session userSession = Engine.Game.Users.getUserSession(this.ID);
                if (userSession.inRoom)
                {
                    if (userSession.roomInstance.Information.isUserFlat)
                        FSB.Append("Floor1a");
                    else
                        FSB.Append(userSession.roomInstance.Information.Name);
                }
                this.lastActivity = DateTime.Now;
            }
            else
                FSB.Append("Hotel View");

            FSB.appendChar(2);
            FSB.appendClosedValue(messengerLastActivity);
            FSB.appendClosedValue(this.Figure);

            return FSB.ToString();
        }
        /// <summary>
        /// Creates the messenger buddy status string of this user information and returns it.
        /// </summary>
        public string ToStatusString()
        {
            fuseStringBuilder FSB = new fuseStringBuilder();
            FSB.appendWired(this.ID);
            FSB.appendClosedValue(messengerMotto);

            bool isOnline = Engine.Game.Users.userIsLoggedIn(this.ID);
            FSB.appendWired(isOnline);

            if (isOnline) // User is online
            {
                Session userSession = Engine.Game.Users.getUserSession(this.ID);
                if (userSession.inRoom)
                {
                    if (userSession.roomInstance.Information.isUserFlat)
                        FSB.Append("Floor1a");
                    else
                        FSB.Append(userSession.roomInstance.Information.Name);
                }
                else
                    FSB.Append("on Hotel View");
            }
            else
                FSB.Append(messengerLastActivity);
            FSB.appendChar(2);

            return FSB.ToString();
        }
        #endregion
    }
}
