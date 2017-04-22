using System;
using System.Reflection;

namespace Common.Server.ImportOleDb
{
    /// <summary>
    /// Object Attribute for an Excel column
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnAttribute"/> class.
        /// </summary>
        public ColumnAttribute()
        {
            Name = string.Empty;
            Storage = string.Empty;
            Optional = false;
        }

        /// <summary>
        /// Gets or sets the name of the column as defined in excel row
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the property of the destination object. Should not be filled in object
        /// </summary>
        /// <value>The storage.</value>
        public string Storage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is optional.
        /// </summary>
        /// <value>
        /// True if optional, false if not.
        /// </value>
        public bool Optional { get; set; }

        /// <summary>
        /// Gets or sets the property info.
        /// </summary>
        /// <value>The property info.</value>
        internal PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// Gets the selected column.
        /// </summary>
        /// <value>
        /// The select column.
        /// </value>
        internal string SelectColumn
        {
            get
            {
                if (Name == string.Empty && PropertyInfo != null)
                {
                    return PropertyInfo.Name;
                }

                return Name;
            }
        }

        /// <summary>
        /// Gets the name of the storage.
        /// </summary>
        /// <value>
        /// The name of the storage.
        /// </value>
        internal string StorageName
        {
            get
            {
                if (Storage == string.Empty && PropertyInfo != null)
                {
                    return PropertyInfo.Name;
                }

                return Storage;
            }
        }

        /// <summary>
        /// Debugging Ausgabe des Objektes.
        /// </summary>
        /// <returns>
        /// Name and optional parameters.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} - ({1}optional)", Name, Optional ? string.Empty : "nicht ");
        }

        /// <summary>
        /// Determines whether the storage field is filled.
        /// </summary>
        /// <returns><c>false</c> if storage is null or empty; otherwise, <c>true</c>.</returns>
        internal bool IsFieldStorage()
        {
            return string.IsNullOrEmpty(Storage) == false;
        }
    }
}
