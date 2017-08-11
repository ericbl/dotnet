// <copyright file="CSVReader.cs">
// Copyright (c) 2017 Eric B. All rights reserved.
// </copyright>
// <author>Eric</author>
// <date>18.04.2017, last updated 29.06.2017</date>
// <summary>Implements the CSV reader class</summary>
using Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Common.Files
{
    /// <summary>
    /// A CSV reader.
    /// </summary>
    public static class CSVReader
    {
        /// <summary>
        /// Filters the collection to serialize by removing existing items. T MUST implements equals used by list.Contains()!!
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="objects">The objects.</param>
        /// <param name="folderFullPath">The folder full path.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>Filtered collection. If the target file does not exists, returns all objects</returns>
        public static IEnumerable<T> FilterCollectionToSerializeByRemovingExistingItems<T>(string folderFullPath, string fileName, IEnumerable<T> objects)
          where T : new()
        {
            IEnumerable<T> results = null;
            if (objects != null && objects.Count() > 0)
            {
                string filePath = IOUtils.CheckAndCombinePath(folderFullPath, fileName, false);
                if (System.IO.File.Exists(filePath))
                {
                    // filter objects
                    IList<T> previousList = ReadCsvFile<T>(filePath, true);
                    results = objects.Where(item => !previousList.Contains(item)).ToArray();
                }
                else
                {   // no file, nothing to filter, return the full input list!
                    results = objects.ToArray();
                }
            }

            return results;
        }

        /// <summary>
        /// Reads the CSV file and transform it as a list of object
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="fileFullPath">The file full path.</param>
        /// <param name="firstLineIsHeader">if set to <c>true</c>, consider the first line as the header.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <returns>
        /// A list of objects
        /// </returns>
        /// <exception cref="Common.Exceptions.UserException">Error creating an instance</exception>
        public static IList<T> ReadCsvFile<T>(string fileFullPath, bool firstLineIsHeader = true, string cultureInfo = null)
            where T : new()
        {
            if (string.IsNullOrEmpty(fileFullPath))
                return null;

            var delimiter = CSVDelimiter.DelimiterCharFromCurrentCulture;
            string[] lines = IOUtils.ReadTxtFileLines(fileFullPath, false);
            if (lines == null)
            {
                return null;
            }
            // Prepare properties.
            Dictionary<int, MemberInfo> fieldDict = null;
            int firstLineNrData = firstLineIsHeader ? 1 : 0;
            if (firstLineIsHeader)
            {
                // First line shall be the header!
                var columnNames = lines[0].Split(delimiter);
                fieldDict = ReadHeader<T>(columnNames);
            }
            else
            {
                int i = 0;
                fieldDict = new Dictionary<int, MemberInfo>();
                foreach (var mi in Utils.GetAllFieldsAndPropertiesOfClass(typeof(T), true))
                {
                    fieldDict.Add(i++, mi);
                }
            }

            var list = new List<T>();
            for (int lineNr = firstLineNrData; lineNr < lines.Length; lineNr++)
            {
                try
                {
                    string[] rowData = lines[lineNr].Split(delimiter);
                    if (rowData == null)
                        continue;
                    T item = CreateObjectFromStringArray.CreateObject<T>(fieldDict, rowData, cultureInfo);
                    list.Add(item);
                }
                catch (Exception e)
                {
                    string errorMsg = $"Error creating an instance of {typeof(T)}, the row data are {lines[lineNr]} and text delimiter is {delimiter}!";
                    throw new Common.Exceptions.UserException(errorMsg, e);
                }
            }

            return list;
        }

        #region Read CSV file / De serialization

        //public static IEnumerable<T> ReadCsvFileTextFieldParser<T>(string fileFullPath, string delimiter = ";") where T : new()
        //{
        //    if (!File.Exists(fileFullPath))
        //    {
        //        return null;
        //    }
        //    Dictionary<int, MemberInfo> fieldDict = null;
        //    var list = new List<T>();
        //    using (var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(fileFullPath))
        //    {
        //        parser.SetDelimiters(delimiter);
        //        while (!parser.EndOfData)
        //        {
        //            //Processing row
        //            string[] rowFields = parser.ReadFields();
        //            if (fieldDict == null)
        //            {
        //                fieldDict = ReadHeader<T>(rowFields);
        //            }
        //            else
        //            {
        //                CreateObjectAndAddToList(fieldDict, rowFields, list);
        //            }
        //        }
        //    }
        //    return list;
        //}

        /// <summary>
        /// Reads the header.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="headerFields">The header fields.</param>
        /// <returns>
        /// The header.
        /// </returns>
        private static Dictionary<int, MemberInfo> ReadHeader<T>(string[] headerFields)
        {
            var fieldDict = new Dictionary<int, MemberInfo>();
            var csvFields = Utils.GetAllFieldsAndPropertiesOfClassOrdered(typeof(T));
            for (int i = 0; i < headerFields.Length; i++)
            {
                string headerField = headerFields[i];
                if (!ReadHeaderSelectAndAndToDict(csvFields, fieldDict, headerField, i))
                {
                    headerField = headerField.Replace("-", string.Empty);
                    ReadHeaderSelectAndAndToDict(csvFields, fieldDict, headerField, i);
                }

            }
            return fieldDict;
        }

        /// <summary>
        /// Reads header select and to dictionary.
        /// </summary>
        /// <param name="csvFields">  The CSV fields.</param>
        /// <param name="fieldDict">  Dictionary of fields.</param>
        /// <param name="headerField">The header field.</param>
        /// <param name="i">          Zero-based index.</param>
        /// <returns>
        /// True if it succeeds, false if it fails.
        /// </returns>
        private static bool ReadHeaderSelectAndAndToDict(IEnumerable<MemberInfo> csvFields, Dictionary<int, MemberInfo> fieldDict, string headerField, int i)
        {
            var csvField = csvFields.Where(f => f.Name == headerField).FirstOrDefault();
            if (csvField != null)
            {
                fieldDict.Add(i, csvField);
                return true;
            }
            return false;
        }
        #endregion
    }
}
