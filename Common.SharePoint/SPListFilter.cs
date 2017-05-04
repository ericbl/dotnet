using Microsoft.SharePoint.Client;

namespace Common.SharePoint
{
    /// <summary>
    /// A sp list filter.
    /// </summary>
    public class SPListFilter : SPListFilterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SPListFilter"/> class.
        /// </summary>
        /// <param name="sourceToCopy">The source to copy.</param>
        /// <param name="type">The type.</param>
        public SPListFilter(SPListFilterBase sourceToCopy, FieldType type)
            : this()
        {
            FieldName = sourceToCopy.FieldName;
            FieldValue = sourceToCopy.FieldValue;
            FieldType = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SPListFilter"/> class.
        /// </summary>
        /// <param name="sourceToCopy">The source to copy.</param>
        public SPListFilter(SPListFilterBase sourceToCopy)
            : this(sourceToCopy, FieldType.Text)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SPListFilter"/> class.
        /// </summary>
        public SPListFilter()
            : base()
        {
        }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public FieldType FieldType { get; internal set; }
    }
}
