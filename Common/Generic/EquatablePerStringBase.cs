using System;

namespace Common.Generic
{
    /// <summary>
    /// Abstract class forces object equality by equality of dedicated ToString methods, and thus allowing an easier comparison.
    /// </summary>
    /// <seealso cref="T:System.IEquatable{Common.Generic.EquatablePerStringBase}"/>
    [Serializable]
    public abstract class EquatablePerStringBase : IEquatable<EquatablePerStringBase>
    {
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return ToStringUnique().GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) // avoid == operator in Equals!
                return false;
            if (obj is EquatablePerStringBase)
            {
                return Equals(((EquatablePerStringBase)obj));
            }
            return false;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(EquatablePerStringBase other)
        {
            if (ReferenceEquals(other, null)) // avoid == operator in Equals!
                return false;
            return ToStringUnique().Equals(other.ToStringUnique());
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToStringUnique();
        }

        /// <summary>
        /// Define a unique <see cref="System.String" />  to represent the object. 
        /// This string will be compared for Equals methods and its hash code will considered as object.GetHashCode.
        /// Thus, this string should combine the properties defining together the uniqueKey of the object
        /// </summary>
        /// <returns>A <see cref="System.String" /> defining the object on a unique manner</returns>
        public abstract string ToStringUnique();
    }
}
