using System;
using System.Data;
using System.Collections.Generic;

using Woodpecker.Core;
using Woodpecker.Storage;

namespace Woodpecker.Game.Externals
{
    /// <summary>
    /// Provides management and caching for external texts.
    /// </summary>
    public class externalManager
    {
        #region Fields
        /// <summary>
        /// A Dictionary (string, string) collection holding the external texts.
        /// </summary>
        private Dictionary<string, string> Texts;
        #endregion

        #region Methods
        /// <summary>
        /// Wipes the current collection of external texts and re-loads them from the database.
        /// </summary>
        public void loadEntries()
        {
            if (this.Texts != null)
                this.Texts.Clear(); // Wipe old entries
            this.Texts = new Dictionary<string, string>();

            Logging.Log("Loading external_texts entries...");
            Database dbClient = new Database(true, true);
            if (!dbClient.Ready)
            {
                Logging.Log("Failed to initialize external_texts! Database was not contactable!", Logging.logType.commonError);
                return;
            }

            foreach (DataRow dEntry in dbClient.getTable("SELECT * FROM external_texts").Rows)
            {
                string extKey = (string)dEntry["extkey"];
                string extValue = (string)dEntry["extvalue"];
                
                this.Texts.Add(extKey, extValue);
            }
            Logging.Log("Loaded " + this.Texts.Count + " external_texts entries.");
        }
        /// <summary>
        /// Returns a text entry for a given key. If the given key is not found in the collection, the key is returned.
        /// </summary>
        /// <param name="Key">The key to retrieve the value of.</param>
        public string getTextEntry(string Key)
        {
            if (this.Texts != null && this.Texts.ContainsKey(Key))
                return this.Texts[Key];
            else
                return Key;
        }
        #endregion
    }
}
