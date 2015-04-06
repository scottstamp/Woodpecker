using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

using Woodpecker.Game.Rooms.Pathfinding;

namespace Woodpecker.Game.Rooms.Units
{
    /// <summary>
    /// A non-inheritable class that is the base for room users, bots and virtual pets.
    /// </summary>
    public abstract class roomUnit
    {
        #region Fields
        #region General
        /// <summary>
        /// The room user ID (not the database ID) of this room unit.
        /// </summary>
        public int ID;
        #endregion

        #region Pathfinding
        /// <summary>
        /// The X position of this room unit in the room.
        /// </summary>
        public byte X;
        /// <summary>
        /// The Y position of this room unit in the room.
        /// </summary>
        public byte Y;
        /// <summary>
        /// The height of this room unit in the room as a float.
        /// </summary>
        public float Z;
        /// <summary>
        /// True if this room unit is currently moving in the room.
        /// </summary>
        public bool Moves;
        /// <summary>
        /// The next X position of this room unit in the room.
        /// </summary>
        public byte nextX;
        /// <summary>
        /// The next Y position of this room unit in the room.
        /// </summary>
        public byte nextY;
        /// <summary>
        /// The next height of this room unit in the room as a float.
        /// </summary>
        public float nextZ;
        /// <summary>
        /// True if this user has set a tile to move to and the moving hasn't been processed yet.
        /// </summary>
        public bool goalTileSet;
        /// <summary>
        /// The X coordinate of the tile that this user wants to move to.
        /// </summary>
        public byte goalX;
        /// <summary>
        /// The Y coordinate of the tile that this user wants to move to.
        /// </summary>
        public byte goalY;
        /// <summary>
        /// True if this room unit is allowed to move to the next tile without being affected by the map.
        /// </summary>
        public bool allowOverideNextTile;
        public List<blisterMoleNode> Path = new List<blisterMoleNode>();
        #endregion

        #region Appearance
        /// <summary>
        /// The current head rotation of this room user a byte.
        /// </summary>
        public byte rotationHead;
        /// <summary>
        /// The current body rotation of this room user as a byte.
        /// </summary>
        public byte rotationBody;
        #endregion

        #region Statuses
        /// <summary>
        /// A System.Collections.Generic.Dictionary with a string as key and a roomUnitStatus as value.
        /// </summary>
        private Dictionary<string, roomUnitStatus> Statuses = new Dictionary<string, roomUnitStatus>();
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Adds a given unique status to this room unit and requests a refresh in the room.
        /// </summary>
        /// <param name="Key">The key the status should have in the status collection. The key should be unique.</param>
        /// <param name="Name">The actual name of the status.</param>
        /// <param name="Data">Optional. The data/variable this status will hold.</param>
        /// <param name="secsLifetime">The amount of seconds this status lasts. If the status expires, it is removed and the room unit is refreshed in room.</param>
        /// <param name="Action">Optional. The status name can be replaced with another status name (the 'Action') after a certain amount of seconds.</param>
        /// <param name="secsActionSwitch">The delay between the original status and the action in seconds.</param>
        /// <param name="secsActionLength">The amount of seconds the action lasts once enabled. Upon expiring of the action, the normal status is restored.</param>
        public void addStatus(string Key, string Name, string Data, int secsLifetime, string Action, int secsActionSwitch, int secsActionLength)
        {
            if (this.Statuses.ContainsKey(Key))
                this.Statuses.Remove(Key);

            this.Statuses.Add(Key, new roomUnitStatus(Name, Data, secsLifetime, Action, secsActionSwitch, secsActionLength));
        }
        /// <summary>
        /// Tries to remove a status with a given key. A request in the room for this room unit is requested.
        /// </summary>
        /// <param name="Key">The key of the status to remove.</param>
        public void removeStatus(string Key)
        {
            this.Statuses.Remove(Key);
            this.requiresUpdate = true;
        }
        /// <summary>
        /// Returns a boolean indicating if this room unit currently has a status with a given key.
        /// </summary>
        /// <param name="Key">The key of the status to check for.</param>
        public bool hasStatus(string Key)
        {
            return this.Statuses.ContainsKey(Key);
        }
        /// <summary>
        /// Private boolean holding the current update status of this room unit.
        /// </summary>
        private bool _requiresUpdate;
        /// <summary>
        /// Holds true if this room unit currently requires an update in the room for whatever reason.
        /// </summary>
        public bool requiresUpdate
        {
            get
            {
                if (_requiresUpdate)
                    return true;

                lock (this.Statuses)
                {
                    foreach (KeyValuePair<string, roomUnitStatus> lStatus in this.Statuses)
                    {
                        if (lStatus.Value.Updated)
                            return true;
                    }
                }

                return false;
            }
            set { _requiresUpdate = value; }
        }
        /// <summary>
        /// Converts the current status of this room unit to a string and returns it.
        /// </summary>
        public string ToStatusString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.ID + " ");
            sb.Append(this.X + ",");
            sb.Append(this.Y + ",");
            sb.Append(this.Z.ToString("0.00", CultureInfo.InvariantCulture) + ",");
            sb.Append(this.rotationHead + ",");
            sb.Append(this.rotationBody + "/");

            if (this.Moves)
                sb.Append("mv " + this.nextX + "," + this.nextY + "," + this.nextZ.ToString("0.00", CultureInfo.InvariantCulture) + "/");

            lock (this.Statuses)
            {
                List<string> statusesToRemove = new List<string>();
                foreach (KeyValuePair<string, roomUnitStatus> statusCollector in this.Statuses)
                {
                    if (statusCollector.Value.Valid)
                    {
                        sb.Append(statusCollector.Value.Name);
                        if (statusCollector.Value.Data != null)
                        {
                            sb.Append(' ');
                            sb.Append(statusCollector.Value.Data);
                        }
                        sb.Append('/');
                    }
                    else
                        statusesToRemove.Add(statusCollector.Key);
                }

                foreach (string sStatus in statusesToRemove) this.Statuses.Remove(sStatus); // Remove invalid statuses
            }

            sb.Append(Convert.ToChar(13));
            return sb.ToString();
        }
    
        #endregion
    }

    /// <summary>
    /// Represents a unique status of a room unit. Thanks to Joe Hegarty (Thor) for the 'is still active, actiontimer' etc stuff.
    /// </summary>
    public class roomUnitStatus
    {
        #region Fields
        /// <summary>
        /// The key/name of this status. Will be replaced by 'Action' when this is a flipping status.
        /// </summary>
        public string Name;
        /// <summary>
        /// The value of this status. This field can be omitted.
        /// </summary>
        public string Data;
        /// <summary>
        /// A string that will hold the action to be switched with the key when this is a flipping status.
        /// </summary>
        private string Action;
        /// <summary>
        /// The amount of seconds that this status last before it is switched with the action.
        /// </summary>
        private int mActionSwitch;
        /// <summary>
        /// The amount of seconds that the action of this status lasts before it turns into the normal status again.
        /// </summary>
        private int mActionLength;
        /// <summary>
        /// A double that keeps the amount of seconds the action of this status requires updating.
        /// </summary>
        private double mActionEndTime;
        /// <summary>
        /// A double that keeps the amount of seconds for stuff.
        /// </summary>
        private double mEndTime;
        /// <summary>
        /// True if this status is currently using it's Action value.
        /// </summary>
        private bool mActionActive;
        private bool mLastCheckResult;
        public bool Updated
        {
            get
            {
                bool ret = false;
                bool Check = this.Valid;
                if (Check != mLastCheckResult)
                    ret = true;
                mLastCheckResult = Check;

                return ret;
            }
        }
        /// <summary>
        /// True if this status is still active.
        /// </summary>
        public bool Valid
        {
            get
            {
                if (this.Static)
                    return true;
                else
                {
                    if (mEndTime < DateTime.Now.TimeOfDay.TotalSeconds) // Non-persistent status expired
                        return false;
                }

                if (this.Action != null) // Status changes (eg, carry item)
                {
                    if (mActionEndTime < DateTime.Now.TimeOfDay.TotalSeconds) // Status requires update
                    {
                        string s = this.Name;
                        this.Name = this.Action;
                        this.Action = s;

                        // Calculate new action length
                        int switchSeconds = 0;
                        if (mActionActive)
                            switchSeconds = mActionSwitch;
                        else
                            switchSeconds = mActionLength;

                        mActionActive = !mActionActive;
                        mActionEndTime = DateTime.Now.TimeOfDay.TotalSeconds + switchSeconds;
                        mLastCheckResult = !mLastCheckResult;
                    }
                }

                return true; // Still valid!
            }
        }
        /// <summary>
        /// True if this status does not expire/change.
        /// </summary>
        public bool Static
        {
            get { return mEndTime == 0.0d; }
        }
        #endregion

        #region Constructors
        public roomUnitStatus(string Name, string Data, int secsLifetime, string Action, int secsActionSwitch, int secsActionLength)
        {
            this.Name = Name;
            this.Data = Data;
            if (secsLifetime > 0)
                mEndTime = DateTime.Now.TimeOfDay.TotalSeconds + secsLifetime;

            if (Action != null)
            {
                this.Action = Action;
                mActionSwitch = secsActionSwitch;
                mActionLength = secsActionLength;
                mActionEndTime = DateTime.Now.TimeOfDay.TotalSeconds + secsActionSwitch;
            }
        }
        #endregion
    }
}
