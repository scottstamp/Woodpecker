using System;
using System.Data;
using System.Drawing;
using System.Collections.Generic;

using Woodpecker.Storage;

namespace Woodpecker.Game.Items.Bots
{
    /// <summary>
    /// Contains information about a virtual bot. Virtual bots can be purchased by users to be put in their virtual rooms. Bots interact with other room units, demand food and attention etc.
    /// </summary>
    public class virtualBotInformation
    {
        #region Fields
        #region Permanent values
        /// <summary>
        /// The database ID of this virtual bot. The ID is equal to the database ID of the 'nest item' of the bot.
        /// </summary>
        public int ID;
        /// <summary>
        /// The database ID of this virtual bot. The ID is equal to the database ID of the 'nest item' of the bot.
        /// </summary>
        public int roomID;
        /// <summary>
        /// The name of this virtual bot.
        /// </summary>
        public string Name;

        /// <summary>
        /// The figure string of this bot, consisting out of it's type, race and color.
        /// </summary>
        public string Figure
        {
            get
            {
                return this.figureCode;
            }
        }
        #endregion

        #region Dynamical values
        /// <summary>
        /// The minimum Y value in the allowable walking distance for bot.
        /// </summary>
        public byte minX;
        /// <summary>
        /// The minimum Y value in the allowable walking distance for bot.
        /// </summary>
        public byte minY;
        /// <summary>
        /// The maximum Y value in the allowable walking distance for bot.
        /// </summary>
        public byte maxX;
        /// <summary>
        /// The maximum Y value in the allowable walking distance for bot.
        /// </summary>
        public byte maxY;
        /// <summary>
        /// The starting X value in the allowable walking distance for bot.
        /// </summary>
        public byte startX;
        /// <summary>
        /// The starting Y value in the allowable walking distance for bot.
        /// </summary>
        public byte startY;
        /// <summary>
        /// The figure code of the specified bot.
        /// </summary>
        public string figureCode;
        /// <summary>
        /// The default rotation of the specified bot.
        /// </summary>
        public byte rotation;
        /// <summary>
        /// The default rotation of the specified bot.
        /// </summary>
        public Dictionary<uint, String> randomSpeech;
        /// <summary>
        /// Triggers and Responses, as specified in a multidimensional string array.
        /// </summary>
        //public Dictionary<String, String> triggerResponse;
        public List<BotResponse> Responses;
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Parses a System.Data.DataRow with the required fields to a full virtualBotInformation object. Null is returned on errors.
        /// </summary>
        /// <param name="dRow">The System.Data.DataRow object with all the required fields for the parse.</param>
        public static virtualBotInformation Parse(DataRow dRow)
        {
            if (dRow == null)
                return null;

            virtualBotInformation Bot = new virtualBotInformation();
            // Constant values
            Bot.ID = (int)dRow["id"];
            Bot.roomID = (int)dRow["room_id"];
            Bot.Name = (string)dRow["name"];
            Bot.figureCode = dRow["figure"].ToString();

            // Special values
            Bot.startX = byte.Parse(dRow["start_x"].ToString());
            Bot.startY = byte.Parse(dRow["start_y"].ToString());
            Bot.minX = byte.Parse(dRow["min_x"].ToString());
            Bot.minY = byte.Parse(dRow["min_y"].ToString());
            Bot.maxX = byte.Parse(dRow["max_x"].ToString());
            Bot.maxY = byte.Parse(dRow["max_y"].ToString());
            Bot.rotation = byte.Parse(dRow["rot"].ToString());

            try
            {
                Bot.randomSpeech = new Dictionary<uint, string>();
                foreach (String entry in dRow["random_speech"].ToString().Split('|'))
                    if (entry.Contains("~"))
                    {
                        Bot.randomSpeech = new Dictionary<uint, string>();
                        String[] entryData = entry.Split('~');
                        Bot.randomSpeech.Add(uint.Parse(entryData[0]), entryData[1]);
                    }
            }
            catch (Exception ex) { Bot.randomSpeech = null; }
            /*try
            {
                Bot.triggerResponse = new Dictionary<string, string>();
                foreach (String entry in dRow["trigger_response"].ToString().Split('|'))
                    if (entry.Contains("~"))
                    {
                        String[] entryData = entry.Split('~');
                        Bot.triggerResponse.Add(entryData[0], entryData[1]);
                    }
            }
            catch (Exception ex) { Bot.triggerResponse = null; }*/

            Bot.Responses = new List<BotResponse>();

            Database dbClient = new Database(false, true);
            dbClient.addParameterWithValue("id", Bot.ID);
            dbClient.Open();

            if (dbClient.Ready)
            {
                DataTable dTable = dbClient.getTable("SELECT id, bot_id, keywords, response_text, mode, serve_id FROM bots_responses WHERE bot_id = '" + Bot.ID + "'");

                if (dTable != null)
                {
                    foreach (DataRow Row in dTable.Rows)
                    {
                        Bot.Responses.Add(new BotResponse((UInt32)Row["id"], (UInt32)Row["bot_id"], (string)Row["keywords"], (string)Row["response_text"], Row["mode"].ToString(), (int)Row["serve_id"]));
                    }
                }
            }

            return Bot;
        }

        public BotResponse GetResponse(string Message)
        {
            foreach (BotResponse Response in Responses)
            {
                if (Response.KeywordMatched(Message))
                {
                    return Response;
                }
            }

            return null;
        }
        #endregion
    }

    public class BotResponse
    {
        private UInt32 Id;
        public UInt32 BotId;

        public List<string> Keywords;

        public string ResponseText;
        public string ResponseType;

        public int ServeId;

        public BotResponse(UInt32 Id, UInt32 BotId, string Keywords, string ResponseText, string ResponseType, int ServeId)
        {
            this.Id = Id;
            this.BotId = BotId;
            this.Keywords = new List<string>();
            this.ResponseText = ResponseText;
            this.ResponseType = ResponseType;
            this.ServeId = ServeId;

            foreach (string Keyword in Keywords.Split(';'))
            {
                this.Keywords.Add(Keyword.ToLower());
            }
        }

        public bool KeywordMatched(string Message)
        {
            foreach (string Keyword in Keywords)
            {
                if (Message.ToLower().Contains(Keyword.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
