using System;
using System.Text;

using Woodpecker.Specialized.Text;
using Woodpecker.Specialized.Encoding;

namespace Woodpecker.Net.Game.Messages
{
    /// <summary>
    /// Represents a server>client message for the Habbo FUSE protocol.
    /// </summary>
    public class serverMessage : fuseStringBuilder
    {
        #region Constructors
        /// <summary>
        /// Constructs a serverMessage with no content.
        /// </summary>
        public serverMessage()
        {
            base.Content = new StringBuilder();
        }
        /// <summary>
        /// Constructs a server>client message with a certain ID.
        /// </summary>
        /// <param name="messageID">The ID of this message.</param>
        public serverMessage(int messageID)
        {
            base.Content = new StringBuilder(base64Encoding.Encode(messageID));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Clears the stringbuilder and initializes it with a certain message ID.
        /// </summary>
        /// <param name="messageID">The ID of this message.</param>
        public void Initialize(int messageID)
        {
            base.Content = new StringBuilder(base64Encoding.Encode(messageID));
        }

        /// <summary>
        /// Appends char1 to the message content and returns the message content as a string.
        /// </summary>
        public override string ToString()
        {
            return base.ToString() + Convert.ToChar(1);
        }

        public static serverMessage createDefaultMessageBox(string Text)
        {
            serverMessage retCast = new serverMessage(139); // "BK"
            retCast.Append(Text);
            return retCast;
        }
        #endregion
    }
}
