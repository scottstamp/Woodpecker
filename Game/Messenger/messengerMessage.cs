using System;

using Woodpecker.Specialized.Text;

namespace Woodpecker.Game.Messenger
{
    /// <summary>
    /// Represents a sent text message for the in-game messenger. ('Console')
    /// </summary>
    public class messengerMessage
    {
        #region Properties
        /// <summary>
        /// The ID of this message.
        /// </summary>
        public int ID;
        /// <summary>
        /// The database ID of the user that sent this message.
        /// </summary>
        public int senderID;
        /// <summary>
        /// The System.DateTime object containing the date and time this message was sent on.
        /// </summary>
        public DateTime Sent;
        /// <summary>
        /// This message's sent datetime formatted as a string in the format dd-MM-yyyy HH:mm:ss.
        /// </summary>
        public string sentString
        {
            get
            {
                return this.Sent.ToString("dd-MM-yyyy HH:mm:ss");
            }
        }
        /// <summary>
        /// The message body of this message. (actual text)
        /// </summary>
        public string Body;
        #endregion

        #region Methods
        /// <summary>
        /// Creates the messenger textmessage message of this string and returns it.
        /// </summary>
        public override string ToString()
        {
            fuseStringBuilder FSB = new fuseStringBuilder();
            FSB.appendWired(this.ID);
            FSB.appendWired(this.senderID);
            FSB.appendClosedValue(this.sentString);
            FSB.appendClosedValue(this.Body);

            return FSB.ToString();
        }
        #endregion
    }
}
