using System;
using System.Text;
using System.Net;
using System.Collections.Generic;

namespace Woodpecker.Net.Http
{
    /// <summary>
    /// Provides various functions for interacting with the internet via the HTTP protocol.
    /// </summary>
    public class httpProvider
    {
        #region Fields
        private Dictionary<string, string> Parameters = new Dictionary<string, string>();
        #endregion

        #region Methods
        /// <summary>
        /// Adds a parameter to the request.
        /// </summary>
        /// <param name="Parameter">The parameter to add.</param>
        /// <param name="Value">The value for this parameter.</param>
        public void addParameter(string Parameter, string Value)
        {
            if (Parameter.Length > 0)
                this.Parameters.Add(Parameter, Value);
        }
        /// <summary>
        /// Processes the request at a certain URI.
        /// </summary>
        /// <param name="URI">The URI to process the request at.</param>
        public string getResponse(string URI)
        {
            int i = this.Parameters.Count;
            if (i > 0)
            {
                int i2 = 0;
                URI += "?";
                foreach (string Parameter in this.Parameters.Keys)
                {
                    i2++;
                    URI += Parameter + "=" + this.Parameters[Parameter];
                    if (i2 < i)
                        URI += "&";
                }
            }

            try { return new WebClient().DownloadString(URI); }
            catch { return ""; }
        }
        #endregion

    }
}
