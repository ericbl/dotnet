using System;
using System.Globalization;

namespace Common.Generic
{
    /// <summary>
    /// Parser for string, trying to convert string to a given type.
    /// </summary>
    public static class TypeParser
    {
        /// <summary>
        /// Delegate for T.TryParse method
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="s">The string.</param>
        /// <param name="result">The result.</param>
        /// <returns>True if successfully parsed</returns>
        public delegate bool TryParseDelegate<T>(string s, out T result) 
            where T : struct;

        /// <summary>
        /// Launch the tryParse action and set as nullable.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="result">The result.</param>
        /// <param name="tryParse">The try parse.</param>
        /// <returns><c>true</c> if successfully parsed</returns>
        public static bool TryParseNullable<T>(string value, out T? result, TryParseDelegate<T> tryParse) 
            where T : struct
        {
            if (!string.IsNullOrEmpty(value))
            {
                T temp;
                if (tryParse(value, out temp))
                {
                    result = temp;
                    return true;
                }
            }

            result = null; // default(T);
            return false;
        }

        /// <summary>
        /// Attempts to parse a structure from the given data, returning a default value rather than throwing an exception if it fails.
        /// </summary>
        /// <exception cref="InvalidCastException">Thrown when an object cannot be cast to a required type.</exception>
        /// <exception cref="FormatException">     Thrown when the format of the parsed object is incorrect.</exception>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="input">                                       The input.</param>
        /// <param name="cultureInfo">                                 (Optional) Information describing the culture.</param>
        /// <param name="ignoreInvalidCastExceptionAndFormatException">(Optional) True to ignore invalid cast exception and format exception.</param>
        /// <returns>
        /// The converted null-able object
        /// </returns>
        public static T? TryParseStruct<T>(string input, IFormatProvider cultureInfo = null, bool ignoreInvalidCastExceptionAndFormatException = false) 
            where T : struct
        {
            if (string.IsNullOrEmpty(input))
            {
                return default(T);
            }

            T? result = new T?();
            try
            {
                IConvertible convertibleString = input;
                if (cultureInfo == null)
                {
                    cultureInfo = CultureInfo.CurrentCulture;
                }

                result = new T?((T)convertibleString.ToType(typeof(T), cultureInfo));
            }
            catch (InvalidCastException)
            {
                if (!ignoreInvalidCastExceptionAndFormatException)
                {
                    throw;
                }
            }
            catch (FormatException)
            {
                if (!ignoreInvalidCastExceptionAndFormatException)
                {
                    throw;
                }
            }

            return result;
        }
    }
}
