using System;

namespace Common.Extensions
{
    /// <summary>
    /// Extension Methods
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// A DateTime extension method that gets only year and month, with the first day of the month.
        /// </summary>
        /// <param name="date">The date to act on.</param>
        /// <returns>
        /// A DateTime.
        /// </returns>
        public static DateTime DateYearMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }
    }
}
