using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Common.Strings
{
    /// <summary>
    /// Filters Special Characters from String to be used as fieldname
    /// </summary>
    public static class StringCharacterFilter
    {
        //Note rudimentary implementation add special characters as needed;

        /// <summary>
        /// A string extension method that remove German Umlaut and convert to ASCII.
        /// </summary>
        /// <param name="str">      The input string.</param>
        /// <param name="keepSpace">(Optional) True to keep space.</param>
        /// <returns>
        /// The converted string.
        /// </returns>
        public static string LatinizeAndConvertToASCII(this string str, bool keepSpace = false)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            str = str.LatinizeGermanCharacters().ConvertWesternEuropeanToASCII();
            return keepSpace ? str : str.RemoveSpace();
        }

        /// <summary>
        /// A string extension method that convert western European (Windows 1251) string to ASCII.
        /// </summary>
        /// <param name="str">The string in Windows 1251 format.</param>
        /// <returns>
        /// The ASCII string.
        /// </returns>
        public static string ConvertWesternEuropeanToASCII(this string str)
        {
            return Encoding.ASCII.GetString(Encoding.GetEncoding(1251).GetBytes(str));
        }

        /// <summary>
        /// Removes the special characters in German, translating Umlaut to the corresponding diphthong.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>String cleaned from special characters</returns>
        public static string LatinizeGermanCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder(str.Length);
            foreach (char c in str)
            {
                switch (c)
                {
                    case 'ä':
                        sb.Append("ae");
                        break;
                    case 'ö':
                        sb.Append("oe");
                        break;
                    case 'ü':
                        sb.Append("ue");
                        break;
                    case 'Ä':
                        sb.Append("Ae");
                        break;
                    case 'Ö':
                        sb.Append("Oe");
                        break;
                    case 'Ü':
                        sb.Append("Ue");
                        break;
                    case 'ß':
                        sb.Append("ss");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Removes the space.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>The input string without space</returns>
        public static string RemoveSpace(this string str)
        {
            return str.Remove(" ");
        }

        /// <summary>
        /// Removes the specified string: replace it with empty string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="stringToRemove">The string to remove.</param>
        /// <returns>The input string without the string to remove</returns>
        public static string Remove(this string str, string stringToRemove)
        {
            return str.Replace(stringToRemove, string.Empty);
        }

        /// <summary>
        /// Removes the whitespace.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The string without whitespace</returns>
        public static string RemoveWhitespace(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            var sb = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                if (!char.IsWhiteSpace(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Replaces the space by underscore.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>The input string where space got replaced by an underscore</returns>
        public static string ReplaceSpaceByUnderscore(this string str)
        {
            return str.Replace(" ", "_");
        }

        /// <summary>
        /// The special character for Windows as array
        /// </summary>
        private static readonly char[] spSpecialChar = "~#%&*{}\\:<>?/|\"".ToCharArray();

        /// <summary>
        /// Removes the forbidden characters on share point.
        /// <seealso href="https://support.microsoft.com/en-us/help/905231/information-about-the-characters-that-you-cannot-use-in-site-names,-folder-names,-and-file-names-in-sharepoint"/>
        /// </summary>
        /// <param name="spText">The special text.</param>
        /// <param name="replaceChar">The replacement character.</param>
        /// <returns>The input string cleaned from special characters</returns>
        public static string RemoveOrReplaceSpecialCharactersSharePoint(this string spText, char? replaceChar = null)
        {
            return RemoveOrReplaceSpecialCharacters(spText, spSpecialChar, replaceChar);
        }

        /// <summary>
        /// A string extension method that query if 'str' contains any special character.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <returns>
        /// True if it succeeds, false if it fails.
        /// </returns>
        public static bool ContainsAnySpecialChar(this string str)
        {
            return str.IndexOfAny(spSpecialChar) >= 0;
        }

        /// <summary>
        /// Removes the special characters in a string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="specialCharacters">The special characters.</param>
        /// <param name="replaceChar">The replacement character.</param>
        /// <returns>The cleaned input string without the special characters</returns>
        public static string RemoveOrReplaceSpecialCharacters(this string str, char[] specialCharacters, char? replaceChar = null)
        {
            if (!str.ContainsAnySpecialChar())
                return str;
            StringBuilder sb = new StringBuilder(str.Length);
            foreach (char c in str)
            {
                if (!specialCharacters.Contains(c))
                    sb.Append(c);
                else if (replaceChar.HasValue)
                    sb.Append(replaceChar.Value);
            }
            return sb.ToString();

            // Using Except method created strange behavior, where duplicate are ignored and further issue.
            // For instance, "Drucker-ID" becomes "DruckeI" when specifying - as single special char.
            //return new string(str.Except(specialCharacters).ToArray());
        }

        /// <summary>
        /// Uses the only is letter or digit.
        /// </summary>
        /// <param name="dirtyString">The dirty string.</param>
        /// <returns>
        /// A string.
        /// </returns>
        public static string UseOnlyIsLetterOrDigit(this string dirtyString)
        {
            return new string(dirtyString.Where(char.IsLetterOrDigit).ToArray());
        }

        /// <summary>
        /// Checks whether both strings contains common characters, ignoring additional characters. They must not be null or empty
        /// </summary>
        /// <param name="string1">String 1 for compare</param>
        /// <param name="string2">String 2 for compare</param>
        /// <returns><c>bool</c> both strings contains common characters; otherwise false</returns>
        public static bool StringsContainsCommonCharacters(string string1, string string2)
        {
            string strLong = (string1.Length > string2.Length) ? string1.ToLower() : string2.ToLower();
            string strShort = (string1.ToLower() == strLong) ? string2.ToLower() : string1.ToLower();

            for (int i = 0; i < strShort.Length; i++)
            {
                string pattern = @"[" + strShort.Substring(i, 1) + "]";
                if (Regex.IsMatch(strLong, pattern))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
