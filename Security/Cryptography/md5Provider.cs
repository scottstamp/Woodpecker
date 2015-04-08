using System;
using System.Text;
using System.Security.Cryptography;

using Woodpecker.Core;

namespace Woodpecker.Security.Cryptography
{
    /// <summary>
    /// Provides MD5 hashing functions.
    /// </summary>
    public class md5Provider
    {
        #region Fields
        /// <summary>
        /// The base salt to use.
        /// </summary>
        public string baseSalt;

        MD5CryptoServiceProvider provider;
        #endregion

        #region Methods
        /// <summary>
        /// Returns the 32-characters hash of a string. The string is hashed using MD5, with a basesalt and a unique salt.
        /// </summary>
        /// <param name="Input">The input string to hash.</param>
        /// <param name="partialSalt">The additional salt to use. The total input data will be Input + basesalt + partialSalt.</param>
        public string Hash(string Input, string partialSalt)
        {
            provider = new MD5CryptoServiceProvider();

            string szData = Input + this.baseSalt + partialSalt;
            byte[] workData = new UTF7Encoding().GetBytes(szData);
            workData = provider.ComputeHash(workData);
            provider.Dispose();

            StringBuilder sb = new StringBuilder(32);
            foreach (byte b in workData)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
        public string Hash2(string Input, string partialSalt)
        {
            provider = new MD5CryptoServiceProvider();

            string szData = Input + this.baseSalt + partialSalt;
            byte[] workData = new UTF7Encoding().GetBytes(szData);
            workData = provider.ComputeHash(workData);
            provider.Dispose();
            
            StringBuilder sb = new StringBuilder(32);
            foreach (byte b in workData)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
        public string rawHash(ref string Input)
        {
            provider = new MD5CryptoServiceProvider();

            byte[] workData = Configuration.charTable.GetBytes(Input);
            workData = provider.ComputeHash(workData);
            provider.Dispose();

            return Configuration.charTable.GetString(workData);
        }
        #endregion
    }
}
