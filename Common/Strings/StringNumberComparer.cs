using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Common.Strings
{
    /// <summary>
    /// Compares StringNumberElements
    /// </summary>
    public class StringNumberComparer : IComparer<string>, System.Collections.IComparer
    {
        /// <summary>
        /// Sort order of the column. ascending or descending.
        /// </summary>
        private ListSortDirection sortOrder;

        /// <summary>
        /// Initializes a new instance of the  class.
        /// </summary>
        /// <param name="sortOrder">The sort order.</param>
        public StringNumberComparer(ListSortDirection sortOrder)
        {
            this.sortOrder = sortOrder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringNumberComparer"/> class.
        /// </summary>
        public StringNumberComparer()
        {
        }

        #region IComparer<string> Members

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value
        /// Condition
        /// Less than zero
        /// <paramref name="x"/> is less than <paramref name="y"/>.
        /// Zero
        /// <paramref name="x"/> equals <paramref name="y"/>.
        /// Greater than zero
        /// <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        /// Neither <paramref name="x"/> nor <paramref name="y"/> implements the <see cref="T:System.IComparable"/> interface.
        /// -or-
        /// <paramref name="x"/> and <paramref name="y"/> are of different types and neither one can handle comparisons with the other.
        /// </exception>
        public int Compare(string x, string y)
        {
            if (x == null)
            {
                return sortOrder == ListSortDirection.Descending ? 1 : -1;
            }

            if (y == null)
            {
                return sortOrder == ListSortDirection.Descending ? -1 : 1;
            }

            string xValue = x; // ((System.Windows.Forms.DataGridViewRow)x).Cells[_columnToSort].Value;
            string yValue = y; // ((System.Windows.Forms.DataGridViewRow)y).Cells[_columnToSort].Value;

            if (xValue == yValue)
            {
                return 0;
            }

            int len1 = xValue.Length;
            int len2 = yValue.Length;
            int marker1 = 0;
            int marker2 = 0;

            // Walk through two the strings with two markers.
            while (marker1 < len1 && marker2 < len2)
            {
                char ch1 = xValue[marker1];
                char ch2 = yValue[marker2];

                // Some buffers we can build up characters in for each chunk.
                char[] space1 = new char[len1];
                int loc1 = 0;
                char[] space2 = new char[len2];
                int loc2 = 0;

                // Walk through all following characters that are digits or
                // characters in BOTH strings starting at the appropriate marker.
                // Collect char arrays.
                do
                {
                    space1[loc1++] = ch1;
                    marker1++;

                    if (marker1 < len1)
                    {
                        ch1 = xValue[marker1];
                    }
                    else
                    {
                        break;
                    }
                }
                while (char.IsDigit(ch1) == char.IsDigit(space1[0]));

                do
                {
                    space2[loc2++] = ch2;
                    marker2++;

                    if (marker2 < len2)
                    {
                        ch2 = yValue[marker2];
                    }
                    else
                    {
                        break;
                    }
                }
                while (char.IsDigit(ch2) == char.IsDigit(space2[0]));

                // If we have collected numbers, compare them numerically.
                // Otherwise, if we have strings, compare them alphabetically.
                string str1 = new string(space1);
                string str2 = new string(space2);

                int result;

                if (char.IsDigit(space1[0]) && char.IsDigit(space2[0]))
                {
                    int thisNumericChunk = int.Parse(str1);
                    int thatNumericChunk = int.Parse(str2);
                    result = thisNumericChunk.CompareTo(thatNumericChunk);
                }
                else
                {
                    result = str1.CompareTo(str2);
                }

                if (result != 0)
                {
                    return result;
                }
            }

            return sortOrder == ListSortDirection.Descending ? len2 - len1 : len1 - len2;
        }

        #endregion

        #region IComparer Members

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value Condition Less than zero <paramref name="x"/> is less than <paramref name="y"/>. Zero <paramref name="x"/> equals <paramref name="y"/>. Greater than zero <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">Neither <paramref name="x"/> nor <paramref name="y"/> implements the <see cref="T:System.IComparable"/> interface.-or- <paramref name="x"/> and <paramref name="y"/> are of different types and neither one can handle comparisons with the other. </exception>
        /// <exception cref="ArgumentNullException">Argument is null.</exception>
        /// <exception cref="ArgumentException">Only comparison of string values.</exception>
        public int Compare(object x, object y)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));

            if (y == null)
                throw new ArgumentNullException(nameof(y));

            if (x.GetType() != typeof(string) || y.GetType() != typeof(string))
                throw new ArgumentException("Only comparison of string values.");

            return Compare((string)x, (string)y);
        }

        #endregion
    }
}
