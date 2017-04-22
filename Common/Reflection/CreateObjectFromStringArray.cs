using System;
using System.Collections.Generic;
using System.Reflection;
using Common.Generic;

namespace Common.Reflection
{
    /// <summary>
    /// Utilities to create a generic object from a string array.
    /// </summary>
    public static class CreateObjectFromStringArray
    {
        /// <summary>
        /// Creates the object from a string array and the member infos of the objects
        /// </summary>
        /// <typeparam name="T">type of the object</typeparam>
        /// <param name="fieldDict">The field dictionary.</param>
        /// <param name="rowFields">The row fields.</param>
        /// <returns>A new instance of T</returns>
        /// <exception cref="Exception">Unhandled case.</exception>
        public static T CreateObject<T>(Dictionary<int, MemberInfo> fieldDict, string[] rowFields)
            where T : new()
        {
            T newObj = new T();
            for (int i = 0; i < rowFields.Length; i++)
            {
                MemberInfo mi = fieldDict[i];
                string record = rowFields[i];

                if (mi is FieldInfo)
                {
                    ((FieldInfo)mi).SetValue(newObj, record);
                }
                else if (mi is PropertyInfo)
                {
                    var pi = (PropertyInfo)mi;
                    pi.SetValue(newObj, TypeObjectConverter.ConvertObjectRecord(record, pi.PropertyType), null);
                }
                else
                {
                    throw new Exception("Unhandled case.");
                }
            }
            return newObj;
        }

        /// <summary>
        /// Creates the object.
        /// </summary>
        /// <typeparam name="T">type of the object</typeparam>
        /// <param name="rowData">The row data.</param>
        /// <param name="columnNames">The column names.</param>
        /// <param name="lowerAllColumnNames">if set to <c>true</c> lower all column names.</param>
        /// <returns>A new instance of T</returns>
        public static T CreateObject<T>(IList<string> rowData, IList<string> columnNames, bool lowerAllColumnNames)
            where T : new()
        {
            var properties = Utils.GetAllFieldsAndPropertiesOfClassOrdered(typeof(T));
            return CreateObject<T>(properties, rowData, columnNames, lowerAllColumnNames);
        }

        /// <summary>
        /// Creates the object.
        /// </summary>
        /// <typeparam name="T">type of the object</typeparam>
        /// <param name="properties">The properties.</param>
        /// <param name="rowData">The row data.</param>
        /// <param name="columnNames">The column names.</param>
        /// <param name="lowerAllColumnNames">if set to <c>true</c> lower all column names.</param>
        /// <returns>A new instance of T</returns>
        /// <exception cref="Exception">Unhandled case.</exception>
        public static T CreateObject<T>(IEnumerable<MemberInfo> properties, IList<string> rowData, IList<string> columnNames, bool lowerAllColumnNames) 
            where T : new()
        {
            T customObject = new T();
            foreach (MemberInfo mi in properties)
            {
                // get value
                string record = rowData[columnNames.IndexFor(mi.Name, lowerAllColumnNames)];
                // get type
                Type targetType;
                if (mi is FieldInfo)
                {
                    targetType = ((FieldInfo)mi).FieldType;
                }
                else if (mi is PropertyInfo)
                {
                    targetType = ((PropertyInfo)mi).PropertyType;
                }
                else
                {
                    throw new Exception("Unhandled case.");
                }
                // convert value
                object convertedRecord = TypeObjectConverter.ConvertObjectRecord(record, targetType);
                // set value
                if (mi is FieldInfo)
                {
                    ((FieldInfo)mi).SetValue(customObject, convertedRecord);
                }
                else if (mi is PropertyInfo)
                {
                    ((PropertyInfo)mi).SetValue(customObject, convertedRecord, null);
                }
                else
                {
                    throw new Exception("Unhandled case.");
                }
            }
            return customObject;
        }

        /// <summary>
        /// An IList&lt;string&gt; extension method that gets the index for the given name in the list.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="list">                The list to act on.</param>
        /// <param name="name">                The name.</param>
        /// <param name="lowerAllColumnNames">True to lower all column names.</param>
        /// <returns>
        /// An int.
        /// </returns>
        public static int IndexFor(this IList<string> list, string name, bool lowerAllColumnNames)
        {
            int idx = list.IndexOf(lowerAllColumnNames ? name.ToLower() : name);
            if (idx < 0)
            {
                throw new Exception(string.Format("Missing required column mapped to: {0}.", name));
            }
            return idx;
        }
    }
}
