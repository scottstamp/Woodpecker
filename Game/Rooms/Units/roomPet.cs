using System;

using Woodpecker.Storage;
using Woodpecker.Specialized.Text;
using Woodpecker.Game.Items.Pets;

namespace Woodpecker.Game.Rooms.Units
{
    public class roomPet : roomUnit
    {
        #region Fields
        /// <summary>
        /// A virtualPetInformation instance with all kinds of information of this pet, such as it's appearance etc.
        /// </summary>
        public virtualPetInformation Information;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a roomPet object for a given virtualPetInformation object.
        /// </summary>
        /// <param name="pInfo">The virtualPetInformation that holds the values for this room pet.</param>
        public roomPet(virtualPetInformation pInfo)
        {
            this.Information = pInfo;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Converts this virtual room pet to a string to make it appear for game clients.
        /// </summary>
        public override string ToString()
        {
            fuseStringBuilder FSB = new fuseStringBuilder();
            if(this.Information != null)
            {
                FSB.appendKeyValueParameter("i", this.ID);
                FSB.appendKeyValueParameter("n", this.Information.ID.ToString() + Convert.ToChar(4).ToString() + this.Information.Name);
                FSB.appendKeyValueParameter("f", this.Information.Figure);
                FSB.appendKeyValueParameter("l", this.X + " " + this.Y + " " + stringFunctions.formatFloatForClient(this.Z));

                return FSB.ToString();
            }

            return FSB.ToString();
        }
        #endregion
    }
}
