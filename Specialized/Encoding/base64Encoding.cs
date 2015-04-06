using System;
using System.Collections.Generic;

namespace Woodpecker.Specialized.Encoding
{
    /// <summary>
    /// Provides Base64 encoding and decoding.
    /// </summary>
    public static class base64Encoding
    {
        /// <summary>
        /// Encodes an integer to a Base64 string.
        /// </summary>
        /// <param name="i">The integer to encode.</param>
        public static string Encode(int i)
        {
            try
            {
                string s = "";
                for (int x = 1; x <= 2; x++)
                    s += (char)((byte)(64 + (i >> 6 * (2 - x) & 0x3f)));

                return s;
            }
            catch { return ""; }
        }
        /// <summary>
        /// Decodes a Base64 string to to an integer.
        /// </summary>
        /// <param name="s">The string to decode.</param>
        public static int Decode(string s)
        {
            char[] val = s.ToCharArray();
            try
            {
                int intTot = 0;
                int y = 0;
                for (int x = (val.Length - 1); x >= 0; x--)
                {
                    int intTmp = (int)(byte)((val[x] - 64));
                    if (y > 0)
                        intTmp = intTmp * (int)(Math.Pow(64,y));
                    intTot += intTmp;
                    y++;
                }
                return intTot;
            }
            catch { return -1; }
        }
        /// <summary>
        /// Searches through a given string for the xxth parameter and returns it. If not found, then "" is returned.
        /// </summary>
        /// <param name="s">The string to search through.</param>
        /// <param name="parameterLocation">The parameter to look for, 0 will return the first parameter, 1 the second, 2 the third etc.</param>
        public static string getParameter(string s, int parameterLocation)
        {
            try
            {
                int j = 0;
                while (s.Length > 0)
                {
                    int i = Decode(s.Substring(0, 2));
                    if (j == parameterLocation)
                        return s.Substring(2, i);

                    s = s.Substring(i + 2);
                    j++;
                }
            }
            catch { }

            return "";
        }
        /// <summary>
        /// Searches through a given string for a parameter with a ID and returns it. If not found, then "" is returned.
        /// </summary>
        /// <param name="messageContent">The string to search through.</param>
        /// <param name="paramID">The ID of the parameter to get.</param>
        public static string getStructuredParameter(string s, int paramID)
        {
            try
            {
                int Cycles = 0;
                float maxCyles = s.Length / 4;
                while (Cycles <= maxCyles)
                {
                    int cID = Decode(s.Substring(0, 2));
                    int cLength = Decode(s.Substring(2, 2));
                    if (cID == paramID)
                        return s.Substring(4, cLength);

                    s = s.Substring(cLength + 4);
                }
            }
            catch { }

            return "";
        }
        /// <summary>
        /// Gets all parameters from a string encoded with Base64 headers, and returns it as a string array.
        /// </summary>
        /// <param name="messageContent">The content to get the parameters off.</param>
        public static string[] getParameters(string messageContent)
        {
            List<string> res = new List<string>();
            try
            {
                while (messageContent.Length > 0)
                {
                    int v = Decode(messageContent.Substring(0, 2));
                    res.Add(messageContent.Substring(2, v));
                    messageContent = messageContent.Substring(2 + v);
                }
            }
            catch { }

            return res.ToArray();
        }
    }
}
