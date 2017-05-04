using System.Collections.Generic;

namespace Common.Serialization
{
    /// <summary>
    /// The input parameter to serialize a collection of generic objects
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    public class SerializationParameter<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationParameter{T}"/> class.
        /// </summary>
        /// <param name="objects">The objects.</param>
        /// <param name="exportHeader">if set to <c>true</c> export the header.</param>
        public SerializationParameter(IEnumerable<T> objects, bool exportHeader)
            : this(objects, exportHeader, false, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationParameter{T}"/> class.
        /// </summary>
        /// <param name="objects">                  The objects.</param>
        /// <param name="exportHeader">             if set to <c>true</c> export the header.</param>
        /// <param name="orderFields">              True to order fields.</param>
        /// <param name="filterWithIgnoreAttribute">True will filter when ignore attribute is set.</param>
        public SerializationParameter(IEnumerable<T> objects, bool exportHeader, bool orderFields, bool filterWithIgnoreAttribute)
        {
            Objects = objects;
            ExportHeader = exportHeader;
            OrderFieldsPerName = orderFields;
            FilterWithIgnoreAttribute = filterWithIgnoreAttribute;
        }

        /// <summary>
        /// Gets the objects.
        /// </summary>
        /// <value>
        /// The objects.
        /// </value>
        public IEnumerable<T> Objects { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the header should be exported.
        /// </summary>
        /// <value>
        /// True if export header, false if not.
        /// </value>
        public bool ExportHeader { get; private set; }

        /// <summary>
        /// Gets a value indicating whether fields should be ordered per name.
        /// </summary>
        /// <value>
        /// True if order fields per name, false if not.
        /// </value>
        public bool OrderFieldsPerName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the <seealso cref="Reflection.IgnoreSerializationAttribute"/> shall be considered.
        /// </summary>
        /// <value>
        /// True will filter when ignore attribute is set.
        /// </value>
        public bool FilterWithIgnoreAttribute { get; private set; }
    }
}
