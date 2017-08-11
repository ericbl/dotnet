using Common.Extensions;
using Common.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Common.Files
{
    /// <summary>
    /// A CSV serializer.
    /// </summary>
    public static class CsvSerializer
    {
        /// <summary>
        /// Flag if only the date of the date time shall be considered. <c>True</c> per default.
        /// </summary>
        /// <value>
        /// True.
        /// </value>
        public static bool ConverDateTimeAsDateOnly = true;

        /// <summary>
        /// Gets the false.
        /// </summary>
        /// <value>
        /// The false.
        /// </value>
        public static bool ExportQuotesForEmptyString = false;

        /// <summary>
        /// Serializes the collection to Comma Separated Value (CSV) format.
        /// </summary>
        /// <typeparam name="T">Type of the items.</typeparam>
        /// <param name="exportFolderPath">The export folder path.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="collection">The collection.</param>
        /// <param name="filterWithIgnoreAttribute">(Optional) True to filter with ignore attribute.</param>
        /// <param name="append">(Optional) if set to <c>true</c> append to existing file. Otherwise, overwrite the file.</param>
        /// <returns>
        /// The full path of the serialized file.
        /// </returns>
        public static string SerializeCollection<T>(string exportFolderPath, string filename, IEnumerable<T> collection, bool filterWithIgnoreAttribute = true, bool append = false)
        {
            string filePath = null;
            if (collection.Count() > 0)
            {
                filePath = Serialize(exportFolderPath, filename, objects: collection, append: append, sortTFields: false);
            }
            return filePath;
        }

        /// <summary>
        /// Serialize objects to Comma Separated Value (CSV) format ( http://tools.ietf.org/html/rfc4180 )
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="folderFullPath">           The folder full path.</param>
        /// <param name="fileName">                 Name of the file.</param>
        /// <param name="append">                   if set to <c>true</c> append to existing file. Otherwise, overwrite the file.</param>
        /// <param name="objects">                  The collection of objects to serialize.</param>
        /// <param name="sortTFields">              (Optional) True to sort fields.</param>
        /// <param name="filterWithIgnoreAttribute">(Optional) True to filter with ignore attribute.</param>
        /// <returns>
        /// The full path of the written file.
        /// </returns>
        public static string Serialize<T>(
            string folderFullPath, string fileName, bool append, IEnumerable<T> objects, bool sortTFields = false, bool filterWithIgnoreAttribute = true)
        {
            var fileParameter = new SerializationFileParameter(folderFullPath, fileName, append);
            string filePath = fileParameter.GetFileFullPath();
            bool exportHeader = true;
            if (append && File.Exists(filePath))
            {
                exportHeader = false;
            }

            if (objects.Count() > 0)
            {
                var serializationParameter = new SerializationParameter<T>(objects, exportHeader, sortTFields, filterWithIgnoreAttribute);
                return SerializeUtils.Serialize(fileParameter, serializationParameter, Serialize);
            }

            return filePath;
        }

        /// <summary>
        /// Serialize objects to Comma Separated Value (CSV) format ( http://tools.ietf.org/html/rfc4180 )
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="output">                The output.</param>
        /// <param name="serializationParameter">The parameters as input for the serialization.</param>
        internal static void Serialize<T>(TextWriter output, SerializationParameter<T> serializationParameter)
        {
            if (serializationParameter == null)
                throw new ArgumentNullException(nameof(serializationParameter));

            var type = typeof(T);
            var fields = serializationParameter.OrderFieldsPerName
                ? Reflection.Utils.GetAllFieldsAndPropertiesOfClassOrdered(type, serializationParameter.FilterWithIgnoreAttribute)
                : Reflection.Utils.GetAllFieldsAndPropertiesOfClass(type, serializationParameter.FilterWithIgnoreAttribute);
            if (serializationParameter.ExportHeader)
                output.WriteLine(QuoteRecord(fields.Select(f => f.Name)));
            foreach (T record in serializationParameter.Objects)
            {
                string line = QuoteRecord(FormatObject(fields, record));
                output.WriteLine(line);
            }
        }

        private static IEnumerable<string> FormatObject<T>(IEnumerable<MemberInfo> fields, T record)
        {
            foreach (var field in fields)
            {
                if (field is FieldInfo)
                {
                    var fi = (FieldInfo)field;
                    yield return ConvertValue(fi.GetValue(record));
                }
                else if (field is PropertyInfo)
                {
                    var pi = (PropertyInfo)field;
                    yield return ConvertValue(pi.GetValue(record, null));
                }
                else
                {
                    throw new Exception("Unhandled case.");
                }
            }
        }

        private static string ConvertValue(object val)
        {
            string result;
            if (ConverDateTimeAsDateOnly && val is DateTime)
                result = ((DateTime)val).ToShortDateString();
            else if (val is Enum)
            {
                result = ((Enum)val).DisplayDescription();
            }
            else result = Convert.ToString(val);
            return result;
        }

        private static string CsvSeparator = CSVDelimiter.DefaultDelimiter.ToString();

        private static string QuoteRecord(IEnumerable<string> record)
        {
            return string.Join(CsvSeparator, record.Select(field => QuoteField(field)).ToArray());
        }

        private static string QuoteField(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return ExportQuotesForEmptyString ? "\"\"" : string.Empty;
            }
            else if (field.Contains(CsvSeparator) || field.Contains("\"") || field.Contains("\r") || field.Contains("\n"))
            {
                return string.Format("\"{0}\"", field.Replace("\"", "\"\""));
            }
            else
            {
                return field;
            }
        }
    }
}