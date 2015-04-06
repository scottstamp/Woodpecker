using System;
using System.Text;
using System.Data;
using System.Collections.Generic;

using Woodpecker.Core;
using Woodpecker.Storage;

namespace Woodpecker.Game.Users.Roles
{
    /// <summary>
    /// Provides various functions for user roles and their permissions, badges etc.
    /// </summary>
    public class roleManager
    {
        #region Fields
        #region Main roles
        private Dictionary<userRole, List<string>> roleRights = new Dictionary<userRole,List<string>>();
        private Dictionary<userRole, List<string>> roleBadges = new Dictionary<userRole, List<string>>();
        #endregion

        #region Club
        private List<string> clubRights = new List<string>();
        #endregion
        #endregion

        #region Methods
        public void loadRoles()
        {
            Logging.Log("Initializing user roles...");
            Database Database = new Database(true, false);
            DataTable dTable = new DataTable();
            if (Database.Ready)
            {
                for (int roleID = 0; roleID <= 6; roleID++)
                {
                    userRole Role = (userRole)roleID;
                    Logging.Log("Role '" + Role.ToString() + "'");

                    // Role rights
                    List<string> tmpList = new List<string>();
                    dTable = Database.getTable("SELECT fuseright FROM users_roles_fuserights WHERE minrole <= '" + roleID + "'");
                    
                    foreach (DataRow dRow in dTable.Rows)
                    {
                        string Right = (string)dRow["fuseright"];
                        tmpList.Add(Right);
                        //Logging.Log("   - FUSE right: " + Right);
                    }
                    this.roleRights.Add(Role, tmpList);

                    // Role badges
                    tmpList = new List<string>();
                    dTable = Database.getTable("SELECT badge FROM users_roles_badges WHERE minrole <= '" + roleID + "'");
                    foreach (DataRow dRow in dTable.Rows)
                    {
                        string Badge = (string)dRow["badge"];
                        tmpList.Add(Badge);
                        //Logging.Log("   - Badge: " + Badge);
                    }
                    this.roleBadges.Add(Role, tmpList);

                    //ObjectTree.Application.GUI.logSacredText(null);
                }

                //Logging.Log("FUSE rights for users with Club subscription:");
                // Club rights
                dTable = Database.getTable("SELECT fuseright FROM users_club_fuserights");
                foreach (DataRow dRow in dTable.Rows)
                {
                    string Right = (string)dRow["fuseright"];
                    this.clubRights.Add(Right);
                    //Logging.Log("- " + Right);
                }
                //ObjectTree.Application.GUI.logSacredText(null);

                Logging.Log("Initialized user roles.");
            }
            else
                Logging.Log("Failed to initialize user roles, database was not contactable!", Logging.logType.commonWarning);
        }
        /// <summary>
        /// Parses a numeric role ID (string) to a value of the userRole enum. userRole.Banned is returned if an invalid value is given.
        /// </summary>
        /// <param name="s">The string object containing the role ID.</param>
        public userRole parseRoleFromString(string s)
        {
            int iRole = 0;
            if (int.TryParse(s, out iRole))
                return (userRole)iRole;
            else
                return userRole.Banned;
        }
        /// <summary>
        /// Returns a string with every fuse right followed by char 2, for a certain user role. The fuserights for users with a club subscription are added if required.
        /// </summary>
        /// <param name="Role">The userRole enum value of the role to get the rights of.</param>
        /// <param name="hasClub">If the user has club subscription, then supply true.</param>
        public string getRightsForRole(userRole Role, bool hasClub)
        {
            StringBuilder ret = new StringBuilder();
            try
            {
                List<string> rolesRights = this.roleRights[Role];
                foreach(string Right in rolesRights)
                {
                    ret.Append(Right);
                    ret.Append("\x02");
                }
                if (hasClub)
                {
                    foreach (string Right in this.clubRights)
                    {
                        ret.Append(Right);
                        ret.Append("\x02");
                    }
                }
            }
            catch {}

            return ret.ToString();
        }
        /// <summary>
        /// Returns a boolean that indicates if a given user role contains a certain fuse right.
        /// </summary>
        /// <param name="Role">The userRole enum value of the role to lookup.</param>
        /// <param name="hasClub">Indicates if to include rights for club subscribers.</param>
        /// <param name="Right">The fuse right to check.</param>
        public bool roleHasRight(userRole Role, bool hasClub, string Right)
        {
            bool Aye = this.roleRights[Role].Contains(Right);
            if (Aye == false && hasClub)
                Aye = this.clubRights.Contains(Right);

            return Aye;
        }
        /// <summary>
        /// Returns a boolean that indicates if a give user role has access to a given badge.
        /// </summary>
        /// <param name="Role">The userRole enum value of the role to lookup.</param>
        /// <param name="Badge">The badge to check.</param>
        public bool roleHasBadge(userRole Role, string Badge)
        {
            return this.roleBadges[Role].Contains(Badge);
        }
        public List<string> getDefaultBadgesForRole(userRole Role)
        {
            return this.roleBadges[Role];
        }
        #endregion
    }
}
