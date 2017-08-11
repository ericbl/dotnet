using Common.Logging;
using System;
using System.Diagnostics;
using System.Reflection;

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
        /// <param name="extraLogger">(Optional) The extra logger.</param>
        public static void ProcessException(Exception ex, Action<string> serializeMsg, string message = "", ILogger defaultLogger = null, ILogger extraLogger = null)
        {
            if (ex == null)
            {
                ProcessException(new ArgumentNullException(nameof(ex)), null, defaultLogger, extraLogger);
                return;
            }

            // Get message
            message += Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace;
            // Prepare the message to log
            string siteMethodName = GetMethodNameFromSite(ex);
            var fullMsg = "Exception in " + siteMethodName;
            // Get info from source code: useless since duplicate from  ex.StackTrace
            //string stackTraceInfo = string.Empty;
            //var stackTrace = new StackTrace(ex, true);
            //stackTrace.GetFrames().ForEach(frame => stackTraceInfo += "from " + GetMethodAndFileNameAndLineNrFromStackFrame(frame) + Environment.NewLine);
            //if (!string.IsNullOrEmpty(stackTraceInfo))
            //{
            //    fullMsg += stackTraceInfo;
            //}

            fullMsg += message;
            if (defaultLogger == null)
            {
                Trace.TraceError(fullMsg);
            }
            else
            {
                defaultLogger.WriteError(fullMsg);
            }

            if (extraLogger != null)
            {
                extraLogger.WriteError(fullMsg);
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
        /// <param name="extraLogger">(Optional) The extra logger.</param>
        public static void ProcessException(Exception ex, Action<string> serializeMsg, object entryObject, ILogger defaultLogger = null, ILogger extraLogger = null)
        {
            ProcessException(ex, serializeMsg, null, entryObject, defaultLogger, extraLogger);
        }

        /// <summary>
        /// Processes the exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="serializeMsg">The function to serialize the error message or show it on the UI. Input parameters: error msg.</param>
        /// <param name="preProcessSpecialException">The pre process special exception.</param>
        /// <param name="entryObject">The entry object to be documented (using ToString()) when processing an exception.</param>
        /// <param name="defaultLogger">The default logger.</param>
        /// <param name="extraLogger">(Optional) The extra logger.</param>
        public static void ProcessException(Exception ex, Action<string> serializeMsg,
            Action<Exception> preProcessSpecialException, object entryObject,
            ILogger defaultLogger = null, ILogger extraLogger = null)
        {
            preProcessSpecialException?.Invoke(ex);
            ProcessException(ex, serializeMsg, entryObject == null ? string.Empty : "Error in " + entryObject.ToString(), defaultLogger, extraLogger);
        }

        /// <summary>
        /// Executes the method within a try/catch block.
        /// </summary>
        /// <typeparam name="T">.Type of the object</typeparam>
        /// <param name="method">The method.</param>
        /// <param name="serializeMsg">The function to serialize the error message or show it on the UI. Input parameters: error msg.</param>
        /// <param name="entryObject">(Optional) The entry object to be documented (using ToString()) when processing an exception.</param>
        /// <param name="defaultLogger">(Optional) The default logger.</param>
        /// <param name="extraLogger">(Optional) The extra logger.</param>
        /// <returns>
        /// the result of the method.
        /// </returns>
        public static T TryCatchMethod<T>(Func<T> method, Action<string> serializeMsg, object entryObject = null, ILogger defaultLogger = null, ILogger extraLogger = null)
        {
            return TryCatchMethod(method, serializeMsg, null, entryObject, defaultLogger, extraLogger);
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
        /// <param name="extraLogger">(Optional) The extra logger.</param>
        /// <returns>
        /// the result of the method
        /// </returns>
        public static T TryCatchMethod<T>(Func<T> method, Action<string> serializeMsg,
            Action<Exception> preProcessSpecialException, object entryObject = null, ILogger defaultLogger = null, ILogger extraLogger = null)
        {
            T result;
            try
            {
                if (method != null)
                    result = method();
                else
                    result = default(T);
            }
            catch (Exception ex)
            {
                ProcessException(ex, serializeMsg, preProcessSpecialException, entryObject, defaultLogger, extraLogger);
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
        /// <param name="extraLogger">(Optional) The extra logger.</param>
        public static void TryCatchMethod(Action method, Action<string> serializeMsg, object entryObject = null, ILogger defaultLogger = null, ILogger extraLogger = null)
        {
            TryCatchMethod(method, serializeMsg, null, entryObject, defaultLogger, extraLogger);
        }

        /// <summary>
        /// Tries the catch method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="serializeMsg">The serialized message.</param>
        /// <param name="preProcessSpecialException">The pre process special exception.</param>
        /// <param name="entryObject">The entry object.</param>
        /// <param name="defaultLogger">The default logger.</param>
        /// <param name="extraLogger">(Optional) The extra logger.</param>
        public static void TryCatchMethod(Action method, Action<string> serializeMsg,
            Action<Exception> preProcessSpecialException, object entryObject = null, ILogger defaultLogger = null, ILogger extraLogger = null)
        {
            try
            {
                method?.Invoke();
            }
            catch (Exception ex)
            {
                ProcessException(ex, serializeMsg, preProcessSpecialException, entryObject, defaultLogger, extraLogger);
            }
        }

        #region StackFrame
        /// <summary>
        /// Gets the calling method in the n frame before the current one (default = 1)
        /// </summary>
        /// <param name="n">
        /// The Calling Method Number n.
        /// </param>
        /// <returns>
        /// The calling method
        /// </returns>
        public static MethodBase GetCallingMethod(int n = 1)
        {
            return GetStackFrame(n).GetMethod();
        }

        /// <summary>
        /// Gets stack frame.
        /// </summary>
        /// <param name="n">(Optional) The Calling Method Number n.</param>
        /// <returns>The stack frame.</returns>
        public static StackFrame GetStackFrame(int n = 1)
        {
            // get call stack
            var stackTrace = new StackTrace();

            // get calling method name
            var result = stackTrace.GetFrame(n);
            if (result.GetMethod().Name == "Invoke")
            {
                // The stacktrace behaves differently if the app is started from Visual Studio (VS) or from the Filesytem (FS)
                // In FS, the trace is shifted with Invoke/SyncInvoke, thus get the previous frame (-1,-2, -3, -4 always return SyncInvokeXXX!)
                result = stackTrace.GetFrame(n - 1);
            }

            return result;
        }

        /// <summary>
        /// Gets stack frame from the exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>
        /// The stack frame from the exception.
        /// </returns>
        public static StackFrame GetStackFrameFromException(Exception ex)
        {
            // get call stack
            var stackTrace = new StackTrace(ex, true);
            return stackTrace.GetFrame(1);
        }

        /// <summary>
        /// Gets the calling method name, the source file name and line number from the stack trace.
        /// </summary>
        /// <param name="frame">The Calling Method Number n.</param>
        /// <returns>
        /// The calling method name, the source file name and line number from the stack trace.
        /// </returns>
        public static string GetMethodAndFileNameAndLineNrFromStackFrame(StackFrame frame)
        {
            if (frame != null && frame.GetMethod() != null)
                return $"method {frame.GetMethod().Name} in source file {frame.GetFileName()} line {frame.GetFileLineNumber()}";

            return null;
        }

        /// <summary>
        /// Gets the method name from site.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns>The method name from the ex.TargetSite.Name property</returns>
        public static string GetMethodNameFromSite(Exception ex)
        {
            MethodBase site = ex.TargetSite;
            return site == null ? null : site.Name;
        }


        #endregion
    }
}
