using Common.Generic;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Common.Strings
{
    /// <summary>
    /// Utilities methods dealing with strings.
    /// </summary>
    public static class Helper
    {
        #region DateTime formated to string
        /// <summary>
        /// Gets the current date year and week.
        /// </summary>
        /// <returns>Something like 2016-Week35</returns>
        public static string GetCurrentDateYearAndWeek()
        {
            var dateTime = DateTime.UtcNow;
            var dfi = DateTimeFormatInfo.CurrentInfo;
            int weekNr = dfi.Calendar.GetWeekOfYear(dateTime, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
            return $"{dateTime.Year}-Week{weekNr}";
        }

        /// <summary>
        /// Gets the current date as string compatible for a file name, <seealso cref="ToFileNameString(DateTime)"/>.
        /// </summary>
        /// <returns>
        /// The current date file name string.
        /// </returns>
        public static string GetCurrentDateFileNameString()
        {
            return DateTime.Now.ToFileNameString();
        }

        /// <summary>
        /// Format the date as yyyy-MM-ddTHH-mm.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>A string representation of the date</returns>
        public static string ToFileNameString(this DateTime date)
        {
            return date.ToString("yyyy-MM-ddTHH-mm");
        }

        /// <summary>
        /// Format the date as yyyy-MM-ddTHH-mm.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>A string representation of the date</returns>
        public static string ToFileNameString(this DateTime? date)
        {
            return date.HasValue ? date.Value.ToFileNameString() : string.Empty;
        }

        /// <summary>
        /// Format the date as yyyy-MM-dd.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>A string representation of the date</returns>
        public static string ToShortDateStringFixedYearFirst(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// Format the date as yyyy-MM-dd, i.e. year first, easier for filename or any alphanumeric sort.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>A string representation of the date</returns>
        public static string ToShortDateStringFixedYearFirst(this DateTime? date)
        {
            return date.HasValue ? date.Value.ToShortDateStringFixedYearFirst() : string.Empty;
        }

        /// <summary>
        /// Format the date only, as defined by the regional settings, day or month first.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>A string representation of the date</returns>
        public static string ToShortDateString(this DateTime? date)
        {
            return date.HasValue ? date.Value.ToShortDateString() : string.Empty;
        }


        /// <summary>
        /// Format the date only, as defined by the given culture info.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="cultureInfo">The culture information, e.g. de-CH</param>
        /// <returns>
        /// A string representation of the date
        /// </returns>
        public static string ToShortDateString(this DateTime date, string cultureInfo)
        {
            return date.ToString("d", GetProviderFromCultureInfo(cultureInfo));
        }

        /// <summary>
        /// Gets the provider from culture information.
        /// </summary>
        /// <param name="cultureInfo">The culture information.</param>
        /// <returns></returns>
        public static IFormatProvider GetProviderFromCultureInfo(string cultureInfo)
        {
            IFormatProvider provider;
            if (string.IsNullOrEmpty(cultureInfo))
                provider = CultureInfo.CurrentCulture;
            else
                provider = CultureInfo.GetCultureInfo(cultureInfo);

            return provider;
        }
        #endregion

        #region Base64

        /// <summary>
        /// A string extension method that encodes a text to base64 .
        /// </summary>
        /// <param name="plainText">The plainText to act on.</param>
        /// <returns>
        /// The converted Base64 string.
        /// </returns>
        public static string Base64Encode(this string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// A string extension method that decodes a base64 string to a text.
        /// </summary>
        /// <param name="base64EncodedData">The base64EncodedData to act on.</param>
        /// <returns>
        /// The original text.
        /// </returns>
        public static string Base64Decode(this string base64EncodedData)
        {
            if (string.IsNullOrEmpty(base64EncodedData))
            {
                return string.Empty;
            }
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
        #endregion

        #region TitleCase, Upper first worst, trim, cut

        /// <summary>
        /// A string extension method that transform a string to title case, calling the CurrentCulture.TextInfo.ToTitleCase.
        /// </summary>
        /// <param name="anyCase">The anyCase to act on.</param>
        /// <returns>
        /// AnyCase as a string.
        /// </returns>
        public static string ToTitleCase(this string anyCase)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(anyCase);
        }

        /// <summary>
        /// Return a string with the first word (defined by the separator) is in UPPER case and the other left as is. E.g. Total Recall -> TOTAL Recall
        /// </summary>
        /// <param name="input">The anycase.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>A similar string as the input, but with first word in upper case</returns>
        public static string ToUpperFirstWord(this string input, char separator = ' ')
        {
            char[] stringSeparators = new char[] { separator };
            var array = input.Split(stringSeparators, 2, StringSplitOptions.None);
            if (array.Length == 2) // 2 = max len
            {
                return array[0].ToUpper() + separator.ToString() + array[1]; // standard expected output
            }
            else // array null or empty
            {
                return input.ToUpper();
            }
        }

        /// <summary>
        /// Trim the given string but first check if not null, in which case the input is returned
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The trimed string</returns>
        public static string TrimNullable(this string input)
        {
            return !string.IsNullOrEmpty(input) ? input.Trim() : input;
        }

        /// <summary>
        /// Cuts the string to the given max length.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="maxLength">Length of the max.</param>
        /// <returns>The cutted string</returns>
        public static string CutStringToLength(this string input, int maxLength)
        {
            if (input.Length > maxLength)
            {
                input = input.Substring(0, maxLength);
            }

            return input;
        }
        #endregion

        #region StringCollectionToString, ParseBoolean, AreEquals
        /// <summary>
        /// Get a string from all strings in the collection separated by the separator.
        /// </summary>
        /// <param name="source">The source, e.g. {"aa", "ab", "bb"}.</param>
        /// <param name="separator">The separator, e.g. ", " (default)</param>
        /// <returns>The collection in line, e.g. "aa, ab, bb"</returns>
        public static string StringCollectionToString(IEnumerable<string> source, string separator = ", ")
        {
            return new CommaSeparatedStringDump<string>(separator).DumpListCommaSeparated(source);
        }

        /// <summary>
        /// Parst den String booleanStr um einen Boolean zurückzubekommen.
        /// Der Wert true wird unter folgenden Bedingungen an booleanStr zurückgegeben:
        /// True
        /// erster Buchstabe j oder J
        /// erster Buchstabe y oder Y
        /// 1
        /// </summary>
        /// <param name="booleanStr">String, der für einen boolean Kodiert</param>
        /// <returns>Ermittelter Boolean</returns>
        public static bool ParseBoolean(string booleanStr)
        {
            if (string.IsNullOrEmpty(booleanStr))
            {
                return false;
            }

            bool boolean;
            if (bool.TryParse(booleanStr, out boolean))
            {
                return boolean;
            }

            return booleanStr[0] == 'j' || booleanStr[0] == 'J' ||
                   booleanStr[0] == 'y' || booleanStr[0] == 'Y' ||
                   booleanStr == "1";
        }

        /// <summary>
        /// Compare both string, forcing empty reference as null.
        /// </summary>
        /// <param name="referenceValue">The reference value.</param>
        /// <param name="currentValue">The current value.</param>
        /// <returns><c>True</c> if equals, <c>false</c> otherwise</returns>
        public static bool AreEquals(string referenceValue, string currentValue)
        {
            if (string.IsNullOrEmpty(referenceValue))
                referenceValue = null; // force empty == null
            return currentValue == referenceValue;
        }
        #endregion

        #region Read string from Ressource with Culture
        /// <summary>
        /// Displays the description.
        /// </summary>
        /// <param name="type">Type of the object.</param>
        /// <param name="fieldOrPropertyName">The enum value string.</param>
        /// <param name="language">The language. When set, a LocalizedDescriptionAttribute is required.</param>
        /// <param name="getKeyOnly">if set to <c>true</c> read the LocalizedDescriptionAttribute
        /// but get only the key or name instead of the translated description.</param>
        /// <param name="valueSeparator">The value separator.</param>
        /// <returns>The description or key if required</returns>
        public static string DisplayLocalizedDescription(Type type, string fieldOrPropertyName, string language, bool getKeyOnly, string valueSeparator = ", ")
        {
            string description = string.Empty;
            MemberInfo memberInfo = type.GetMember(fieldOrPropertyName).SingleOrDefault();
            if (memberInfo == null)
            {
                // Try if the enum has combined value
                var enumValueArray = fieldOrPropertyName.Split(','); // | is transformed into , by ToString()
                if (enumValueArray.Length > 1)
                {   // only if many item in array, i.e. if the separator is found in the source string!
                    for (int i = 0; i < enumValueArray.Length; i++)
                    {
                        description += DisplayLocalizedDescription(type, enumValueArray[i].Trim(), language, getKeyOnly, valueSeparator);
                        if (i < (enumValueArray.Length - 1))
                        {
                            description += valueSeparator;
                        }
                    }
                }
            }
            else
            {
                bool hasLanguage = !string.IsNullOrEmpty(language);
                if (hasLanguage || getKeyOnly)
                {
                    var attribute = (LocalizedDescriptionAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(LocalizedDescriptionAttribute));
                    if (attribute != null)
                    {
                        if (getKeyOnly)
                            description = attribute.GetKeyOnly();
                        else // normal case
                            description = attribute.GetDescription(language);
                    }
                    else
                        hasLanguage = false;
                }
                if (!hasLanguage && !getKeyOnly)
                {
                    var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(DescriptionAttribute));
                    if (attribute != null)
                    {
                        description = attribute.Description;
                    }
                }
            }

            if (string.IsNullOrEmpty(description))
                description = fieldOrPropertyName;

            return description;
        }


        /// <summary>
        /// Gets the string from resource. Get the cultureInfo for the given language, and returns the resource string given by the key for that language.
        /// If not found, return the default value
        /// </summary>
        /// <param name="resourceManager">The resource manager.</param>
        /// <param name="resourceKey">The resource key.</param>
        /// <param name="language">The language.</param>
        /// <param name="defaultValue">The default value. If null (default), the resource key will be considered</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns>The string from the resource for the given language</returns>
        public static string GetStringFromResourceForLanguage(this ResourceManager resourceManager, string resourceKey, string language,
            string defaultValue = null, string suffix = null)
        {
            if (string.IsNullOrEmpty(defaultValue))
                defaultValue = resourceKey;
            try
            {
                // Create cache key
                string cachekey = string.IsNullOrEmpty(suffix) ? resourceKey : resourceKey + suffix;
                var tuple = new Tuple<string, string>(cachekey, language);
                // Get from cache
                if (localizationCache != null && localizationCache.ContainsKey(tuple))
                    return localizationCache[tuple];
                // Get the string for the culture!
                CultureInfo culture = CultureInfo.GetCultureInfo(language);
                string displayName = resourceManager.GetString(resourceKey, culture);
                string result = string.IsNullOrEmpty(displayName) ? defaultValue
                    : string.IsNullOrEmpty(suffix) ? displayName : displayName + suffix;

                // Cache result
                if (localizationCache == null)
                    localizationCache = new Dictionary<Tuple<string, string>, string>();
                localizationCache.Add(tuple, result);

                return result;
            }
            catch // any, not only CultureNotFoundException
            {
                return defaultValue;
            }
        }

        private static Dictionary<Tuple<string, string>, string> localizationCache = new Dictionary<Tuple<string, string>, string>();
        #endregion

        #region Format Telephone number
        /// <summary>
        /// Formats the telephone number.
        /// </summary>
        /// <param name="inputTelNumber">The input tel number.</param>
        /// <returns>The formated telephone number</returns>
        public static string FormatTelephoneNumber(this string inputTelNumber)
        {
            string output = inputTelNumber;
            if (!string.IsNullOrEmpty(inputTelNumber))
            {
                // CH: +41 xx xxx xx xx, given from Abacus without space as +41xxxxxxxxx
                if (inputTelNumber.StartsWith("+41") || inputTelNumber.StartsWith("41"))
                {
                    long telLong;
                    if (long.TryParse(inputTelNumber, out telLong))
                    {
                        output = string.Format("+{0:## ## ### ## ##}", telLong);
                    }
                }
            }

            return output;
        }
        #endregion

        #region Log list of string
        /// <summary>
        /// Logs the messages.
        /// </summary>
        /// <param name="messages">The messages.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="loggerLevel">The logger level.</param>
        public static void LogMessages(IEnumerable<string> messages, ILogger logger, LoggerLevel loggerLevel)
        {
            switch (loggerLevel)
            {
                case LoggerLevel.Info:
                    messages.ForEach(msg => logger.WriteInfo(msg));
                    break;
                case LoggerLevel.Warning:
                    messages.ForEach(msg => logger.WriteWarning(msg));
                    break;
                case LoggerLevel.Error:
                    messages.ForEach(msg => logger.WriteError(msg));
                    break;
                default:
                    logger.WriteError($"Wrong enum value {nameof(loggerLevel)}: {loggerLevel}");
                    break;
            }
        }
        #endregion
    }
}
