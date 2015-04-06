using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Woodpecker.Specialized.Text
{
    /// <summary>
    /// Provides various functions for the in-game chat.
    /// </summary>
    public static class stringFunctions
    {
        /// <summary>
        /// Returns a boolean that indicates if a certain string contains ' or \.
        /// </summary>
        /// <param name="s">The string to check.</param>
        public static bool containsVulnerableStuff(string s)
        {
            return (false); // char 9 etc
        }
        /// <summary>
        /// Returns a boolean that indicates if a certain string contains at least one number.
        /// </summary>
        /// <param name="s">The string to check.</param>
        public static bool hasNumbers(string s)
        {
            return Regex.IsMatch(s, @"\d+");
        }
        /// <summary>
        /// Returns a boolean that indicates if a certain string contains numbers only.
        /// </summary>
        /// <param name="s">The string to check.</param>
        public static bool isNumeric(string s)
        {
            foreach (char chr in s)
            {
                if (!Char.IsNumber(chr))
                    return false;
            }
            return true;
        }
        public static bool usernameIsValid(string Username)
        {
            const string Allowed = "1234567890qwertyuiopasdfghjklzxcvbnm-+=?!@:.,$";
            foreach (char chr in Username.ToLower())
            {
                if(!Allowed.Contains(chr.ToString()))
                    return false;
            }
            
            return true;
        }
        public static void filterVulnerableStuff(ref string s, bool filterChar13)
        {
            s = s.Replace(Convert.ToChar(2), ' ');
            s = s.Replace(Convert.ToChar(9), ' ');
            s = s.Replace(Convert.ToChar(9), ' ');
            s = s.Replace(Convert.ToChar(10), ' ');
            s = s.Replace(Convert.ToChar(12), ' ');
            if (filterChar13)
                s = s.Replace(Convert.ToChar(13), ' ');

            //return s;
        }
        /// <summary>
        /// Returns a boolean that indicates if an email address is valid.
        /// </summary>
        /// <param name="Email">The email address to check.</param>
        public static bool emailIsValid(string Email)
        {
            try
            {
                // mr.nillus@nillus.net
                string[] Parts = Email.Split('@');
                return (Parts.Length == 2 && Parts[1].Contains("."));
            }
            catch { return false; }
        }
        /// <summary>
        /// Returns a boolean that indicates if a string is valid as a password for a user. The string shouldn't be the same as the username, it should be 6-9 characters uint, it should contain at least one number and no 'stripslashed' characters.
        /// </summary>
        /// <param name="Username">The username for this password.</param>
        /// <param name="Password">The password to check.</param>
        public static bool passwordIsValid(string Username, string Password)
        {
            return (Username != Password && (Password.Length > 5 && Password.Length < 10) && hasNumbers(Password));
        }
        /// <summary>
        /// Garbles a string with a given intensity.
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="Rate"></param>
        public static void garbleText(ref string Text, int Intensity)
        {
            StringBuilder sb = new StringBuilder();
            Random v = new Random(DateTime.Now.Millisecond + Intensity);
            for (int pos = 0; pos < Text.Length; pos++)
            {
                if (v.Next(Intensity, 7) > 3 &&
                    Text[pos] != ' ' &&
                    Text[pos] != ',' &&
                    Text[pos] != '?' &&
                    Text[pos] != '!')
                    sb.Append('.');
                else
                    sb.Append(Text[pos]);
            }

            Text = sb.ToString();
        }
        /// <summary>
        /// Formats a given floating point value to the format 0.00 and returns it as a string.
        /// </summary>
        /// <param name="f">The floating point value to format and return.</param>
        public static string formatFloatForClient(float f)
        {
            return f.ToString("0.00", CultureInfo.InvariantCulture);
        }
    }
}
