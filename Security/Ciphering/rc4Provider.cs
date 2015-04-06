using System;
using System.Text;

namespace Woodpecker.Security.Ciphering
{
    /// <summary>
    /// Provides data ciphering with RC4.
    /// </summary>
    public class rc4Provider : rc4Core
    {
        #region Fields
        /// <summary>
        /// Multi-user integer.
        /// </summary>
        private int i;
        /// <summary>
        /// Multi-use integer.
        /// </summary>
        private int j;

        private int[] key = new int[256];
        private int[] table = new int[256];
        #endregion

        #region Constructors
        public rc4Provider(string publicKey)
        {
            // Reset work integers
            this.i = 0;
            this.j = 0;

            string decodedKey = this.decodeKey(publicKey); // Decode public key
            this.Initialize(decodedKey); // Initialize work integers, tables etc
            this.premixTable(this.premixString); // Premix tables
        }
        #endregion

        #region Methods
        #region Initializing & premix
        private void Initialize(string Key)
        {
            int keyValue = int.Parse(Key);
            int keyLength = (keyValue & 0xf8) / 8;
            if (keyLength < 20)
                keyLength += 20;
            int keyOffset = keyValue % keyWindow.Length;
            int tGiven = keyValue;
            int tOwn = 0;

            int[] w = new int[keyLength];

            for (int a = 0; a < keyLength; a++)
            {
                tOwn = keyWindow[Math.Abs((keyOffset + a) % keyWindow.Length)];
                w[a] = Math.Abs(tGiven ^ tOwn);
                if (a == 31)
                    tGiven = keyValue;
                else
                    tGiven = (tGiven / 2);
            }

            for (int b = 0; b < 256; b++)
            {
                key[b] = w[b % w.Length];
                table[b] = b;
            }

            int t = 0;
            int u = 0;
            for (int a = 0; a < 256; a++)
            {
                u = (int)((u + table[a] + key[a]) % 256);
                t = table[a];
                table[a] = table[u];
                table[u] = t;
            }
        }
        private void premixTable(string s)
        {
            for (int a = 0; a < 17; a++)
            {
                this.Encipher(s);
            }
        }
        #endregion

        #region Ciphering
        public string Encipher(string s)
        {
            StringBuilder Ret = new StringBuilder(s.Length * 2);

            int t = 0;
            int k = 0;

            for (int a = 0; a < s.Length; a++)
            {
                i = (i + 1) % 256;
                j = (j + table[i]) % 256;
                t = table[i];
                table[i] = table[j];
                table[j] = t;

                k = table[(table[i] + table[j]) % 256];

                int c = (char)s.Substring(a, 1).ToCharArray()[0] ^ k;

                if (c <= 0)
                    Ret.Append("00");
                else
                {
                    Ret.Append(di[c >> 4 & 0xf]);
                    Ret.Append(di[c & 0xf]);
                }

            }

            return Ret.ToString();
        }
        public string Decipher(string s)
        {
            try
            {
                StringBuilder Ret = new StringBuilder(s.Length);
                int t = 0;
                int k = 0;
                for (int a = 0; a < s.Length; a += 2)
                {
                    i = (i + 1) % 256;
                    j = (j + table[i]) % 256;
                    t = table[i];
                    table[i] = table[j];
                    table[j] = t;
                    k = table[(table[i] + table[j]) % 256];
                    t = System.Convert.ToInt32(this.JavaSubstring(s, a, a + 2), 16);
                    Ret = Ret.Append((char)(t ^ k));
                }

                return Ret.ToString();
            }
            catch { return ""; }
        }
        #endregion
        #endregion
    }
}
