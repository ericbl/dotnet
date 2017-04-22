using System;

namespace Common.Generic
{
    /// <summary>
    /// Extends the  <seealso cref="T:Common.Generic.EquatablePerStringBase"/> to implements the <seealso cref="T:System.IFormattable"/> interface
    /// </summary>
    [Serializable]
    public abstract class FormattableAndEquatableStringBase : EquatablePerStringBase, IFormattable
    {
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToStringIFormattable();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that concatenate ALL properties of this instance. Would be use to quickly define full equality of objects.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public abstract string ToStringAllProperties();

        /// <summary>
        /// Returns a <see cref="System.String" /> that concatenate the most relevant properties of this instance.
        /// This is the string that will be used when the instance is serialized as string, through the IFormattable interface 
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public abstract string ToStringIFormattable();
    }
}
