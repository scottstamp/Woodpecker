using System;
using System.Text;
using System.Collections.Generic;

namespace Woodpecker.Specialized.Encoding
{
    /// <summary>
    /// Provides 'wire' encoding and decoding for numbers, better known as 'VL64' or 'LV64'. 
    /// </summary>
    public static class wireEncoding
    {
        /// <summary>
        /// Encodes an integer to a VL64 string.
        /// </summary>
        /// <param name="i">The integer to encode.</param>
        public static string Encode(int i)
        {
            try
            {
                byte[] res = new byte[6];
                int p = 0;
                int sP = 0;
                int bytes = 1;
                int negativeMask = i >= 0 ? 0 : 4;

                i = Math.Abs(i);
                res[p++] = (byte)(64 + (i & 3));
                for (i >>= 2; i != 0; i >>= 6)
                {
                    bytes++;
                    res[p++] = (byte)(64 + (i & 0x3f));
                }

                res[sP] = (byte)(res[sP] | bytes << 3 | negativeMask);
                return new ASCIIEncoding().GetString(res).Replace("\0", "");
            }
            catch { return ""; }
        }
        public static string Encode(uint i)
        {
            return Encode((int)i);
        }
        /// <summary>
        /// Encodes each integer in the input array and adds it to the 'wire'. Returns the result as a string.
        /// </summary>
        /// <param name="i">The integer array to wire up the values of.</param>
        public static string Encode(int[] i)
        {
            string s = "";
            foreach (int j in i)
                s += Encode(j);

            return s;
        }
        /// <summary>
        /// Encodes a boolean to a VL64 char.
        /// </summary>
        /// <param name="b">The boolean to encode.</param>
        public static char Encode(bool b)
        {
            if (b)
                return 'I';
            else
                return 'H';
        }
        /// <summary>
        /// Decodes a wire encoded string to the first encoded number in the wire.
        /// </summary>
        /// <param name="s">The write to decode.</param>
        public static int Decode(ref string s)
        {
            try
            {
                char[] raw = s.ToCharArray();
                int pos = 0;
                int i = 0;
                bool negative = (raw[pos] & 4) == 4;
                int totalBytes = raw[pos] >> 3 & 7;
                i = raw[pos] & 3;
                pos++;
                int shiftAmount = 2;
                for (int b = 1; b < totalBytes; b++)
                {
                    i |= (raw[pos] & 0x3f) << shiftAmount;
                    shiftAmount = 2 + 6 * b;
                    pos++;
                }

                if (negative == true)
                    i *= -1;
                return i;
            }
            catch { return 0; }
        }
        public static int Decode(string s)
        {
            return Decode(ref s);
        }
        /// <summary>
        /// Decodes a wire encoded string to a boolean. This is fancy.
        /// </summary>
        /// <param name="s">The wire encoded boolean to decode.</param>
        public static bool decodeBoolean(string s)
        {
            return (s == "I");
        }
        /// <summary>
        /// Decodes a wire encoded char to a boolean. This is fancy.
        /// </summary>
        /// <param name="s">The wire encoded boolean to decode.</param>
        public static bool decodeBoolean(char s)
        {
            return (s == 'I');
        }
        public static int[] decodeWire(string s)
        {
            List<int> Ret = new List<int>();
            try
            {
                while (s.Length > 0)
                {
                    int i = Decode(ref s);
                    Ret.Add(i);
                    s = s.Substring(Encode(i).Length);
                }
            }
            catch { }

            return Ret.ToArray();
        }
        public static string[] getMixedParameters(string s)
        {
            List<string> Ret = new List<string>();
            while (s.Length > 0)
            {
                int len = 0;
                string sVar = "";
                if (s[0] == '@')
                {
                    len = base64Encoding.Decode(s.Substring(0, 2));
                    sVar = s.Substring(2, len);
                    len += 2;
                }
                else
                {
                    int w = wireEncoding.Decode(ref s);
                    sVar = w.ToString();
                    len = wireEncoding.Encode(w).Length;
                }
                s = s.Substring(len);
                Ret.Add(sVar);
            }

            return Ret.ToArray();
        }
    }
}
