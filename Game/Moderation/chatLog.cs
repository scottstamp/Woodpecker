using System;

namespace Woodpecker.Game.Moderation
{
    public struct chatLog
    {
        #region Fields
        /// <summary>
        /// The ID of the sessio that chatted this message.
        /// </summary>
        private uint sessionID;
        /// <summary>
        /// The System.DateTime object representing the timestamp of this chat message.
        /// </summary>
        public DateTime Timestamp;
        /// <summary>
        /// The database ID of the user that chatted this message.
        /// </summary>
        private int userID;
        /// <summary>
        /// The database ID of the room where this message was chatted in.
        /// </summary>
        private int roomID;
        /// <summary>
        /// In case the type of this chatlog is a whisper, this value will hold the database ID of the receiving user.
        /// </summary>
        private int receiverID;
        /// <summary>
        /// The type of this chat log message.
        /// </summary>
        private chatType Type;
        /// <summary>
        /// The actual message of this chat message.
        /// </summary>
        public string Text;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a chatLog object with given parameters.
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="userID"></param>
        /// <param name="roomID"></param>
        /// <param name="receiverID"></param>
        /// <param name="Type"></param>
        /// <param name="Text"></param>
        public chatLog(uint sessionID, int userID, int roomID, int receiverID, chatType Type, string Text)
        {
            this.Timestamp = DateTime.Now;
            this.sessionID = sessionID;
            this.userID = userID;
            this.roomID = roomID;
            this.receiverID = receiverID;
            this.Type = Type;
            this.Text = Text;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Crafts the SQL query for inserting this message in the database.
        /// </summary>
        public override string ToString()
        {
            return
            "INSERT INTO " +
            "moderation_chatlog " +
            "VALUES (@moment,'" + this.sessionID + "','" + this.userID + "','" + this.roomID + "','" + this.receiverID + "','" + this.Type.ToString() + "',@text)";
        }
        #endregion
    }
}
