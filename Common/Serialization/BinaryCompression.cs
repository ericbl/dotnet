using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace Common.Serialization
{
    /// <summary>
    /// Utilities to manage compression and decompression of an object together with serialization
    /// </summary>
    public static class BinaryCompression
    {
        #region SerializeAndCompress
        /// <summary>
        /// Writes the given object instance to a binary file.
        /// <para>Object type (and all child types) must be decorated with the [Serializable] attribute.</para>
        /// <para>To prevent a variable from being serialized, decorate it with the [NonSerialized] attribute; cannot be applied to properties.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <param name="objectToWrite">The object instance to write to the file.</param>
        /// <param name="filePath">The file path to write the object instance to.</param>
        public static void SerializeAndCompress<T>(T objectToWrite, string filePath) where T : class
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            //var buffer = SerializeAndCompress(collection);
            //File.WriteAllBytes(filePath, buffer);
            SerializeAndCompress(objectToWrite, () => new FileStream(filePath, FileMode.Create), null, () =>
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            });
        }

        /// <summary>
        /// Serialize and compress the given collection
        /// </summary>
        /// <typeparam name="T">Type of the collection</typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns>Compressed array</returns>
        public static byte[] SerializeAndCompress<T>(T collection)
            where T : class
        {
            return SerializeAndCompress(collection, () => new MemoryStream(), st => st.ToArray(), null);
        }

        /// <summary>
        /// Writes the given object instance to a binary file.
        /// <para>Object type (and all child types) must be decorated with the [Serializable] attribute.</para>
        /// <para>To prevent a variable from being serialized, decorate it with the [NonSerialized] attribute; cannot be applied to properties.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <typeparam name="TStream">The type of the stream.</typeparam>
        /// <param name="objectToWrite">The object instance to write to the file.</param>
        /// <param name="createStream">The method to create the first stream. Required. e.g. FileStream, MemoryStream, CryptoStream</param>
        /// <param name="returnMethod">The method to return the byte Array from the first stream. Optional.</param>
        /// <param name="catchAction">The action to run when an Exception occurs. Optional.</param>
        /// <returns>The value from the retuenMethod if specified; null otherwise</returns>
        public static byte[] SerializeAndCompress<T, TStream>(T objectToWrite, Func<TStream> createStream, Func<TStream, byte[]> returnMethod, Action catchAction)
            where T : class
            where TStream : Stream
        {
            if (objectToWrite == null || createStream == null)
            {
                return null;
            }
            byte[] result = null;
            try
            {
                using (var outputStream = createStream())
                {
                    using (var compressionStream = new GZipStream(outputStream, CompressionMode.Compress))
                    {
                        var formatter = new BinaryFormatter();
                        formatter.Serialize(compressionStream, objectToWrite);
                    }
                    if (returnMethod != null)
                        result = returnMethod(outputStream);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(Exceptions.ExceptionFormat.Serialize(ex));
                catchAction?.Invoke();
            }
            return result;
        }
        #endregion

        #region DecompressAndDeserialize
        /// <summary>
        /// Uncompress and de-serializes the data, via a <seealso cref="FileStream"/>, symmetric to <seealso cref="SerializeAndCompress{T}(T, string)"/>.
        /// </summary>
        /// <typeparam name="T">type of the collection</typeparam>
        /// <param name="filePath">The file path.</param>
        /// <returns>The created collection from the de-serialization</returns>
        /// <exception cref="FileNotFoundException">if file does not exists</exception>
        public static T DecompressAndDeserialize<T>(string filePath) 
            where T : class
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            return DecompressAndDeserialize<T, FileStream>(() => new FileStream(filePath, FileMode.Open));
        }

        /// <summary>
        /// Uncompress and de-serializes the data, via a <seealso cref="MemoryStream"/>, symmetric to <seealso cref="SerializeAndCompress{T}(T)"/>.
        /// </summary>
        /// <typeparam name="T">Type of the collection</typeparam>
        /// <param name="compressedStream">The compressed stream.</param>
        /// <returns>List of objects</returns>
        public static T DecompressAndDeserialize<T>(byte[] compressedStream) where T : class
        {
            if (compressedStream == null)
            {
                return null;
            }

            return DecompressAndDeserialize<T, MemoryStream>(() => new MemoryStream(compressedStream));
        }

        /// <summary>
        /// Uncompress and de-serializes the data, symmetric to 
        /// <seealso cref="SerializeAndCompress{T, TStream}(T, Func{TStream}, Func{TStream, byte[]}, Action)"/>.
        /// </summary>
        /// <typeparam name="T">Type of the collection</typeparam>
        /// <typeparam name="TStream">The type of the stream.</typeparam>
        /// <param name="createStream">The create stream.</param>
        /// <returns>
        /// List of objects
        /// </returns>
        /// <exception cref="System.ArgumentException"></exception>
        public static T DecompressAndDeserialize<T, TStream>(Func<TStream> createStream)
            where T : class
            where TStream : Stream
        {
            if (createStream == null)
            {
                return null;
            }

            using (var compressedData = createStream())
            {
                using (GZipStream dataStream = new GZipStream(compressedData, CompressionMode.Decompress))
                {
                    var serializer = new BinaryFormatter();
                    object deserializedObject = serializer.Deserialize(dataStream);
                    if (deserializedObject != null)
                    {
                        var list = deserializedObject as T;
                        if (list == null)
                        {
                            var message = $"Expecting Type: {typeof(T)}, but is {deserializedObject.GetType()} ";
                            throw new ArgumentException(message);
                        }
                        return list;
                    }
                    return null;
                }
            }
        }
        #endregion
    }
}

