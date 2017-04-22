using System.Globalization;

namespace Common.Files
{
    /// <summary>
    ///  Define the CSV delimiter.
    /// </summary>
    public static class CSVDelimiter
    {
        /// <summary>
        /// Gets the default delimiter.
        /// </summary>
        /// <value>
        /// The default delimiter.
        /// </value>
        public static char DefaultDelimiter { get { return ';'; } }

        /// <summary>
        /// Gets the delimiter character from current culture.
        /// </summary>
        /// <value>
        /// The delimiter character from current culture.
        /// </value>
        public static char DelimiterCharFromCurrentCulture { get { return GetDelimiterCharFromCulture(CultureInfo.CurrentCulture); } }

        /// <summary>
        /// Gets the delimiter string from current culture.
        /// </summary>
        /// <value>
        /// The delimiter string from current culture.
        /// </value>
        public static string DelimiterStrFromCurrentCulture { get { return GetDelimiterFromCulture(CultureInfo.CurrentCulture); } }

        /// <summary>
        /// Gets the delimiter string from ch-de culture .
        /// </summary>
        /// <value>
        /// The delimiter string from culture ch-de.
        /// </value>
        public static string DelimiterStrFromCultureChDe { get { return GetDelimiterFromCulture(CultureInfo.GetCultureInfo("ch-DE")); } }

        /// <summary>
        /// Gets delimiter character from culture.
        /// </summary>
        /// <param name="cultureInfo">Information describing the culture.</param>
        /// <returns>
        /// The delimiter character from culture.
        /// </returns>
        public static char GetDelimiterCharFromCulture(CultureInfo cultureInfo)
        {
            return GetDelimiterFromCulture(cultureInfo).ToCharArray()[0];
        }

        /// <summary>
        /// Gets the default list separator from the culture.
        /// </summary>
        /// <param name="cultureInfo">Information describing the culture.</param>
        /// <returns>
        /// The the delimiter from the culture.
        /// </returns>
        public static string GetDelimiterFromCulture(CultureInfo cultureInfo)
        {

            return cultureInfo.TextInfo.ListSeparator;
        }
    }
}
