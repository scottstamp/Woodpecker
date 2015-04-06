using System;
using System.Data;

using Woodpecker.Game.Rooms.Instances;
using Woodpecker.Game.Rooms.Pathfinding;

namespace Woodpecker.Game.Rooms
{
    public class roomModel
    {
        #region Fields
        /// <summary>
        /// The name of this room model.
        /// </summary>
        public string typeName;
        /// <summary>
        /// A value of the roomModelUserType enum representing the type of this model. (public space, user flat etc)
        /// </summary>
        internal roomModelUserType userType;
        /// <summary>
        /// The X position of the door of this room model.
        /// </summary>
        public byte doorX;
        /// <summary>
        /// The Y position of the door of this room model.
        /// </summary>
        public byte doorY;
        /// <summary>
        /// The height of the door of this room model as a float.
        /// </summary>
        public float doorZ;
        /// <summary>
        /// The heightmap of this room model as a string.
        /// </summary>
        public string sHeightmap;
        private string[] mHeightMapAxes;
        public string[] heightMapAxes
        {
            get { return (string[])mHeightMapAxes.Clone(); }
        }
        #endregion
        
        #region Methods
        public static roomModel Parse(DataRow dRow)
        {
            roomModel retModel = new roomModel();
            try
            {
                retModel.typeName = (string)dRow["modeltype"];
                retModel.userType = (roomModelUserType)int.Parse(dRow["usertype"].ToString());
                retModel.doorX = (byte)int.Parse(dRow["door_x"].ToString());
                retModel.doorY = (byte)int.Parse(dRow["door_y"].ToString());
                retModel.doorZ = (float)dRow["door_z"];
                retModel.sHeightmap = dRow["heightmap"].ToString().ToLower();
                retModel.mHeightMapAxes = retModel.sHeightmap.Split('|');
            }
            catch { retModel = null; }

            return retModel;
        }
        #endregion
    }
    /// <summary>
    /// Represents the user type of a room model.
    /// </summary>
    internal enum roomModelUserType
    {
        /// <summary>
        /// This model can be used with public spaces only.
        /// </summary>
        PublicSpaceModel = 0,
        /// <summary>
        /// This model is a generic user flat.
        /// </summary>
        UserFlatModel = 1,
        /// <summary>
        /// This model is a special user flat which requires fuse_use_special_room_layouts to create an instance of.
        /// </summary>
        UserFlatSpecialModel = 2
    }
}
