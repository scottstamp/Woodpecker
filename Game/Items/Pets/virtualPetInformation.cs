using System;
using System.Data;
using System.Drawing;

using Woodpecker.Storage;

namespace Woodpecker.Game.Items.Pets
{
    /// <summary>
    /// Contains information about a virtual pet. Virtual pets can be purchased by users to be put in their virtual rooms. Pets interact with other room units, demand food and attention etc.
    /// </summary>
    public class virtualPetInformation
    {
        #region Fields
        #region Permanent values
        /// <summary>
        /// The database ID of this virtual pet. The ID is equal to the database ID of the 'nest item' of the pet.
        /// </summary>
        public int ID;
        /// <summary>
        /// The type of this virtual pet. (specie)
        /// </summary>
        public char Type;
        /// <summary>
        /// The name of this virtual pet.
        /// </summary>
        public string Name;
        public int naturePositive;
        public int natureNegative;

        /// <summary>
        /// The race of this virtual pet. Pets are shipped in many races.
        /// </summary>
        public byte Race;
        /// <summary>
        /// The color/and or pattern of this pet as a hex HTML color.
        /// </summary>
        public string Color;
        /// <summary>
        /// The figure string of this pet, consisting out of it's type, race and color.
        /// </summary>
        public string Figure
        {
            get
            {
                return
                    this.Type.ToString() + " " +
                    String.Format("{0:000}", this.Race) + " " +
                    this.Color;
            }
        }
        #endregion

        #region Dynamical values
        /// <summary>
        /// The last saved X position of this pet in the room. Default = 0.
        /// </summary>
        public byte lastX;
        /// <summary>
        /// The last saved Y position of this pet in the room. Default = 0.
        /// </summary>
        public byte lastY;
        public float fFriendship;
        public DateTime dtLastKip;
        public DateTime dtLastFed;
        public DateTime dtLastDrink;
        public DateTime dtLastPlayToy;
        public DateTime dtLastPlayUser;
        public DateTime dtBorn;

        /// <summary>
        /// The age of this pet as an integer. This is the amount of days since the day the pet was 'born'. (purchased)
        /// </summary>
        public int Age
        {
            get
            {
                return (int)((DateTime.Now - this.dtBorn)).TotalDays;
            }
        }
        public float Hunger
        {
            get { return 0; }
        }
        public float Thirst
        {
            get { return 0; }
        }
        public float Happiness
        {
            get { return 0; }
        }
        public float Energy
        {
            get { return 0; }
        }
        public float Friendship
        {
            get { return 0; }
        }
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Updates the dynamical fields for this pet in the database.
        /// </summary>
        public void Update()
        {
            Database dbClient = new Database(false, true);
            dbClient.addParameterWithValue("id", this.ID);
            dbClient.addParameterWithValue("friendship", this.fFriendship);
            dbClient.addParameterWithValue("x", this.lastX);
            dbClient.addParameterWithValue("y", this.lastY);
            dbClient.addParameterWithValue("last_kip", this.dtLastKip);
            dbClient.addParameterWithValue("last_eat", this.dtLastFed);
            dbClient.addParameterWithValue("last_drink", this.dtLastDrink);
            dbClient.addParameterWithValue("last_playtoy", this.dtLastPlayToy);
            dbClient.addParameterWithValue("last_playuser", this.dtLastPlayUser);

            dbClient.Open();
            if (dbClient.Ready)
                dbClient.runQuery(
                    "UPDATE items_pets SET " +

                    "friendship = @friendship," +
                    "x = @x," +
                    "y = @y," +
                    "last_kip = @last_kip," +
                    "last_eat = @last_eat," +
                    "last_drink = @last_drink," +
                    "last_playtoy = @last_playtoy," +
                    "last_playuser = @last_playuser " +

                    "WHERE id = @id " +
                    "LIMIT 1");
        }
        /// <summary>
        /// Parses a System.Data.DataRow with the required fields to a full virtualPetInformation object. Null is returned on errors.
        /// </summary>
        /// <param name="dRow">The System.Data.DataRow object with all the required fields for the parse.</param>
        public static virtualPetInformation Parse(DataRow dRow)
        {
            if (dRow == null)
                return null;

            virtualPetInformation Pet = new virtualPetInformation();
            // Constant values
            Pet.ID = (int)dRow["id"];
            Pet.Name = (string)dRow["name"];
            Pet.Type = char.Parse(dRow["type"].ToString());
            Pet.Race = byte.Parse(dRow["race"].ToString());
            Pet.Color = "#" + dRow["color"].ToString();
            Pet.naturePositive = (int)dRow["nature_positive"];
            Pet.natureNegative = (int)dRow["nature_negative"];

            // Event recordings
            Pet.dtBorn = (DateTime)dRow["born"];
            Pet.dtLastKip = (DateTime)dRow["last_kip"];
            Pet.dtLastFed = (DateTime)dRow["last_eat"];
            Pet.dtLastDrink = (DateTime)dRow["last_drink"];
            Pet.dtLastPlayToy = (DateTime)dRow["last_playtoy"];
            Pet.dtLastPlayUser = (DateTime)dRow["last_playuser"];

            // Special values
            Pet.fFriendship = (float)dRow["friendship"];
            Pet.lastX = byte.Parse(dRow["x"].ToString());
            Pet.lastY = byte.Parse(dRow["y"].ToString());

            return Pet;
        }
        #endregion
    }
}
