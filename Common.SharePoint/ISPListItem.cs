using System;

namespace Common.SharePoint
{
    /// <summary>
    /// Interface for SharePoint list item.
    /// </summary>
    public interface ISPListItem
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        int ID { get; set; }

        /// <summary>
        /// Gets or sets the Date/Time of the last modification.
        /// </summary>
        /// <value>
        /// The Date/Time of the last modification.
        /// </value>
        DateTime Modified { get; set; }
    }
}
