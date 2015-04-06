using System;
using System.Data;

using Woodpecker.Storage;

namespace Woodpecker.Game.Users
{
    public class userAccessInformation
    {
        #region Fields
        /// <summary>
        /// The database ID of the user of this access information.
        /// </summary>
        public int userID;
        /// <summary>
        /// The ID of the session that was being used during the access this access information represents.
        /// </summary>
        public uint sessionID;
        /// <summary>
        /// The IP address of the session that was being used during the access this access information represents.
        /// </summary>
        public string IP;
        /// <summary>
        /// The 'machine ID' (Shockwave client ID) that was being used during the access this access information represents.
        /// </summary>
        public string machineID;
        /// <summary>
        /// The datetime of the last access of the user of this access information.
        /// </summary>
        public DateTime lastUpdate;
        #endregion

        #region Methods
        /// <summary>
        /// Inserts a new row with the current access details into the database's 'users_access' table.
        /// </summary>
        public void Update()
        {
            Database dbClient = new Database(false, true);
            dbClient.addParameterWithValue("userid", this.userID);
            dbClient.addParameterWithValue("sessionid", this.sessionID);
            dbClient.addParameterWithValue("ip", this.IP);
            dbClient.addParameterWithValue("machineid", machineID);

            dbClient.Open();
            if (dbClient.Ready)
            {
                dbClient.runQuery("INSERT INTO users_access VALUES (@userid,@sessionid,NOW(),@ip,@machineid)");
                this.lastUpdate = DateTime.Now;
            }
        }
        /// <summary>
        /// Parses a System.Data.DataRow object to a userAccessInformation object.
        /// </summary>
        /// <param name="dRow">The System.Data.DataRow object containing the required fields.</param>
        public static userAccessInformation Parse(DataRow dRow)
        {
            if (dRow == null)
                return null;
            else
            {
                userAccessInformation ret = new userAccessInformation();
                ret.userID = (int)dRow["userid"];
                ret.sessionID = uint.Parse(dRow["sessionid"].ToString());
                ret.IP = (string)dRow["ip"];
                ret.machineID = (string)dRow["machineid"];
                ret.lastUpdate = (DateTime)dRow["moment"];

                return ret;
            }
        }
        #endregion
    }
}
