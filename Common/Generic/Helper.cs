using Common.Strings;
using System;
using System.ComponentModel;

namespace Common.Generic
{
    /// <summary>
    /// Allgemeine Hilfsfunktionen with generic or types
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Swaps the specified items references.
        /// </summary>
        /// <typeparam name="T">Type of the item</typeparam>
        /// <param name="item1">The item1.</param>
        /// <param name="item2">The item2.</param>
        public static void Swap<T>(ref T item1, ref T item2)
        {
            T dummy = item1;
            item1 = item2;
            item2 = dummy;
        }

        /// <summary>
        /// Check if null and only if not return ToString()
        /// </summary>
        /// <typeparam name="T">Type of the item.</typeparam>
        /// <param name="item">The item.</param>
        /// <returns>
        /// Item as a string.
        /// </returns>
        public static string ToStringOrNull<T>(this T item)
        {
            return item == null ? null : item.ToString();
        }

        /// <summary>
        /// Check if null and only if not check if DateTime and call <seealso cref="Strings.Helper.ToShortDateString(DateTime?)"/>; otherwise return ToString()
        /// </summary>
        /// <typeparam name="T">Type of the item.</typeparam>
        /// <param name="item">The item.</param>
        /// <returns>
        /// Item as a string
        /// </returns>
        public static string ToStringOrNullOrShortDate<T>(this T item)
        {
            if (item == null)
                return null;
            if (item is DateTime?)
                return (item as DateTime?).ToShortDateString();
            else if (item is DateTime)
                return ((DateTime)(object)item).ToShortDateString();
            return item.ToString();
        }

        /// <summary>
        /// Determines whether the specified type is a null-able type. 
        /// </summary>
        /// <param name="theType">The type.</param>
        /// <returns><c>true</c> if  whether the specified type is a null-able type; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullableType(this Type theType)
        {
            return (theType.IsGenericType && theType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
        }

        /// <summary>
        /// Gets the type behind the null-able one
        /// </summary>
        /// <param name="theType">The null-able type.</param>
        /// <returns>The UnderlyingType if found </returns>
        public static Type GetNullableUnderlyingType(Type theType)
        {
            return IsNullableType(theType) ? new NullableConverter(theType).UnderlyingType : theType;
        }

        /// <summary>
        /// Appends the string only once to the property of the item.
        /// </summary>
        /// <typeparam name="T">Type of the item</typeparam>
        /// <param name="item">The item.</param>
        /// <param name="propertyName">Name of the property of the string to update.</param>
        /// <param name="messageToAppend">The message to append to item.propertyName</param>
        /// <param name="separator">The separator.</param>
        /// <exception cref="ArgumentException">{propertyName} not found in {typeof(T)}")</exception>
        public static void AppendStringOnceToString<T>(T item, string propertyName, string messageToAppend, string separator = ", ")
        {
            var pi = typeof(T).GetProperty(propertyName, typeof(string));
            if (pi == null)
                throw new ArgumentException($"{propertyName} not found in {typeof(T)}");
            string currentValue = pi.GetValue(item) as string;

            if (string.IsNullOrEmpty(currentValue) ||
              (!string.IsNullOrEmpty(currentValue) && !currentValue.Contains(messageToAppend)))
            {
                if (!string.IsNullOrEmpty(currentValue))
                    currentValue += separator;
                currentValue += messageToAppend;
                pi.SetValue(item, messageToAppend);
            }
        }


    }
}
