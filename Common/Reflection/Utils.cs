using Common.Generic;
using Common.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Common.Reflection
{
    /// <summary>
    /// Utilities for reflection, i.e. working on member infos of a generic object
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Gets all properties of class with the attribute.
        /// </summary>
        /// <typeparam name="TAttr">The type of the attribute to consider.</typeparam>
        /// <param name="type">The type.</param>
        /// <returns>Collection of propertyInfo</returns>
        public static IEnumerable<PropertyInfo> GetAllPropertiesOfClassWithAttribute<TAttr>(Type type)
           where TAttr : Attribute
        {
            return from mi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                   let customAttr = (TAttr)Attribute.GetCustomAttribute(mi, typeof(TAttr))
                   where customAttr != null
                   select mi;
        }
        /// <summary>
        /// Gets all properties of the class.
        /// </summary>
        /// <param name="type">                     The type of the class to read the memberInfo from.</param>
        /// <param name="filterWithIgnoreAttribute">(Optional) True will read the <seealso cref="IgnoreSerializationAttribute"/> 
        /// and ignore the properties with that attribute.</param>
        /// <returns>
        /// collection of PropertyInfo.
        /// </returns>
        public static IEnumerable<PropertyInfo> GetAllPropertiesOfClass(Type type, bool filterWithIgnoreAttribute)
        {
            return from mi in GetAllFieldsAndPropertiesOfClass(type, filterWithIgnoreAttribute)
                   where mi.MemberType == MemberTypes.Property
                   select mi as PropertyInfo;
        }



        /// <summary>
        /// Gets all fields and properties of the class.
        /// </summary>
        /// <param name="type">                     The type of the class to read the memberInfo from.</param>
        /// <param name="filterWithIgnoreAttribute">True will read the <seealso cref="IgnoreSerializationAttribute"/>
        /// and filter them out from the collection.</param>
        /// <returns>
        /// collection of memberInfo.
        /// </returns>
        public static IEnumerable<MemberInfo> GetAllFieldsAndPropertiesOfClass(Type type, bool filterWithIgnoreAttribute)
        {
            return from mi in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                   let ignoreAttr = (IgnoreSerializationAttribute)Attribute.GetCustomAttribute(mi, typeof(IgnoreSerializationAttribute))
                   where (mi.MemberType == MemberTypes.Field || mi.MemberType == MemberTypes.Property) &&
                       (!filterWithIgnoreAttribute || (filterWithIgnoreAttribute && ignoreAttr == null))
                   orderby type.Equals(mi.DeclaringType) ? 1 : -1
                   select mi;
        }

        /// <summary>
        /// Gets all fields and properties of the class, order by ColumnOrderAttribute or Name.
        /// </summary>
        /// <param name="type">                     The type of the class to read the memberInfo from.</param>
        /// <param name="filterWithIgnoreAttribute">(Optional) True will read the <seealso cref="IgnoreSerializationAttribute"/>
        /// and filter them out from the collection.</param>
        /// <returns>
        /// collection of memberInfo.
        /// </returns>
        public static IEnumerable<MemberInfo> GetAllFieldsAndPropertiesOfClassOrdered(Type type, bool filterWithIgnoreAttribute = true)
        {
            return from mi in GetAllFieldsAndPropertiesOfClass(type, filterWithIgnoreAttribute)
                   let orderAttr = (ColumnOrderAttribute)Attribute.GetCustomAttribute(mi, typeof(ColumnOrderAttribute))
                   orderby orderAttr == null ? int.MaxValue : orderAttr.Order, mi.Name
                   select mi;
        }

        /// <summary>
        /// Find the property in item and set it with PropertyName.
        /// </summary>
        /// <typeparam name="T">type of the object</typeparam>
        /// <param name="item">The item.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="ArgumentException">{propertyName} not found in {typeof(T)}")</exception>
        public static void SetPropertyValueAsPropertyName<T>(T item, string propertyName)
        {
            if (item == null)
                return;
            var pi = item.GetType().GetProperty(propertyName, typeof(string));
            if (pi != null)
                pi.SetValue(item, propertyName);
            else throw new ArgumentException($"{propertyName} not found in {typeof(T)}");
        }

        /// <summary>
        /// Gets the names of the public properties which values differ between first and second objects.
        /// </summary>
        /// <typeparam name="T">type of the object</typeparam>
        /// <param name="first">The first object.</param>
        /// <param name="second">The second object.</param>
        /// <param name="namesOfPropertiesToBeIgnored">The names of the properties to be ignored.</param>
        /// <returns>
        /// List of the property names
        /// </returns>
        public static IReadOnlyList<string> GetNamesOFPropertiesChanged<T>(this T first, T second, params string[] namesOfPropertiesToBeIgnored) where T : class
        {
            IEnumerable<ObjectPropertyChanged> propertiesChanged = GetPublicGenericPropertiesChanged(first, second, namesOfPropertiesToBeIgnored);
            return GetNamesOFPropertiesChanged(propertiesChanged);
        }

        /// <summary>
        /// Gets the names of properties changed.
        /// </summary>
        /// <param name="propertiesChanged">The properties changed.</param>
        /// <returns>The names of properties changed.</returns>
        public static IReadOnlyList<string> GetNamesOFPropertiesChanged(IEnumerable<ObjectPropertyChanged> propertiesChanged)
        {
            return propertiesChanged.Select(o => o.PropertyName).ToList();
        }

        /// <summary>
        /// Gets the names of the public properties which values differ.
        /// </summary>
        /// <param name="propertiesChanged">The properties changed.</param>
        /// <returns>
        /// Comma separated names of the properties
        /// </returns>
        public static string GetConcatenatedNamesOfPropertiesChanged(IEnumerable<ObjectPropertyChanged> propertiesChanged)
        {
            if (propertiesChanged != null)
            {
                return Strings.Helper.StringCollectionToString(GetNamesOFPropertiesChanged(propertiesChanged));
            }
            return null;
        }

        /// <summary>
        /// Gets the names of the public properties which values differs between first and second objects.
        /// </summary>
        /// <typeparam name="T">type of the object</typeparam>
        /// <param name="previous">The first object.</param>
        /// <param name="proposedChange">The second object.</param>
        /// <param name="namesOfPropertiesToBeIgnored">The names of the properties to be ignored.</param>
        /// <returns>
        /// the names of the properties
        /// </returns>
        public static IReadOnlyList<ObjectPropertyChanged> GetPublicSimplePropertiesChanged<T>(this T previous, T proposedChange,
         string[] namesOfPropertiesToBeIgnored) where T : class
        {
            return GetPublicGenericPropertiesChanged(previous, proposedChange, namesOfPropertiesToBeIgnored, true, null, null, false);
        }

        /// <summary>
        /// Gets the names of the public properties which values differs between first and second objects.
        /// Considers 'simple' properties AND for complex properties without index, get the simple properties of the children objects.
        /// </summary>
        /// <typeparam name="T">type of the object</typeparam>
        /// <param name="previous">The previous object.</param>
        /// <param name="proposedChange">The second object which should be the new one.</param>
        /// <param name="namesOfPropertiesToBeIgnored">The names of the properties to be ignored.</param>
        /// <param name="prefixPropertyNameWithTypeName">if set to <c>true</c> prefix the property name with the type name.</param>
        /// <returns>
        /// the names of the properties
        /// </returns>
        public static IReadOnlyList<ObjectPropertyChanged> GetPublicGenericPropertiesChanged<T>(this T previous, T proposedChange,
            string[] namesOfPropertiesToBeIgnored, bool prefixPropertyNameWithTypeName = true) where T : class
        {
            return GetPublicGenericPropertiesChanged(previous, proposedChange, namesOfPropertiesToBeIgnored,
                false, null, null, prefixPropertyNameWithTypeName);
        }

        /// <summary>
        /// Gets the names of the public properties which values differs between first and second objects.
        /// Considers 'simple' properties AND for complex properties without index, get the simple properties of the children objects.
        /// </summary>
        /// <typeparam name="T">type of the object</typeparam>
        /// <param name="previous">The previous object.</param>
        /// <param name="proposedChange">The second object which should be the new one.</param>
        /// <param name="namesOfPropertiesToBeIgnored">The names of the properties to be ignored.</param>
        /// <param name="simpleTypeOnly">if set to <c>true</c> consider simple types only.</param>
        /// <param name="parentTypeString">The parent type string. Meant only for recursive call with simpleTypeOnly set to <c>true</c>.</param>
        /// <param name="secondType">when calling recursively, the current type of T must be clearly defined here, as T will be more generic (using base class).</param>
        /// <param name="prefixPropertyNameWithTypeName">if set to <c>true</c> prefix the property name with the type name.</param>
        /// <returns>
        /// the names of the properties
        /// </returns>
        private static IReadOnlyList<ObjectPropertyChanged> GetPublicGenericPropertiesChanged<T>(this T previous, T proposedChange,
            string[] namesOfPropertiesToBeIgnored, bool simpleTypeOnly, string parentTypeString, Type secondType, bool prefixPropertyNameWithTypeName) where T : class
        {
            List<ObjectPropertyChanged> propertiesChanged = new List<ObjectPropertyChanged>();

            if (previous != null && proposedChange != null)
            {
                var type = secondType == null ? typeof(T) : secondType;
                string typeStr = prefixPropertyNameWithTypeName ? parentTypeString + type.Name + "." : null;
                var ignoreList = namesOfPropertiesToBeIgnored.CreateList();
                IEnumerable<IEnumerable<ObjectPropertyChanged>> genericPropertiesChanged =
                    from pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    where !ignoreList.Contains(pi.Name) && pi.GetIndexParameters().Length == 0
                        && (!simpleTypeOnly || simpleTypeOnly && pi.PropertyType.IsSimpleType())
                    let firstValue = type.GetProperty(pi.Name).GetValue(previous, null)
                    let secondValue = type.GetProperty(pi.Name).GetValue(proposedChange, null)
                    where firstValue != secondValue && (firstValue == null || !firstValue.Equals(secondValue))
                    let subPropertiesChanged = simpleTypeOnly || pi.PropertyType.IsSimpleType()
                        ? null
                        : GetPublicGenericPropertiesChanged(firstValue, secondValue, namesOfPropertiesToBeIgnored,
                            true, typeStr, pi.PropertyType, prefixPropertyNameWithTypeName)
                    let objectPropertiesChanged = subPropertiesChanged != null && subPropertiesChanged.Count() > 0
                        ? subPropertiesChanged
                        : (new ObjectPropertyChanged(proposedChange.ToString(), typeStr + pi.Name,
                            firstValue.ToStringOrNullOrShortDate(), secondValue.ToStringOrNullOrShortDate())).CreateList()
                    select objectPropertiesChanged;

                if (genericPropertiesChanged != null)
                {   // get items from sub lists
                    genericPropertiesChanged.ForEach(a => propertiesChanged.AddRange(a));
                }
            }
            return propertiesChanged;
        }

        /// <summary>
        /// Determine whether a type is simple (String, Decimal, DateTime, etc)
        /// or complex (i.e. custom class with public properties and methods).
        /// </summary>
        /// <param name="type">The type of the class to read the memberInfo from.</param>
        /// <returns>
        /// True if simple type, false if not.
        /// </returns>
        public static bool IsSimpleType(this Type type)
        {
            return type.IsValueType || type.IsPrimitive || otherSimpleTypes.Contains(type) ||
                (Convert.GetTypeCode(type) != TypeCode.Object);
        }

        private static readonly Type[] otherSimpleTypes = new[] {
            typeof(string),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(Guid)
        };

        /// <summary>
        /// Maps the properties to the description attribute. To be used with
        /// <seealso cref="GetPublicGenericPropertiesChanged{T}(T, T, string[], bool)"/>
        /// </summary>
        /// <param name="type">The type of the class to read the memberInfo from.</param>
        /// <returns>
        /// A dictionary with all descriptions as keys and the properties as values.
        /// </returns>
        public static Dictionary<string, string> MapPropertiesToDescriptionAttribute(Type type)
        {
            var propertyAndDescriptions =
                from pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                let attribute = (PathDescriptionAttribute)Attribute.GetCustomAttribute(pi, typeof(PathDescriptionAttribute))
                let desc = attribute == null ? null : attribute.Description
                where !string.IsNullOrEmpty(desc)
                select new Tuple<string, string>(pi.Name, desc);

            var mapPropertiesToDescription = new Dictionary<string, List<string>>();
            var result = new Dictionary<string, string>();

            if (propertyAndDescriptions != null)
            {
                foreach (var propAndDesc in propertyAndDescriptions)
                {
                    CollectionHelper.AddItemToDictionaryListOnce(mapPropertiesToDescription, propAndDesc.Item2, propAndDesc.Item1, true);
                }

                var list2String = new CommaSeparatedStringDump<string>("|");
                foreach (var key in mapPropertiesToDescription.Keys)
                {
                    // dump lists of properties
                    result.Add(key, list2String.DumpListCommaSeparated(mapPropertiesToDescription[key]));
                }
            }

            return result;
        }
    }
}
