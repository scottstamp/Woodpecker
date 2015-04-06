using System;
using System.Collections.Generic;
using System.Data;

using Woodpecker.Storage;
using Woodpecker.Specialized.Text;

using Woodpecker.Game;
using Woodpecker.Net.Game;
using Woodpecker.Game.Users.Roles;
using Woodpecker.Game.Messenger;

namespace Woodpecker.Game.Users
{
    public class userReactor : Reactor
    {
        /// <summary>
        /// 7 - "@G"
        /// </summary>
        public void GET_INFO()
        {
            Response.Initialize(5); // "@E"
            Response.Append(Session.User.ToString());
            sendResponse();
        }
        /// <summary>
        /// 8 - "@H"
        /// </summary>
        public void GET_CREDITS()
        {
            Response.Initialize(6); // "@F"
            Response.Append(Session.User.Credits);
            Response.Append(".0");
            sendResponse();
        }
        /// <summary>
        /// 12 - "@L"
        /// </summary>
        public void MESSENGERINIT()
        {
            messengerReactor Messenger = new messengerReactor();
            Session.gameConnection.reactorHandler.Register(Messenger);
            Messenger.initializeBuddyList();
        }
        /// <summary>
        /// 26 - "@Z"
        /// </summary>
        public void SCR_GET_USER_INFO()
        {
            Session.refreshClubStatus();
        }

        /// <summary>
        /// 44 - "@l"
        /// </summary>
        public void UPDATE()
        {
            string workArg = Request.getStructuredParameter(4); // Figure
            if (workArg.Length > 0)
            {
                if (workArg.Length == 25 && stringFunctions.isNumeric(workArg))
                    Session.User.Figure = workArg;
            }

            workArg = Request.getStructuredParameter(5); // Sex
            if (workArg.Length > 0)
            {
                if (workArg != "M")
                    Session.User.Sex = 'F';
                else
                    Session.User.Sex = 'M';
            }

            workArg = Request.getStructuredParameter(6); // Motto
            if (workArg.Length > 0)
            {
                stringFunctions.filterVulnerableStuff(ref workArg, true);
                Session.User.Motto = workArg;
            }

            Response.Initialize(5); // "@E"
            Response.Append(Session.User.ToString());
            sendResponse();

            Session.User.updateAppearanceDetails();
        }
        /// <summary>
        /// 149 - "BU"
        /// </summary>
        public void UPDATE_ACCOUNT()
        {
            Response.Initialize(169); // "Bi"
            int errorID = 0; // OK

            if (Engine.Security.Cryptography.MD5.Hash(Request.getStructuredParameter(13), Session.User.Username.ToLower()) != Session.User.Password)
                errorID = 1; // Password's do not match
            else
            {
                string myDOB = Request.getStructuredParameter(8);
                if (myDOB != Session.User.DateOfBirth)
                    errorID = 2; // Dates of birth do not match
                else
                {
                    string newValue = Request.getStructuredParameter(3);
                    if (newValue.Length > 0) // New password set
                    {
                        if (!stringFunctions.passwordIsValid(Session.User.Username, newValue)) // Client should have verified this
                            return;

                        Session.User.Password = Engine.Security.Cryptography.MD5.Hash(newValue, Session.User.Username.ToLower());
                    }
                    else // New email, re-get the new value
                    {
                        newValue = Request.getStructuredParameter(7);
                        if (!stringFunctions.emailIsValid(newValue)) // Client should have verified this (not that we actually care)
                            return;

                        Session.User.Email = newValue;
                    }

                    Session.User.updatePersonalDetails();
                }
            }

            Response.Append(errorID);
            sendResponse();
        }

        /// <summary>
        /// 157 - "B]"
        /// </summary>
        public void GETAVAILABLEBADGES()
        {
            Session.refreshBadgeList();
        }
    }
}
