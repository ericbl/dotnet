using Common.Logging;
using System;
using System.Diagnostics;

namespace Common.Exceptions
{
    /// <summary>
    /// Utilities to manage exceptions and try/catch.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Processes the exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="serializeMsg">The function to serialize the error message or show it on the UI. Input parameters: error msg.</param>
        /// <param name="message">The input message.</param>
        /// <param name="defaultLogger">The default logger.</param>
        public static void ProcessException(Exception ex, Action<string> serializeMsg, string message = "", ILogger defaultLogger = null)
        {
            if (ex == null)
            {
                ProcessException(new ArgumentNullException(nameof(ex)), null);
                return;
            }

            message += "\n" + ex.Message
                + "\n" + ex.StackTrace;
            //string methodName = Helper.Utils.GetCallingMethodNameOnStackTrace(2);
            string methodName = Helper.Utils.GetMethodNameFromSite(ex);
            var fullMsg = "Exception in " + methodName + message;
            if (defaultLogger == null)
            {
                Trace.TraceError(fullMsg);
            }
            else
            {
                defaultLogger.WriteError(fullMsg);
            }
            serializeMsg?.Invoke(fullMsg);
        }

        /// <summary>
        /// Processes the exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="serializeMsg">The function to serialize the error message or show it on the UI. Input parameters: error msg.</param>
        /// <param name="entryObject">The entry object to be documented (using ToString()) when processing an exception.</param>
        /// <param name="defaultLogger">The default logger.</param>
        public static void ProcessException(Exception ex, Action<string> serializeMsg, object entryObject, ILogger defaultLogger = null)
        {
            ProcessException(ex, serializeMsg, null, entryObject, defaultLogger);
        }

        /// <summary>
        /// Processes the exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="serializeMsg">The function to serialize the error message or show it on the UI. Input parameters: error msg.</param>
        /// <param name="preProcessSpecialException">The pre process special exception.</param>
        /// <param name="entryObject">The entry object to be documented (using ToString()) when processing an exception.</param>
        /// <param name="defaultLogger">The default logger.</param>
        public static void ProcessException(Exception ex, Action<string> serializeMsg,
            Action<Exception> preProcessSpecialException, object entryObject,
            ILogger defaultLogger = null)
        {
            preProcessSpecialException?.Invoke(ex);
            ProcessException(ex, serializeMsg, entryObject == null ? string.Empty : "Error in " + entryObject.ToString(), defaultLogger);
        }

        /// <summary>
        /// Executes the method within a try/catch block.
        /// </summary>
        /// <typeparam name="T">.Type of the object</typeparam>
        /// <param name="method">       The method.</param>
        /// <param name="serializeMsg"> The function to serialize the error message or show it on the UI. Input parameters: error msg.</param>
        /// <param name="entryObject">  (Optional) The entry object to be documented (using ToString()) when processing an exception.</param>
        /// <param name="defaultLogger">(Optional) The default logger.</param>
        /// <returns>
        /// the result of the method.
        /// </returns>
        public static T TryCatchMethod<T>(Func<T> method, Action<string> serializeMsg, object entryObject = null, ILogger defaultLogger = null)
        {
            return TryCatchMethod(method, serializeMsg, null, entryObject, defaultLogger);
        }

        /// <summary>
        /// Executes the method within a try/catch block.
        /// </summary>
        /// <typeparam name="T">.Type of the object</typeparam>
        /// <param name="method">The method.</param>
        /// <param name="serializeMsg">The function to serialize the error message or show it on the UI. Input parameters: error msg.</param>
        /// <param name="preProcessSpecialException">The pre process method for special exception.</param>
        /// <param name="entryObject">The entry object to be documented (using ToString()) when processing an exception.</param>
        /// <param name="defaultLogger">The default logger.</param>
        /// <returns>
        /// the result of the method
        /// </returns>
        public static T TryCatchMethod<T>(Func<T> method, Action<string> serializeMsg,
            Action<Exception> preProcessSpecialException, object entryObject = null, ILogger defaultLogger = null)
        {
            T result;
            try
            {
                result = method();
            }
            catch (Exception ex)
            {
                ProcessException(ex, serializeMsg, preProcessSpecialException, entryObject, defaultLogger);
                result = default(T);
            }
            return result;
        }

        /// <summary>
        /// Executes the method within a try/catch block.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="serializeMsg">The function to serialize the error message or show it on the UI. Input parameters: error msg.</param>
        /// <param name="entryObject">The entry object to be documented (using ToString()) when processing an exception.</param>
        /// <param name="defaultLogger">The default logger.</param>
        public static void TryCatchMethod(Action method, Action<string> serializeMsg, object entryObject = null, ILogger defaultLogger = null)
        {
            TryCatchMethod(method, serializeMsg, null, entryObject, defaultLogger);
        }

        /// <summary>
        /// Tries the catch method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="serializeMsg">The serialize MSG.</param>
        /// <param name="preProcessSpecialException">The pre process special exception.</param>
        /// <param name="entryObject">The entry object.</param>
        /// <param name="defaultLogger">The default logger.</param>
        public static void TryCatchMethod(Action method, Action<string> serializeMsg,
            Action<Exception> preProcessSpecialException, object entryObject = null, ILogger defaultLogger = null)
        {
            try
            {
                method();
            }
            catch (Exception ex)
            {
                ProcessException(ex, serializeMsg, preProcessSpecialException, entryObject, defaultLogger);
            }
        }
    }
}
