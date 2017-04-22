using System;

namespace Common.Reflection
{
    /// <summary>
    /// Define the order of the properties during the serialization
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ColumnOrderAttribute : Attribute
    {
        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnOrderAttribute"/> class.
        /// </summary>
        /// <param name="order">The order.</param>
        public ColumnOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
