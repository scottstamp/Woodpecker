using System;
using System.Data;
using System.Collections;

using MySql.Data.MySqlClient;
using Woodpecker.Storage;

using Woodpecker.Specialized.Text;
using Woodpecker.Sessions;
using Woodpecker.Net.Game.Messages;

namespace Woodpecker.Game
{
    public class globalReactor : Reactor
    {
        /// <summary>
        /// 9 - "@I"
        /// </summary>
        public void GETAVAILABLESETS()
        {
            Session.refreshFigureParts();
        }
        /// <summary>
        /// 42 - "@j"
        /// </summary>
        public void APPROVENAME()
        {
            Response.Initialize(36); // "@d"

            bool isPet = (Request.Content[Request.Content.Length - 1] == 'I');
            int errorID = Engine.Game.Users.getNameCheckError(isPet, Request.getParameter(0));
            Response.appendWired(errorID);
            sendResponse();
        }
        /// <summary>
        /// 49 - "@q"
        /// </summary>
        public void GDATE()
        {
            Response.Initialize(163); // "Bc"
            Response.Append(DateTime.Today.ToString("dd-MM-yyyy"));
            sendResponse();
        }
        /// <summary>
        /// 196 - "CD"
        /// </summary>
        public void PONG()
        {
            Session.pongReceived = true;
        }
        /// <summary>
        /// 197 - "CE"
        /// </summary>
        public void APPROVEEMAIL()
        {
            Response.Initialize(271); // "DO"
            sendResponse();
        }
        /// <summary>
        /// 203 - "CK"
        /// </summary>
        public void APPROVE_PASSWORD()
        {
            Response.Initialize(282); // "DZ"
            string Username = Request.getParameter(0);
            string Password = Request.getParameter(1);
            bool OK = stringFunctions.passwordIsValid(Username, Password);
            Response.appendWired(!OK);
            sendResponse();
        }

        /// <summary>
        /// 86 - "AV"
        /// </summary>
        public void CRYFORHELP()
        {
            string[] args = Request.getMixedParameters();
            Database Database = new Database(false, false);
            Database.addParameterWithValue("userid", Session.User.ID);
            Database.addParameterWithValue("senderip", Session.ipAddress);
            Database.addParameterWithValue("msg", args[0]);
            Database.addParameterWithValue("roomid", Session.roomID);
            Database.addParameterWithValue("ip", Session.ipAddress);
            Database.addParameterWithValue("sended", DateTime.Now.ToString());
            Database.Open();
            string row = "";
            string room = "";
            if (Database.Ready)
            {
                Database.runQuery("INSERT INTO callforhelp (id, sended, uid, rid, date, message, send_ip, pickedup, pickedup_person, pickedup_ip, pickedup_date) VALUES ('', @sended, @userid, @roomid, @msg, '" + DateTime.Now.ToString() + "', @senderip, '0', '', '', '')");
                row = Database.getString("SELECT id FROM `callforhelp` ORDER BY `callforhelp`.`id` DESC LIMIT 1");
                room = Database.getString("SELECT roomname FROM `rooms` WHERE `id` = '" + Session.roomID + "' LIMIT 1");
                Database.Close();
            }

            ArrayList list = Engine.Game.Users.UserFuses("fuse_receive_calls_for_help");
            foreach (int x in list)
            {
                Session S = Engine.Game.Users.getUserSession(x);
                serverMessage Message = new serverMessage();
                Message.Initialize(148);
                Message.Append(row);
                Message.appendChar(2);
                Message.Append("ISent: ");
                Message.Append(DateTime.Now.ToString());
                Message.appendChar(2);
                Message.Append(Session.User.Username);
                Message.appendChar(2);
                Message.Append(args[0]);
                Message.appendChar(2);
                Message.Append(Specialized.Encoding.wireEncoding.Encode(Session.roomID));
                Message.appendChar(2);
                Message.Append("Room: " + room);
                Message.appendChar(2);
                Message.Append("I");
                Message.appendChar(2);
                Message.Append(Specialized.Encoding.wireEncoding.Encode(Session.roomID));
                S.gameConnection.sendMessage(Message);
            }
        }

        /// <summary>
        /// 48 - "@p"
        /// </summary>
        public void PICK_CRYFORHELP()
        {
            string[] args = Request.getMixedParameters();
            if (Session.User.hasFuseRight("fuse_receive_calls_for_help"))
            {
                string username = Session.User.Username;
                Database Database = new Database(false, false);
                Database.addParameterWithValue("id", args[0]);
                Database.Open();
                if (Database.Ready)
                {
                    DataRow dRow = Database.getRow("SELECT * FROM callforhelp WHERE id = @id");
                    if (dRow != null)
                    {
                        string room = Database.getString("SELECT roomname FROM `rooms` WHERE `id` = '" + dRow["rid"] + "' LIMIT 1");
                        if (dRow["pickedup"].ToString() == "1")
                        {
                            Response.Initialize(139);
                            Response.Append("This call has already been picked up by " + Engine.Game.Users.getUsername(Convert.ToInt32(dRow["pickedup_person"])) + ".");
                            sendResponse();
                        }
                        else
                        {
                            Database.runQuery("UPDATE callforhelp SET pickedup = '1', pickedup_person = '" + Session.User.ID + "', pickedup_ip = '" + Session.ipAddress + "', pickedup_date = '" + DateTime.Now.ToString() + "' WHERE id = @id");

                            ArrayList list = Engine.Game.Users.UserFuses("fuse_receive_calls_for_help");
                            foreach (int x in list)
                            {
                                Session S = Engine.Game.Users.getUserSession(x);
                                serverMessage Message = new serverMessage();
                                Message.Initialize(148);
                                Message.Append(args[0]);
                                Message.appendChar(2);
                                Message.Append("IPicked up on: ");
                                Message.Append(DateTime.Now.ToString());
                                Message.appendChar(2);
                                Message.Append(Session.User.Username);
                                Message.appendChar(2);
                                Message.Append(dRow["message"].ToString());
                                Message.appendChar(2);
                                Message.Append(Specialized.Encoding.wireEncoding.Encode(Convert.ToInt32(dRow["rid"])));
                                Message.appendChar(2);
                                Message.Append("(Picked up by " + username + ") Room: " + room);
                                Message.appendChar(2);
                                Message.Append("I");
                                Message.appendChar(2);
                                Message.Append(Specialized.Encoding.wireEncoding.Encode(Convert.ToInt32(dRow["rid"])));
                                sendResponse();
                                S.gameConnection.sendMessage(Message);
                            }
                        }
                    }

                    Database.Close();
                }

            }
        }

        /// <summary>
        /// 48 - "@p"
        /// </summary>
        public void CRYFORHELP_PICKUPGO()
        {
            string[] args = Request.getMixedParameters();
            if (Session.User.hasFuseRight("fuse_receive_calls_for_help"))
            {
                Database Database = new Database(false, false);
                Database.addParameterWithValue("id", args[0]);
                Database.Open();
                if (Database.Ready)
                {
                    DataRow dRow = Database.getRow("SELECT * FROM callforhelp WHERE id = @id");
                    if (dRow != null)
                    {
                        string publicroom = Database.getString("SELECT publicroom FROM `rooms` WHERE `id` = '" + dRow["rid"] + "' LIMIT 1");


                        Response.Initialize(286);
                        if (publicroom == "1")
                        {
                            Response.Append("I");
                        }
                        else
                        {
                            Response.Append("H");
                        }
                        Response.appendWired(Convert.ToInt32(dRow["rid"]));
                        sendResponse();
                    }
                }
                Database.Close();

            }
        }

        /// <summary>
        /// 199 - "CG"
        /// </summary>
        public void MESSAGETOCALLER()
        {
            string[] args = Request.getMixedParameters();
            if (Session.User.hasFuseRight("fuse_receive_calls_for_help"))
            {
                Database Database = new Database(false, false);
                Database.addParameterWithValue("id", args[0]);
                Database.addParameterWithValue("msg", args[1]);
                Database.Open();
                if (Database.Ready)
                {
                    DataRow dRow = Database.getRow("SELECT * FROM callforhelp WHERE id = @id");
                    if (dRow != null)
                    {
                        Session S = Engine.Game.Users.getUserSession(Convert.ToInt32(dRow["uid"]));
                        serverMessage Message = new serverMessage();
                        Message.Initialize(274);
                        Message.Append(args[1]);
                        Message.appendChar(2);
                        S.gameConnection.sendMessage(Message);
                        Database.runQuery("INSERT INTO callforhelp_msg VALUES ('', @id, @msg, '" + Session.User.ID + "', '" + Session.ipAddress + "', '" + DateTime.Now.ToString() + "')");
                    }
                }
            }
        }
        /// <summary>
        /// 198 - "CF"
        /// </summary>
        public void CHANGECALLCATEGORY()
        {
            string[] args = Request.getMixedParameters();
            if (Session.User.hasFuseRight("fuse_receive_calls_for_help"))
            {
                //verstuur naar hobba's
                Response.Initialize(139);
                Response.Append("This is not working yet");
                sendResponse();
            }
        }
    }
}
