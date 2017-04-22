using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Common.Strings;

namespace Common.Server.ImportOleDb
{
    /// <summary>
    /// Convert a given datatable to a List of T, one object per row, the column name must fit the object property name
    /// </summary>
    /// <typeparam name="T">Type of the object</typeparam>
    public class Sheet<T> : IEnumerable<T>
        where T : IDataTableToList
    {
        /// <summary>
        /// Provider to read the sheet.
        /// </summary>
        private readonly DataTable provider;

        /// <summary>
        /// Provider for the culture.
        /// </summary>
        private readonly IFormatProvider formatProvider;

        /// <summary>
        /// Intern list of rows read from the sheet.
        /// </summary>
        private readonly List<T> rows;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sheet&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="cultureInfo">The culture info.</param>
        public Sheet(DataTable provider, IFormatProvider cultureInfo)
        {
            this.provider = provider;
            this.formatProvider = cultureInfo;
            rows = new List<T>();
        }

        /// <summary>
        /// Gets the error logging filled during the loading of the data
        /// </summary>
        /// <value>The error logging.</value>
        public StringBuilderWithUniqueMsg ErrorLogging { get; internal set; }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            Load();
            return rows.GetEnumerator();
        }

        /// <summary>
        /// Loads the data from the datatable and generate objects. Continue if an error occurs and set the ErrorLogging
        /// </summary>
        /// <returns>A list of generated objects</returns>
        public List<T> Load()
        {
            IEnumerable<ColumnAttribute> columns = GetColumnList();
            rows.Clear();

            // Prepare columns mapping
            var colMapping = new Dictionary<ColumnAttribute, DataColumn>();
            foreach (ColumnAttribute column in columns)
            {
                bool found = false;
                foreach (DataColumn dataColumn in provider.Columns)
                {
                    string columnName = dataColumn.ColumnName.Replace('#', '.');
                    if (column.SelectColumn == columnName)
                    {
                        colMapping.Add(column, dataColumn);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    foreach (DataColumn dataColumn in provider.Columns)
                    {
                        string columnName = dataColumn.ColumnName.Replace('#', '.');
                        if (column.SelectColumn.StartsWith(columnName))
                        {
                            colMapping.Add(column, dataColumn);
                            break;
                        }
                    }
                }

                if (!column.Optional && !colMapping.ContainsKey(column))
                {
                    throw new ApplicationException(string.Format("Die nicht optionale Spalte mit dem Namen '{0}' wurde nicht gefunden.", column.Name));
                }
            }

            // Parse all rows
            foreach (DataRow row in provider.Rows)
            {
                if (CheckIfRowHasValue(row, provider.Columns))
                {
                    T item = Activator.CreateInstance<T>();
                    foreach (ColumnAttribute col in colMapping.Keys)
                    {
                        object val = row[colMapping[col]];
                        // Convert to the destination type
                        string errorMsg;
                        var typedValue = Generic.TypeObjectConverter.ConvertType(val, col.PropertyInfo.PropertyType, out errorMsg, formatProvider);
                        if (errorMsg != null && ErrorLogging != null)
                        {
                            ErrorLogging.AppendLine("Fehler in Zeile {0} und Spalte '{1}': {2}", provider.Rows.IndexOf(row), col.SelectColumn, errorMsg);
                            continue;
                        }

                        try
                        {
                            if (col.IsFieldStorage())
                            {
                                FieldInfo fi = typeof(T).GetField(
                                    col.StorageName, BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField);
                                if (fi != null)
                                {
                                    fi.SetValue(item, typedValue);
                                }
                            }
                            else
                            {
                                typeof(T).GetProperty(col.StorageName).SetValue(item, typedValue, null);
                            }
                        }
                        catch (Exception)
                        {
                            if (ErrorLogging != null)
                            {
                                ErrorLogging.AppendLine(
                                    "Fehler in Zeile {0} und Spalte '{1}': Die Werte {2} konnten nicht gesetzt werden!",
                                    provider.Rows.IndexOf(row),
                                    col.SelectColumn,
                                    typedValue.ToString());
                            }
                        }
                    }

                    rows.Add(item);
                }
            }

            return this.rows;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            Load();
            return rows.GetEnumerator();
        }

        /// <summary>
        /// Gets the name of the sheet.
        /// </summary>
        /// <returns>Name of the sheet</returns>
        private string GetSheetName()
        {
            return provider.TableName;
        }

        /// <summary>
        /// Gets the column list.
        /// </summary>
        /// <returns>A collection of ColumnAttribute</returns>
        private static IEnumerable<ColumnAttribute> GetColumnList()
        {
            var lst = new List<ColumnAttribute>();
            foreach (PropertyInfo propInfo in typeof(T).GetProperties())
            {
                object[] attr = propInfo.GetCustomAttributes(typeof(ColumnAttribute), true);
                if (attr.Length > 0)
                {
                    var col = (ColumnAttribute)attr[0];
                    col.PropertyInfo = propInfo;
                    lst.Add(col);
                }
            }

            return lst;
        }

        /// <summary>
        /// Checks if the given row is not empty, i.e. if at least one cell is filled.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="columns">The columns.</param>
        /// <returns>true if the row if valid; false if empty</returns>
        private static bool CheckIfRowHasValue(DataRow row, DataColumnCollection columns)
        {
            return columns.Cast<DataColumn>().Any(col => !string.IsNullOrEmpty(row[col].ToString()));
        }
    }
}
