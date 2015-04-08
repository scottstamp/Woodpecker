using System;
using System.Data;
using System.Collections.Generic;

using Woodpecker.Storage;
using Woodpecker.Specialized.Text;
using Woodpecker.Game.Users.Roles;

namespace Woodpecker.Game.Users
{
    /// <summary>
    /// Contains detailed information about a user. This class extends basicUserInformation.
    /// </summary>
    public sealed class userInformation : basicUserInformation
    {
        #region Fields
        /// <summary>
        /// The ID of the session this user is currently using.
        /// </summary>
        public uint sessionID;
        /// <summary>
        /// The role/rank of this user as a value of the Woodpecker.Game.Users.userRole enum.
        /// </summary>
        public userRole Role;
        /// <summary>
        /// The amount of credits that this user currently has.
        /// </summary>
        public int Credits;
        /// <summary>
        /// The amount of tickets that this user currently has.
        /// </summary>
        public int Tickets;
        /// <summary>
        /// The amount of film for the camera that this user currently has.
        /// </summary>
        public int Film;

        public int Multiplier = 1;

        /// <summary>
        /// The current badge of this user. If blank, then no badge is used.
        /// </summary>
        public string Badge;

        #region Habbo Club (credits to Joeh for the 'work out days' method)
        /// <summary>
        /// The amount of pending Habbo Club days.
        /// </summary>
        public int clubDaysLeft;
        /// <summary>
        /// The amount of pending Habbo Club months.
        /// </summary>
        public int clubMonthsLeft;
        /// <summary>
        /// The amount of expired Habbo Club months.
        /// </summary>
        public int clubMonthsExpired;
        /// <summary>
        /// A DateTime object representing the last time the club days etc were worked out.
        /// </summary>
        public DateTime clubLastUpdate;
        /// <summary>
        /// True if the user has atleast one club day left.
        /// </summary>
        public bool hasClub
        {
            get { return this.clubDaysLeft > 0; }
        }
        /// <summary>
        /// True if the user has atleast 11 months of Habbo Club.
        /// </summary>
        public bool hasGoldClub
        {
            get { return this.clubMonthsExpired >= 11; }
        }
        #endregion

        /// <summary>
        /// The email address of this user.
        /// </summary>
        public string Email;
        /// <summary>
        /// The date of birth this user filled in during registration.
        /// </summary>
        public string DateOfBirth;
        /// <summary>
        /// A hashed copy of the password the user filled in during registration.
        /// </summary>
        public string Password;
        /// <summary>
        /// The SSO ticket assigned to the account upon opening the client.
        /// </summary>
        public string SSO;
        #endregion

        #region Methods
        /// <summary>
        /// Returns a boolean indicating if this user has access to a certain FUSE right.
        /// </summary>
        /// <param name="Right">The FUSE right to check.</param>
        public bool hasFuseRight(string Right)
        {
            return Engine.Game.Roles.roleHasRight(this.Role, this.hasClub, Right);
        }
        /// <summary>
        /// Creates the user details string for use with message 5. ('@E')
        /// </summary>
        public override string ToString()
        {
            fuseStringBuilder FSB = new fuseStringBuilder();
            FSB.appendClosedValue(this.sessionID.ToString());
            FSB.appendClosedValue(this.Username);
            FSB.appendClosedValue(this.Figure);
            FSB.appendClosedValue(this.Sex.ToString());
            FSB.appendClosedValue(this.Motto);
            FSB.appendWired(this.Tickets);
            FSB.appendClosedValue(null); // Pool figure
            FSB.appendWired(this.Film);

            return FSB.ToString();
        }
        /// <summary>
        /// Updates the 'users' table with all information from this userInformation object.
        /// </summary>
        public void fUpdate()
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", this.ID);
            Database.addParameterWithValue("password", this.Password);
            Database.addParameterWithValue("role", ((int)this.Role).ToString());
            Database.addParameterWithValue("figure", this.Figure);
            Database.addParameterWithValue("sex", this.Sex);
            Database.addParameterWithValue("motto", this.Motto);
            Database.addParameterWithValue("credits", this.Credits);
            Database.addParameterWithValue("tickets", this.Tickets);
            Database.addParameterWithValue("film", this.Film);
            Database.addParameterWithValue("email", this.Email);
            Database.Open();
            if (Database.Ready)
            {
                Database.runQuery("UPDATE users SET password = @password,role = @role,figure = @figure,sex = @sex,motto = @motto,credits = @credits,tickets = @tickets,film = @film,email = @email WHERE id = @userid");
            }
        }
        /// <summary>
        /// Updates credits/tickets/film of the user into the database.
        /// </summary>
        public void updateValueables()
        {
            Database Database = new Database(true, true);
            Database.addParameterWithValue("userid", this.ID);
            Database.addParameterWithValue("credits", this.Credits);
            Database.addParameterWithValue("tickets", this.Tickets);
            Database.addParameterWithValue("film", this.Film);
            Database.Open();
            if (Database.Ready)
            {
                Database.runQuery("UPDATE users SET credits = @credits,tickets = @tickets,film = @film WHERE id = @userid");
            }
        }
        /// <summary>
        /// Updates the appearance details (figure, sex, motto, messenger motto and current badge) of this user in the database.
        /// </summary>
        public void updateAppearanceDetails()
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", this.ID);
            Database.addParameterWithValue("figure", this.Figure);
            Database.addParameterWithValue("sex", this.Sex.ToString());
            Database.addParameterWithValue("motto", this.Motto);
            Database.addParameterWithValue("motto_messenger", messengerMotto);
            Database.addParameterWithValue("currentbadge", this.Badge);
            Database.Open();
            if (Database.Ready)
            {
                Database.runQuery("UPDATE users SET figure = @figure,sex = @sex,motto = @motto,motto_messenger = @motto_messenger,currentbadge = @currentbadge WHERE id = @userid");
            }
        }
        /// <summary>
        /// Updates the personal details (password and email address) of this user in the database.
        /// </summary>
        public void updatePersonalDetails()
        {
            Database Database = new Database(false, true);
            Database.addParameterWithValue("userid", this.ID);
            Database.addParameterWithValue("password", this.Password);
            Database.addParameterWithValue("email", this.Email);
            Database.Open();
            if (Database.Ready)
            {
                Database.runQuery("UPDATE users SET password = @password,email = @email WHERE id = @userid");
            }
        }
        /// <summary>
        /// Works out the remaining club days, months etc.
        /// </summary>
        public void updateClub(bool forceUpdate)
        {
            if (this.hasClub && (forceUpdate || this.clubLastUpdate < DateTime.Now.AddDays(-1))) // User is still saved as club, and the last check hasn't been done today
            {
                #region Update months/days etc
                TimeSpan ts = DateTime.Now.Subtract(this.clubLastUpdate);
                for (int i = 1; i <= ts.Days; i++)
                {
                    this.clubDaysLeft--;
                    if (this.clubDaysLeft <= 0) // Current month expired
                    {
                        this.clubDaysLeft = 0;
                        this.clubMonthsExpired++;
                        if (this.clubMonthsLeft > 0) // Next month on stack
                        {
                            this.clubMonthsLeft--;
                            this.clubDaysLeft = 31;
                            // Purchase club present
                        }
                    }
                }

                if (this.clubDaysLeft == 0) // Club expired
                    this.Figure = Core.Configuration.getConfigurationValue("users.figure.default"); // Reset default figure

                this.clubLastUpdate = DateTime.Now;
                Database Database = new Database(false, true);
                Database.addParameterWithValue("userid", this.ID);
                Database.addParameterWithValue("figure", this.Figure);
                Database.addParameterWithValue("club_daysleft", this.clubDaysLeft);
                Database.addParameterWithValue("club_monthsleft", this.clubMonthsLeft);
                Database.addParameterWithValue("club_monthsexpired", this.clubMonthsExpired);
                Database.addParameterWithValue("club_lastupdate", this.clubLastUpdate);
                Database.Open();
                if (Database.Ready)
                    Database.runQuery("UPDATE users SET figure = @figure,club_daysleft = @club_daysleft,club_monthsleft = @club_monthsleft,club_monthsexpired = @club_monthsexpired,club_lastupdate = @club_lastupdate WHERE id = @userid LIMIT 1");

                #endregion
            }
        }
        #endregion
    }
}