using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Text;

namespace Common.Exceptions
{
    /// <summary>
    /// Helper class to dump an exception to a string.
    /// </summary>
    public static class ExceptionFormat
    {
        /// <summary>
        /// The exception formatter
        /// </summary>
        private static readonly string ExceptionFormatter =
            "Message:" + Environment.NewLine + "{0}" + Environment.NewLine + Environment.NewLine +
            "Stacktrace:" + Environment.NewLine + "{1}" + Environment.NewLine + Environment.NewLine +
            "Type:" + Environment.NewLine + "{2}";

        private static readonly string ExceptionDataFormatter =
            "Key  : {0}" + Environment.NewLine +
            "Value: {1}" + Environment.NewLine + Environment.NewLine;

        private static readonly string OleDbExceptionFormatter =
            "Index: {0}" + Environment.NewLine +
            "Message: {1}" + Environment.NewLine +
            "NativeError: {2}" + Environment.NewLine +
            "Source: {3}" + Environment.NewLine +
            "SQLState: {4}";

        private static readonly string IdentityFormater = "Name: {0}" + Environment.NewLine + "AuthentificationType: {1}";

        /// <summary>
        /// Format the exception for logging or trace by keeping the message, stack trace, type and inner exceptions
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>
        /// The formatted exception.
        /// </returns>
        public static string FormatException(Exception ex)
        {
            Exception e = ex;
            var sb = new StringBuilder();
            int counter = 0;
            while (e != null && counter < 10)
            {
                sb.AppendFormat(ExceptionFormatter, e.Message, e.StackTrace, e.GetType());
                foreach (DictionaryEntry item in e.Data)
                {
                    sb.AppendFormat(ExceptionDataFormatter, item.Key, item.Value);
                }

                if (e.GetType() == typeof(System.Data.OleDb.OleDbException))
                {
                    var oleEx = (System.Data.OleDb.OleDbException)e;
                    if (oleEx.Errors != null)
                    {
                        for (int idx = 0; idx < oleEx.Errors.Count; idx++)
                        {
                            sb.AppendFormat(OleDbExceptionFormatter, idx, oleEx.Errors[idx].Message, oleEx.Errors[idx].NativeError, oleEx.Errors[idx].Source, oleEx.Errors[idx].SQLState);
                        }
                    }
                }

                System.Reflection.ReflectionTypeLoadException reflectionEx = e as System.Reflection.ReflectionTypeLoadException;
                if (reflectionEx != null)
                {
                    foreach (var loaderEx in reflectionEx.LoaderExceptions)
                    {
                        sb.AppendFormat(ExceptionFormatter, loaderEx.Message, loaderEx.StackTrace, loaderEx.GetType());
                    }
                }

                e = e.InnerException;
                if (e != null)
                {
                    sb.AppendFormat("{0}{0}Inner Exception:{0}{0}", Environment.NewLine);
                }

                counter++;
            }
            if (counter == 10 && e != null)
            {
                sb.AppendLine();
                sb.AppendLine("No further Inner-Exceptions!");
            }

            sb.AppendLine();
            // Gebe Zeitpunkt aus um Logfiles auswerden zu können
            sb.AppendFormat("Time: {1}{0}", Environment.NewLine, DateTime.Now);
            sb.AppendLine();
            // Gebe Identität aus, um Sicherheitsprobleme zu untersuchen 
            sb.AppendLine(IdentityDescription());

            return sb.ToString();
        }

        /// <summary>
        /// Get information about the current Windows Identity (user) running the process.
        /// </summary>
        /// <returns>
        /// A string representing the user
        /// </returns>
        public static string IdentityDescription()
        {
            var identity = WindowsIdentity.GetCurrent();
            return identity != null ? string.Format(IdentityFormater, identity.Name, identity.AuthenticationType) : null;
        }

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
                ex = new Exception(FormatException(ex));
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
        public static Exception Deserialize(string serializedException)
        {
            if (string.IsNullOrEmpty(serializedException))
            {
                return null;
            }

            return DeserializeException(serializedException);
        }

        /// <summary>
        /// Deserialize the exception and throw it.
        /// </summary>
        /// <exception cref="Exception">Thrown when the exception is properly de serialized.</exception>
        /// <param name="serializedException">The exception serialized using the <seealso cref="Serialize(Exception)"/> method</param>
        public static void DeserializeAndThrow(string serializedException)
        {
            Exception hostException = Deserialize(serializedException);
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
