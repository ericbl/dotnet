using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Strings
{
    /// <summary>
    /// Transform a list of entries to a comma separated string.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    public class CommaSeparatedStringDump<T>
    {
        #region fields

        /// <summary>
        /// Separator between the entries
        /// </summary>
        private readonly string separator = string.Format(",{0}", Environment.NewLine);

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CommaSeparatedStringDump{T}"/> class.
        /// </summary>
        public CommaSeparatedStringDump()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommaSeparatedStringDump{T}"/> class.
        /// </summary>
        /// <param name="customSeparator">The custom separator.</param>
        public CommaSeparatedStringDump(string customSeparator)
        {
            separator = customSeparator;
        }

        #endregion

        /// <summary>
        /// Dumps a list to a comma separated string using the default ToString() conversion.
        /// </summary>
        /// <param name="list">     The list.</param>
        /// <param name="maxLength">(Optional) The maximum length of the string: once reached, the loop is broken, and append ...</param>
        /// <returns>
        /// A string.
        /// </returns>
        public string DumpListCommaSeparated(IEnumerable<T> list, int maxLength = int.MaxValue)
        {
            return DumpListCommaSeparated(list, null, maxLength);
        }

        /// <summary>
        /// Dumps a list to a comma separated string.
        /// </summary>
        /// <param name="list">     The list.</param>
        /// <param name="toString">The custom function to transform an item to a string. If null, call the default ToString() object method.</param>
        /// <param name="maxLength">(Optional) The maximum length of the string: once reached, the loop is broken, and append ...</param>
        /// <returns>
        /// A string.
        /// </returns>
        public string DumpListCommaSeparated(IEnumerable<T> list, Func<T, string> toString, int maxLength = int.MaxValue)
        {
            // Use default ToString() if no custom specified
            if (toString == null)
            {
                toString = item => item.ToString();
            }

            bool first = true;
            StringBuilder sb = new StringBuilder();
            foreach (T item in list)
            {
                if (first)
                {
                    first = false; // no separator for first!
                }
                else if (sb.Length < maxLength)
                {
                    sb.AppendFormat(separator);
                }

                if (sb.Length < maxLength)
                {
                    sb.Append(toString(item));
                }
                else
                {
                    sb.Append("...");
                    break;
                }
            }

            return sb.ToString();
        }
    }
}
