using System;
using System.Collections.Generic;
using System.Data;
using Common.Strings;

namespace Common.Server.ImportOleDb
{
    /// <summary>
    /// Conversion tools to read a table of a dataset as a List of T
    /// </summary>
    /// <typeparam name="T">Type of the item in the list</typeparam>
    public class DataTableToList<T>
        where T : IDataTableToList
    {
        /// <summary>
        /// Reads the sheet from the dataset and convert it as a generic list.
        /// </summary>
        /// <param name="dataset">The dataset.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="sheetIndex">Index of the sheet.</param>
        /// <param name="errorLogging">The error logging.</param>
        /// <returns>a list of generic items if read; null otherwise</returns>
        public static IList<T> ReadSheet(DataSet dataset, IFormatProvider formatProvider, int sheetIndex, StringBuilderWithUniqueMsg errorLogging)
        {
            if (dataset != null && dataset.Tables.Count > sheetIndex)
            {
                return ReadSheet(dataset.Tables[sheetIndex], formatProvider, errorLogging);
            }

            return null;
        }

        /// <summary>
        /// Reads the sheet given by its name
        /// </summary>
        /// <param name="dataset">The dataset.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="errorLogging">Return errormessage</param>
        /// <returns>
        /// List of objects created by the Sheet Loader
        /// </returns>
        public static IList<T> ReadSheet(DataSet dataset, IFormatProvider formatProvider, string sheetName, StringBuilderWithUniqueMsg errorLogging)
        {
            if (dataset != null && dataset.Tables.Contains(sheetName))
            {
                return ReadSheet(dataset.Tables[sheetName], formatProvider, errorLogging);
            }

            return null;
        }

        /// <summary>
        /// Reads the sheet.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="errorLogging">The error logging.</param>
        /// <returns>The list of objects</returns>
        private static IList<T> ReadSheet(DataTable table, IFormatProvider formatProvider, StringBuilderWithUniqueMsg errorLogging)
        {
            var sheet = new Sheet<T>(table, formatProvider);
            var result = sheet.Load();
            sheet.ErrorLogging = errorLogging;
            return result;
        }
    }
}
