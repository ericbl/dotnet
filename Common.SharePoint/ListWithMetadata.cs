using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;

namespace Common.SharePoint
{
    /// <summary>
    /// List container with meta data
    /// </summary>
    /// <typeparam name="T">Type of the objects in the list</typeparam>
    public class ListWithMetadata<T>
        where T : ISPListItem
    {
        #region List and its own properties and Ctor
        private const string SourceDateTimeName = "SourceDate";
        private readonly IList<T> list;
        private readonly ICollection<SPListFilterBase> queryFilters;
        private DateTime sourceDateUTC;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListWithMetadata{T}" /> class.
        /// </summary>
        /// <param name="sourceDate"> (Optional) The source date, will be converted to UTC for SharePoint.</param>
        /// <param name="queryFilters">(Optional) The query filters.</param>
        public ListWithMetadata(DateTime? sourceDate = null, ICollection<SPListFilterBase> queryFilters = null)
        {
            this.list = new List<T>();
            if (sourceDate.HasValue)
                this.sourceDateUTC = sourceDate.Value.ToUniversalTime();
            this.queryFilters = queryFilters;
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

        /// <summary>
        /// Gets the query filters.
        /// </summary>
        /// <value>
        /// The query filters.
        /// </value>
        public ICollection<SPListFilterBase> QueryFilters
        {
            get { return queryFilters; }
        }
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
    }
}
