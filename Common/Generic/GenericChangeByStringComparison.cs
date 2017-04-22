using Common.Reflection;
using System;
using System.Collections.Generic;

namespace Common.Generic
{
    /// <summary>
    /// Implements the <seealso cref="T:Common.Generic.EquatablePerStringBase"/> where the generic parameter is itself a 
    /// <seealso cref="T:Common.Generic.FormattableAndEquatableStringBase"/> for tracking changes of objects, between previous/earlier T and new proposed T
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    [Serializable]
    public class GenericChangeByStringComparison<T> : EquatablePerStringBase
        where T : FormattableAndEquatableStringBase
    {
        private readonly string[] propertiesIgnoredForCompare;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericChangeByStringComparison{T}" /> class.
        /// </summary>
        /// <param name="earlier">The earlier.</param>
        /// <param name="proposedChange">The proposed change.</param>
        /// <param name="propertiesIgnoredForCompare">The properties ignored for compare.</param>
        /// <exception cref="ArgumentNullException">parameter not set</exception>
        public GenericChangeByStringComparison(T earlier, T proposedChange, string[] propertiesIgnoredForCompare)
        {
            // set parameter objects
            if (earlier == null)
                throw new ArgumentNullException(nameof(earlier));
            if (proposedChange == null)
                throw new ArgumentNullException(nameof(proposedChange));
            Earlier = earlier;
            ProposedNew = proposedChange;
            // set ignored properties
            if (propertiesIgnoredForCompare == null)
                this.propertiesIgnoredForCompare = new string[] { };
            else
                this.propertiesIgnoredForCompare = propertiesIgnoredForCompare;
        }

        /// <summary>
        /// Gets or sets the earlier item
        /// </summary>
        /// <value>
        /// The earlier.
        /// </value>
        public T Earlier { get; private set; }

        /// <summary>
        /// Gets or sets the proposed new item.
        /// </summary>
        /// <value>
        /// The proposed new.
        /// </value>
        public T ProposedNew { get; private set; }

        /// <summary>
        /// Gets the names of properties changed.
        /// </summary>
        /// <returns>The names of the changed properties</returns>
        public IReadOnlyList<string> GetNamesOfPropertiesChanged()
        {
            return Utils.GetNamesOFPropertiesChanged(Earlier, ProposedNew, propertiesIgnoredForCompare);
        }

        /// <summary>
        /// Gets the properties changed.
        /// </summary>
        /// <returns>The changed properties</returns>
        public IReadOnlyList<ObjectPropertyChanged> GetPropertiesChanged()
        {
            return Utils.GetPublicGenericPropertiesChanged(Earlier, ProposedNew, propertiesIgnoredForCompare);
        }

        /// <summary>
        /// Define a unique <see cref="System.String" />  to represent the object.
        /// This string will be compared for Equals methods and its hash code will considered as object.GetHashCode.
        /// Thus, this string should combine the properties defining together the uniqueKey of the object
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> defining the object on a unique manner
        /// </returns>
        public override string ToStringUnique()
        {
            return Earlier.ToStringUnique();
        }
    }
}
