using System;

using Woodpecker.Storage;

using Woodpecker.Game.Users.Roles;

namespace Woodpecker.Game.Users
{
    /// <summary>
    /// Contains the basic information about a user.
    /// </summary>
    public class basicUserInformation
    {
        #region Fields
        /// <summary>
        /// The database ID of this user.
        /// </summary>
        public int ID;
        /// <summary>
        /// The username of this user.
        /// </summary>
        public string Username;

        /// <summary>
        /// The System.DateTime object representing the last activity of this user.
        /// </summary>
        public DateTime lastActivity;
        /// <summary>
        /// This user's last activity formatted as a string in the format dd-MM-yyyy HH:mm:ss.
        /// </summary>
        public string messengerLastActivity
        {
            get
            {
                return this.lastActivity.ToString("dd-MM-yyyy HH:mm:ss");
            }
        }
        /// <summary>
        /// The figure string (25 characters, digits only) of this user.
        /// </summary>
        public string Figure;
        /// <summary>
        /// The sex of this user. ('M' = male, 'F' = female)
        /// </summary>
        public char Sex;
        /// <summary>
        /// The motto (also known as 'mission') of this user.
        /// </summary>
        public string Motto;
        /// <summary>
        /// The motto displayed for this user in the messenger.
        /// </summary>
        public string messengerMotto;
        #endregion

        #region Methods
        /// <summary>
        /// Updates the last activity datetime in the database and this object to the current time.
        /// </summary>
        public void updateLastActivity()
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", this.ID);
            Database.Open();
            if (Database.Ready)
                Database.runQuery("UPDATE users SET lastactivity = NOW() WHERE id = @userid");

            this.lastActivity = DateTime.Now;
        }
        #endregion
    }
}
