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
        /// Try to parse the string as value type.
        /// </summary>
        /// <typeparam name="T">Type of the value object (struct)</typeparam>
        /// <param name="input">The input.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="ignoreInvalidCastExceptionAndFormatException">if set to <c>true</c>, ignore the invalid cast and format exception.</param>
        /// <returns>The converted object</returns>
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
