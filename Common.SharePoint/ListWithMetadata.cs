using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.SharePoint
{
    /// <summary>
    /// List container with metadata
    /// </summary>
    /// <typeparam name="T">Type of the objects in the list</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public class ListWithMetadata<T, TKey>
    {
        #region List and its own properties and Ctor
        private const string SourceDateTimeName = "SourceDate";
        private readonly IList<T> list;
        private readonly Func<T, TKey> keySelector;
        private DateTime sourceDateUTC;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListWithMetadata{T, TKey}" /> class.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="sourceDate">The source date, will be converted to UTC for SharePoint.</param>
        /// <param name="keySelector">The key selector.</param>
        public ListWithMetadata(IList<T> list = null, DateTime? sourceDate = null, Func<T, TKey> keySelector = null)
        {
            if (list != null)
                this.list = list;
            else
                this.list = new List<T>();
            if (sourceDate.HasValue)
                this.sourceDateUTC = sourceDate.Value.ToUniversalTime();
            this.keySelector = keySelector;
        }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <value>
        /// The list.
        /// </value>
        public IList<T> List
        {
            get { return list; }
        }

        /// <summary>
        /// Gets the source date as local time
        /// </summary>
        /// <value>
        /// The source date.
        /// </value>
        public DateTime SourceDateLocal
        {
            get { return sourceDateUTC.ToLocalTime(); }
        }
        #endregion

        #region Important properties of T for List
        /// <summary>
        /// Gets or sets the name of the identifier field.
        /// </summary>
        /// <value>
        /// The name of the identifier field.
        /// </value>
        public string IdFieldName { get; set; }

        /// <summary>
        /// Gets or sets the name of the second identifier field.
        /// </summary>
        /// <value>
        /// The name of the second identifier field.
        /// </value>
        public string SecondIdFieldName { get; set; }

        /// <summary>
        /// Gets or sets the value of the second identifier field to filter only list item matching it.
        /// </summary>
        /// <value>
        /// The value of the second identifier field to filter only list item matching it.
        /// </value>
        public string SecondIdFieldValueFilter { get; set; }
        #endregion

        /// <summary>
        /// Sets the source description.
        /// </summary>
        /// <param name="listProperties">The list properties.</param>
        internal void SetListProperties(PropertyValues listProperties)
        {
            DateTime? currentValue = ReadSourceDateProperty(listProperties);
            if (currentValue.HasValue) //&& currentValue.Value != SourceDate)
                this.sourceDateUTC = (DateTime)listProperties[SourceDateTimeName];
        }

        /// <summary>
        /// Reads the source date property.
        /// </summary>
        /// <param name="listProperties">The list properties.</param>
        /// <returns>The source date property</returns>
        internal static DateTime? ReadSourceDateProperty(PropertyValues listProperties)
        {
            if (listProperties.FieldValues.ContainsKey(SourceDateTimeName) && listProperties[SourceDateTimeName] != null)
                return (DateTime)listProperties[SourceDateTimeName];
            return null;
        }

        /// <summary>
        /// Sets the source date property.
        /// </summary>
        /// <param name="listProperties">The list properties.</param>
        /// <returns><c>True</c> if changed</returns>
        internal bool SetSourceDateProperty(PropertyValues listProperties)
        {
            DateTime? currentValue = ReadSourceDateProperty(listProperties);
            if (!currentValue.HasValue || currentValue.Value != sourceDateUTC)
            {
                listProperties[SourceDateTimeName] = sourceDateUTC;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the dictionary from list.
        /// </summary>
        /// <returns>The converted dictionary</returns>
        internal Dictionary<TKey, T> GetDictionaryFromList()
        {
            if (keySelector == null)
                return null;
            return List.ToDictionary(keySelector);
        }
    }
}
