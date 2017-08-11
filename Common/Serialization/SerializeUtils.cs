using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <exception cref="ArgumentException">    {folderFullPath}</exception>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="serializationFileParameter">The file parameter.</param>
        /// <param name="serializationParameter">    The input parameter for the serialization.</param>
        /// <param name="action">                    The action.</param>
        /// <returns>
        /// The full path of the written file.
        /// </returns>
        public static string Serialize<T>(
            SerializationFileParameter serializationFileParameter,
            SerializationParameter<T> serializationParameter,
            Action<StreamWriter, SerializationParameter<T>> action)
        {
            if (serializationFileParameter == null)
                throw new ArgumentNullException(nameof(serializationFileParameter));

            if (serializationParameter == null)
                throw new ArgumentNullException(nameof(serializationParameter));

            if (!Directory.Exists(serializationFileParameter.FolderFullPath))
            {
                throw new ArgumentException($"{serializationFileParameter.FolderFullPath} is not a correct folder path");
            }

            var filePath = Path.Combine(serializationFileParameter.FolderFullPath, serializationFileParameter.FileName);
            var fileExists = File.Exists(filePath);
            using (var sw = new StreamWriter(filePath, serializationFileParameter.AppendFile, serializationFileParameter.FileEncoding))
            {
                action(sw, serializationParameter);
            }

            return filePath;
        }

        /// <summary>
        /// Serializes the list as text.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="serializationFileParameter">The file parameter.</param>
        /// <param name="serializationParameter">    The input parameter for the serialization.</param>
        public static void SerializeTxt<T>(SerializationFileParameter serializationFileParameter, SerializationParameter<T> serializationParameter)
        {
            if (serializationFileParameter == null)
                throw new ArgumentNullException(nameof(serializationFileParameter));

            if (serializationParameter == null)
                throw new ArgumentNullException(nameof(serializationParameter));

            Serialize(serializationFileParameter, serializationParameter, (sw, param) =>
            {
                foreach (var record in serializationParameter.Objects)
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
            if (objects != null && objects.Count() > 0)
            {
                var serializationParameter = new SerializationParameter<T>(objects, false);
                var fileParameter = new SerializationFileParameter(folderFullPath, fileName);
                SerializeTxt(fileParameter, serializationParameter);
            }
        }
        #endregion
    }
}
