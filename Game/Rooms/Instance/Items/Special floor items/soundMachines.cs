using System;
using System.Data;
using System.Threading;
using System.Collections.Generic;

using Woodpecker.Storage;
using Woodpecker.Specialized.Text;
using Woodpecker.Net.Game.Messages;

using Woodpecker.Game.Items;
using Woodpecker.Game.Items.Pets;
using Woodpecker.Game.Rooms.Units;
using Woodpecker.Game.Rooms.Pathfinding;

namespace Woodpecker.Game.Rooms.Instances.Interaction
{
    public partial class flatReactor : roomReactor
    {

    }
    public class soundMachineReactor : Reactor
    {
        #region Target methods
        /// <summary>
        /// 218 - "CZ"
        /// </summary>
        public void SAVESONG()
        {

        }
        /// <summary>
        /// 219 - "C["
        /// </summary>
        public void INSERT_SOUND_PACKAGE()
        {

        }
        /// <summary>
        /// 220 - "C\"
        /// </summary>
        public void EJECT_SOUND_PACKAGE()
        {

        }
        #endregion
    }
}

namespace Woodpecker.Game.Rooms.Instances
{
    public partial class roomInstance
    {
        #region Fields
        /// <summary>
        /// The database ID of the soundmachine item that is currently placed in this room instance.
        /// </summary>
        private int mSoundMachineID;
        /// <summary>
        /// The database ID of the soundmachine item that is currently placed in this room instance.
        /// </summary>
        public int soundMachineID
        {
            get { return mSoundMachineID; }
        }
        #endregion

        #region Methods

        #endregion
    }
}