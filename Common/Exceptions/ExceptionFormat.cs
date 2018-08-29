using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Common.Exceptions
{
    /// <summary>
    /// Helper class to dump an exception to a string.
    /// </summary>
    public static class ExceptionFormat
    {
        /// <summary>
        /// Format key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// The formatted key.
        /// </returns>
        public static string FormatKey(object[] key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var sb = new StringBuilder();
            for (int idx = 0; idx < key.Length; idx++)
            {
                if (idx != 0)
                {
                    sb.Append("; ");
                }

                sb.Append(key[idx]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Serialize the given exception to a string.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>
        /// A string representing the exception or null if no exception given.
        /// </returns>
        /// <remarks>The exception will be binary serialized. Data gets encoded as Base64 to avoid issue with special characters.</remarks>
        public static string Serialize(Exception ex)
        {
            if (ex == null)
            {
                return null;
            }

            // Check if the exception is serializable and also the specific ones if generic
            var exceptionType = ex.GetType();
            var allSerializable = exceptionType.IsSerializable;
            if (exceptionType.IsGenericType)
            {
                Type[] typeArguments = exceptionType.GetGenericArguments();
                allSerializable = typeArguments.Aggregate(allSerializable, (current, tParam) => current & tParam.IsSerializable);
            }

            if (!allSerializable)
            {
                // Create a new Exception for not serializable exceptions!
                ex = new HostException(ex.ToString());
            }

            // Serialize the exception
            using (var serialized = new MemoryStream())
            {
                var serialize = new BinaryFormatter();
                serialize.Serialize(serialized, ex);
                return Convert.ToBase64String(serialized.GetBuffer());
            }
        }

        /// <summary>
        /// Deserialize exception.
        /// </summary>
        /// <param name="serializedException">The exception serialized using the <seealso cref="Serialize(Exception)"/> method</param>
        /// <returns>
        /// An Exception.
        /// </returns>
        private static Exception DeserializeException(string serializedException)
        {
            if (string.IsNullOrEmpty(serializedException))
            {
                return null;
            }

            var charBuffer = new char[serializedException.Length];
            serializedException.CopyTo(0, charBuffer, 0, serializedException.Length);
            byte[] buffer = Convert.FromBase64CharArray(charBuffer, 0, serializedException.Length);
            using (var serialized = new MemoryStream(buffer))
            {
                var serialize = new BinaryFormatter();
                return (Exception)serialize.Deserialize(serialized);
            }
        }

        /// <summary>
        /// Deserialize this instance to the given stream.
        /// </summary>
        /// <param name="serializedException">The exception serialized using the <seealso cref="Serialize(Exception)"/> method</param>
        /// <returns>
        /// A HostException.
        /// </returns>
        public static HostException Deserialize(string serializedException)
        {
            if (string.IsNullOrEmpty(serializedException))
            {
                return null;
            }

            var exception = DeserializeException(serializedException);
            return new HostException("Auf dem Server ist während der Verarbeitung der Anfrage ein Fehler aufgetreten.", exception);
        }

        /// <summary>
        /// Deserialize the exception and throw it.
        /// </summary>
        /// <exception cref="HostException">Thrown when the exception is properly de serialized.</exception>
        /// <param name="serializedException">The exception serialized using the <seealso cref="Serialize(Exception)"/> method</param>
        public static void DeserializeAndThrow(string serializedException)
        {
            HostException hostException = Deserialize(serializedException);
            if (hostException != null)
            {
                throw hostException;
            }
        }

        /// <summary>
        /// Compares the exception messages, recursively
        /// </summary>
        /// <param name="firstException">The first exception.</param>
        /// <param name="secondException">The second exception.</param>
        /// <param name="recursiveCheck">If set to <c>true</c> check the inner exceptions recursivy.</param>
        /// <returns>
        ///   <c>true</c> if both exception have the same message tree.
        /// </returns>
        public static bool CompareExceptionMessages(Exception firstException, Exception secondException, bool recursiveCheck)
        {
            if (firstException == null || secondException == null)
            {
                return firstException == null && secondException == null;
            }

            // Compare the messages 
            var equals = firstException.Message == secondException.Message;
            if (recursiveCheck)
            {   // Compare the inner exceptions
                equals &= CompareExceptionMessages(firstException.InnerException, secondException.InnerException, recursiveCheck);
            }

            return equals;
        }

        /// <summary>
        /// Compares the serialized exceptions.
        /// </summary>
        /// <param name="first">The first exception.</param>
        /// <param name="second">The second exception.</param>
        /// <param name="recursiveCheck">If set to <c>true</c> check the inner exceptions recursivy.</param>
        /// <returns><c>true</c> if both exception have the same message tree.</returns>
        public static bool CompareSerializedExceptions(string first, string second, bool recursiveCheck)
        {
            return CompareExceptionMessages(DeserializeException(first), DeserializeException(second), recursiveCheck);
        }
    }
}
