using Common.Strings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Common.Extensions
{
    /// <summary>
    /// Extension Class for the Enum Type
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// Gets the Description of the DescriptionAttribute for the Enum Object
        /// </summary>
        /// <param name="currentEnumValue">Enum Value for Check to Description</param>
        /// <param name="language">The language. Null per default, it will read the Description Attribute. If set, a LocalizedDescriptionAttribute is required!</param>
        /// <param name="getKeyOnly">if set to <c>true</c> read the LocalizedDescriptionAttribute
        /// but get only the key or name instead of the translated description.</param>
        /// <param name="valueSeparator">The value separator.</param>
        /// <returns>
        ///   <c>String</c> If has Description then with Description, otherwise Enum Name
        /// </returns>
        public static string DisplayDescription(this Enum currentEnumValue, string language = null, bool getKeyOnly = false, string valueSeparator = ", ")
        {
            Type enumType = currentEnumValue.GetType();
            string enumValueStr = currentEnumValue.ToString();
            if (string.IsNullOrEmpty(enumValueStr))
                return null;

            return Strings.Helper.DisplayLocalizedDescription(enumType, enumValueStr, language, getKeyOnly, valueSeparator);
        }

        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

        /// <summary>
        /// Gets the EnumValue with the Description or Name same as <c>descriptionOrValue</c>
        /// </summary>
        /// <param name="descriptionOrValue">Name or Description ther look for.</param>
        /// <param name="type">              The enum type.</param>
        /// <returns>
        /// Enum Value as Object or Null if not found.
        /// </returns>
        public static object EnumValueOf(this string descriptionOrValue, Type type)
        {
            //Compare all EnumValues Descriptions or Names from EnumType with descriptionOrValue
            foreach (Enum val in Enum.GetValues(type))
            {
                if (val.DisplayDescription().Equals(descriptionOrValue) || val.ToString().Equals(descriptionOrValue))
                {
                    return val;
                }
            }

            //In case of not found return null
            return null;
        }

        /// <summary>
        /// Gets an Dictionary with Description in Key and Enum Value in Value
        /// </summary>
        /// <typeparam name="T">Enum Type</typeparam>
        /// <param name="value">Enum Value</param>
        /// <returns>Dictionary with string - Description/int - Enumvalue</returns>
        public static Dictionary<string, int> GetEnumBindingList<T>(this T value)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            Type type = typeof(T);
            foreach (int enumValue in Enum.GetValues(type))
            {
                result.Add(((Enum)Enum.GetName(type, enumValue).EnumValueOf(type)).DisplayDescription(), enumValue);
            }

            return result;
        }
    }
}
