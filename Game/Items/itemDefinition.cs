using System;
using System.Data;

namespace Woodpecker.Game.Items
{
    /// <summary>
    /// Represents the definition of an item, containing information about the item's appearance, interaction types etc.
    /// </summary>
    public class itemDefinition
    {
        #region Fields
        /// <summary>
        /// The item definition ID of this item definition.
        /// </summary>
        public int ID;
        /// <summary>
        /// The directory ID of the cast file (.cct) of this item in the hof_furni directory on the webserver.
        /// </summary>
        public int directoryID;
        /// <summary>
        /// The sprite of this item definition.
        /// </summary>
        public string Sprite;
        /// <summary>
        /// The color string of this item. ('0,0,0' or 'null' is default)
        /// </summary>
        public string Color;
        /// <summary>
        /// The length of this item.
        /// </summary>
        public byte Length;
        /// <summary>
        /// The width of this item.
        /// </summary>
        public byte Width;
        /// <summary>
        /// The height the 'top' of this item is located at. Affects the sitheight for seats etc. If this field holds 0.0, then no items can be placed ontop of this item.
        /// </summary>
        public float topHeight;
        /// <summary>
        /// A set of booleans indicating what kind of item this is, how interaction can happen with this item etc.
        /// </summary>
        public itemBehaviourContainer Behaviour = new itemBehaviourContainer();
        /// <summary>
        /// True if this item can hold custom data.
        /// </summary>
        public bool canContainCustomData
        {
            get
            {
                return
                    this.Behaviour.customDataNumericOnOff
                    || this.Behaviour.customDataTrueFalse
                    || this.Behaviour.customDataNumericState
                    || this.Behaviour.customDataOnOff
                    || this.Behaviour.isDoor;
            }
        }
        /// <summary>
        /// The ingame name of this item definition, as expressed in external_texts.
        /// </summary>
        public string Name
        {
            get
            {
                return Engine.Game.Items.getItemName(this, 0);
            }
        }
        /// <summary>
        /// The ingame description of this item definition, as expressed in external_texts.
        /// </summary>
        public string Description
        {
            get
            {
                return Engine.Game.Items.getItemDescription(this, 0);
            }
        }
        public bool isInteractiveStance
        {
            get
            {
                return 
                    this.Behaviour.canSitOnTop
                    || this.Behaviour.canLayOnTop;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Parses a complete System.Data.DataRow of an item definition to a full itemDefinition object. Null is returned on errors.
        /// </summary>
        /// <param name="dRow">The System.Data.DataRow with all the required fields.</param>
        public static itemDefinition Parse(DataRow dRow)
        {
            itemDefinition ret = new itemDefinition();
            ret.ID = (int)dRow["id"];
            ret.directoryID = (int)dRow["cast_directory"];
            ret.Sprite = (string)dRow["sprite"];
            ret.Color = (string)dRow["color"];
            ret.Length = byte.Parse(dRow["length"].ToString());
            ret.Width = byte.Parse(dRow["width"].ToString());
            ret.topHeight = float.Parse(dRow["topheight"].ToString());
            ret.Behaviour = itemBehaviourContainer.Parse(dRow["behaviour"].ToString());
            return ret;
        }
        #endregion

        #region Subclasses
        /// <summary>
        /// A container of booleans, indicating what can be done with an item, what kind of type the item is etc.
        /// </summary>
        public class itemBehaviourContainer
        {
            #region Fields
            #region Primitive types
            /// <summary>
            /// True if this item can be placed on walls.
            /// </summary>
            public bool isWallItem;
            /// <summary>
            /// True if this item can be placed on the floor and is solid.
            /// </summary>
            public bool isSolid;
            /// <summary>
            /// True if this item can be placed on the floor and allows room units to sit on it.
            /// </summary>
            public bool canSitOnTop;
            /// <summary>
            /// True if this item can be placed on the floor and allows room units to lay on it.
            /// </summary>
            public bool canLayOnTop;
            /// <summary>
            /// True if this item can be placed on the floor and allows room units to stand on it.
            /// </summary>
            public bool canStandOnTop;
            /// <summary>
            /// True if other items can be stacked on top of this item.
            /// </summary>
            public bool canStackOnTop;
            /// <summary>
            /// True if this item can be placed on the floor and behaves as a roller. (moving room units and items located on it every 2000ms)
            /// </summary>
            public bool isRoller;
            /// <summary>
            /// True if this item can be placed on the floor, and can only appear in public spaces.
            /// </summary>
            public bool isPublicSpaceObject;
            /// <summary>
            /// True if this item is invisible to room users.
            /// </summary>
            public bool isInvisible;
            #endregion

            #region Interaction requirements
            /// <summary>
            /// True if this item requires a room user to have room rights to interact with the item.
            /// </summary>
            public bool requiresRightsForInteraction;
            /// <summary>
            /// True if this item can be placed on the floor and requires a room user to stand one tile removed from the item to interact with the item.
            /// </summary>
            public bool requiresTouchingForInteraction;
            #endregion

            #region Custom data usage
            /// <summary>
            /// True if the custom data of this item can only hold 'TRUE' or 'FALSE'. Mostly used for items that have a temporarily status, such as an opening and closing fridge.
            /// </summary>
            public bool customDataTrueFalse;
            /// <summary>
            /// True if the custom data of this item can only hold 'ON' or 'OFF'. Mostly used for items that can have a status until it is changed again, such as TVs etc.
            /// </summary>
            public bool customDataOnOff;
            /// <summary>
            /// True if the custom data of this item can only hold '1' (off) or '2' on.
            /// </summary>
            public bool customDataNumericOnOff;
            /// <summary>
            /// True if the custom data of this item can only hold an integer between 1 and 6.
            /// </summary>
            public bool customDataNumericState;
            #endregion

            #region Item usage
            /// <summary>
            /// True if this item can be used to decorate the walls/floor of a user flat.
            /// </summary>
            public bool isDecoration;
            /// <summary>
            /// True if this item is a wall item and behaves as a post.it item. ('sticky')
            /// </summary>
            public bool isPostIt;
            /// <summary>
            /// True if this item can be placed on the floor and can be opened/closed. Open items allow room units to walk through them, closed items do not.
            /// </summary>
            public bool isDoor;
            /// <summary>
            /// True if this item can be placed on the floor and can teleport room units to other teleporter items. BEAM ME UP SCOTTY FFS!!11oneone
            /// </summary>
            public bool isTeleporter;
            /// <summary>
            /// True if this item can be placed on the floor and can be opened and closed, and 'rolled' to a random number.
            /// </summary>
            public bool isDice;
            /// <summary>
            /// True if this item can be placed on the floor and can hold a user-given message as 'inscription'.
            /// </summary>
            public bool isPrizeTrophy;
            /// <summary>
            /// True if this item can be redeemed for credits.
            /// </summary>
            public bool isRedeemable;
            /// <summary>
            /// True if this item can be placed on the floor and behaves as a soundmachine. Soundmachines play user-composed music in user flats and they accept the input of sound sample items.
            /// </summary>
            public bool isSoundMachine;
            /// <summary>
            /// True if this item can be placed on the floor and can be inserted into soundmachines to compose music.
            /// </summary>
            public bool isSoundMachineSampleSet;
            #endregion
            #endregion

            #region Methods
            /// <summary>
            /// Returns a string representation of the behaviour flags of this item behaviour container.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                if (this.isWallItem)
                    sb.Append('W');
                if (this.isSolid)
                    sb.Append('S');
                if (this.canSitOnTop)
                    sb.Append('C');
                if (this.canLayOnTop)
                    sb.Append('B');
                if (this.canStandOnTop)
                    sb.Append('K');
                if (this.isRoller)
                    sb.Append('R');
                if (this.isPublicSpaceObject)
                    sb.Append('P');
                if (this.isInvisible)
                    sb.Append('I');

                if (this.requiresRightsForInteraction)
                    sb.Append('G');
                if (this.requiresTouchingForInteraction)
                    sb.Append('T');

                if (this.customDataTrueFalse)
                    sb.Append('U');
                if (this.customDataOnOff)
                    sb.Append('O');
                if (this.customDataNumericOnOff)
                    sb.Append('M');
                if (this.customDataNumericState)
                    sb.Append('Z');

                if (this.canStackOnTop)
                    sb.Append('H');
                if (this.isDecoration)
                    sb.Append('V');
                if (this.isPostIt)
                    sb.Append('J');
                if (this.isDoor)
                    sb.Append('D');
                if (this.isTeleporter)
                    sb.Append('X');
                if (this.isDice)
                    sb.Append('F');
                if (this.isPrizeTrophy)
                    sb.Append('Y');
                if (this.isRedeemable)
                    sb.Append('Q');
                if (this.isSoundMachine)
                    sb.Append('A');
                if (this.isSoundMachineSampleSet)
                    sb.Append('N');

                return sb.ToString();
            }
            /// <summary>
            /// Parses a item behaviour flag string to a itemBehaviourContainer object.
            /// </summary>
            /// <param name="s">The item behavious flag string.</param>
            public static itemBehaviourContainer Parse(string s)
            {
                itemBehaviourContainer Container = new itemBehaviourContainer();
                foreach (char c in s) // Loop through all 'flags'
                {
                    switch (c) // Determine and set 'flag'
                    {
                        #region Primitive types
                        case 'W':
                            Container.isWallItem = true;
                            break;

                        case 'S':
                            Container.isSolid = true;
                            break;

                        case 'C':
                            Container.canSitOnTop = true;
                            break;

                        case 'B':
                            Container.canLayOnTop = true;
                            break;

                        case 'K':
                            Container.canStandOnTop = true;
                            break;

                        case 'R':
                            Container.isRoller = true;
                            break;

                        case 'P':
                            Container.isPublicSpaceObject = true;
                            break;

                        case 'I':
                            Container.isInvisible = true;
                            break;
                        #endregion

                        #region Interaction requirements
                        case 'G':
                            Container.requiresRightsForInteraction = true;
                            break;

                        case 'T':
                            Container.requiresTouchingForInteraction = true;
                            break;
                        #endregion

                        #region Custom data usage
                        case 'U':
                            Container.customDataTrueFalse = true;
                            break;

                        case 'O':
                            Container.customDataOnOff = true;
                            break;

                        case 'M':
                            Container.customDataNumericOnOff = true;
                            break;

                        case 'Z':
                            Container.customDataNumericState = true;
                            break;
                        #endregion

                        #region Item usage
                        case 'H':
                            Container.canStackOnTop = true;
                            break;

                        case 'V':
                            Container.isDecoration = true;
                            break;

                        case 'J':
                            Container.isPostIt = true;
                            break;

                        case 'D':
                            Container.isDoor = true;
                            break;

                        case 'X':
                            Container.isTeleporter = true;
                            break;

                        case 'F':
                            Container.isDice = true;
                            break;

                        case 'Y':
                            Container.isPrizeTrophy = true;
                            break;

                        case 'Q':
                            Container.isRedeemable = true;
                            break;

                        case 'A':
                            Container.isSoundMachine = true;
                            break;

                        case 'N':
                            Container.isSoundMachineSampleSet = true;
                            break;
                        #endregion
                    }
                }

                return Container;
            }
            #endregion
        }
        #endregion
    }
}
