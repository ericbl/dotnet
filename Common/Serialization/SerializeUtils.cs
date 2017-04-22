using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common.Serialization
{
    /// <summary>
    /// Static and generic utilities for the serialization.
    /// </summary>
    public static class SerializeUtils
    {
        #region Serialization
        /// <summary>
        /// Serialize objects to file.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="folderFullPath">The folder full path.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="append">if set to <c>true</c> append to existing file. Otherwise, overwrite the file.</param>
        /// <param name="exportHeader">if set to <c>true</c> export the header while serializing.</param>
        /// <param name="sortTFields">if set to <c>true</c> [sort t fields].</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="objects">The objects.</param>
        /// <param name="action">The action.</param>
        /// <returns>
        /// The full path of the written file
        /// </returns>
        /// <exception cref="ArgumentException">{folderFullPath}</exception>
        public static string Serialize<T>(string folderFullPath, string fileName, bool append, bool exportHeader, bool sortTFields,
            Encoding encoding, IEnumerable<T> objects, Action<StreamWriter, IEnumerable<T>, bool, bool> action)
        {
            if (!Directory.Exists(folderFullPath))
            {
                throw new ArgumentException($"{folderFullPath} is not a correct folder path");
            }

            var filePath = Path.Combine(folderFullPath, fileName);
            var fileExists = File.Exists(filePath);
            using (var sw = new StreamWriter(filePath, append, encoding))
            {
                action(sw, objects, exportHeader, sortTFields);
            }

            return filePath;
        }

        /// <summary>
        /// Serializes the list as text.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="folderFullPath">The folder full path.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="append">if set to <c>true</c> [append].</param>
        /// <param name="exportHeader">if set to <c>true</c> [export header].</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="objects">The objects.</param>
        public static void SerializeTxt<T>(string folderFullPath, string fileName, bool append, bool exportHeader,
            Encoding encoding, IEnumerable<T> objects)
        {
            Serialize(folderFullPath, fileName, append, exportHeader, true, encoding, objects, (sw, objs, flag, sort) =>
            {
                foreach (var record in objs)
                {
                    sw.WriteLine(record.ToString());
                }
            });
        }

        /// <summary>
        /// Serializes the list as text.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="folderFullPath">The folder full path.</param>
        /// <param name="fileName">Name of the file.</param>        
        /// <param name="objects">The objects.</param>
        public static void SerializeTxt<T>(string folderFullPath, string fileName, IEnumerable<T> objects)
        {
            SerializeTxt(folderFullPath, fileName, false, false, Encoding.Default, objects);
        }
        #endregion
    }
}
