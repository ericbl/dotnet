using Microsoft.SharePoint.Client;

namespace Common.SharePoint
{
    /// <summary>
    /// A sp list filter base.
    /// </summary>
    public class SPListFilterBase
    {
        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the field value.
        /// </summary>
        /// <value>
        /// The field value.
        /// </value>
        public object FieldValue { get; set; }
    }
}
