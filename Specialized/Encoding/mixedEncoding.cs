using System;
using System.Collections.Generic;

namespace Woodpecker.Specialized.Encoding
{
    public static class mixedEncoding
    {
        public static string[] getMixedParameters(string s)
        {
            List<string> Ret = new List<string>();
            while (s.Length > 0)
            {
                int len = 0;
                string sVar = "";

                if ((int)s[0] < 72) // Base64
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
        public static int[] getMixedParametersAsIntegers(string s)
        {
            List<int> ret = new List<int>();
            while (s.Length > 0)
            {
                int len = 0;
                int iVar = 0;
                if ((int)s[0] < 72) // Base64
                {
                    len = base64Encoding.Decode(s.Substring(0, 2));
                    iVar = int.Parse(s.Substring(2, len));
                    len += 2;
                }
                else
                {
                    int w = wireEncoding.Decode(ref s);
                    iVar = int.Parse(w.ToString());
                    len = wireEncoding.Encode(w).Length;
                }
                s = s.Substring(len);
                ret.Add(iVar);
            }

            return ret.ToArray();
        }
    }
}
