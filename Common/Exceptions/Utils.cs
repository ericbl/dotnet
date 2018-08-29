using Common.Logging;
using Common.Strings;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Common.Exceptions
{
    /// <summary>
    /// Utilities to manage exceptions and try/catch.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Logs the date and empty line.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="sourceLineNumber">The source line number.</param>
        public static void LogDateAndEmptyLine(
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Trace.WriteLine(Environment.NewLine + DateTime.Now.ToFileNameString(true, true));
            Trace.WriteLine("Starting... " + PrepareMessage(memberName));
        }

        /// <summary>
        /// Get a string from the exception with source, method, stacktrace and inner exceptions.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="message">The input message.</param>
        /// <param name="traceError">if set to <c>true</c> [trace error].</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="sourceLineNumber">The source line number.</param>
        /// <returns></returns>
        public static string ToDetailedString(this Exception ex, string message = "",
            bool traceError = false,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (ex == null)
            {
                if (traceError)
                    ToDetailedString(new ArgumentNullException(nameof(ex)));
                return null;
            }

            // Get message
            message += GetExceptionMessageAndStackTrace(ex, true);
            // Prepare the message to log
            // from MSDN: "If the Source property is not set explicitly, the runtime automatically sets it to the name of the assembly in which the exception originated"
            var fullMsg = "Exception" + PrepareMessage(memberName, ex.Source, sourceFilePath, sourceLineNumber);
            fullMsg += message;
            if (traceError)
                Trace.TraceError(fullMsg);

            return fullMsg;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance: return the MessageText only
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        private static string PrepareMessage(string memberName, string source = null, string sourceFilePath = null, int sourceLineNumber = 0)
        {
            string fullmsg = string.Empty;
            if (!string.IsNullOrEmpty(memberName))
            {
                fullmsg += " from " + memberName;
            }
            if (!string.IsNullOrEmpty(source))
            {
                fullmsg += " in " + source;
            }
            if (!string.IsNullOrEmpty(sourceFilePath))
            {
                fullmsg += " in " + sourceFilePath;
            }
            if (sourceLineNumber > 0)
            {
                fullmsg += " line " + sourceLineNumber;
            }

            return fullmsg;
        }

        /// <summary>
        /// Gets the exception Message and stack trace.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="includeInnerException">if set to <c>true</c> include inner exception recursively.</param>
        /// <returns>
        /// Concatenated string of exception message and stack trace
        /// </returns>
        public static string GetExceptionMessageAndStackTrace(Exception exception,
            bool includeInnerException)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(exception.Message))
                result += Environment.NewLine + exception.Message;
            if (!string.IsNullOrEmpty(exception.StackTrace))
                result += Environment.NewLine + exception.StackTrace;

            if (includeInnerException && exception.InnerException != null)
            {
                // recursive parsing of inner exception
                result += Environment.NewLine + "InnerException: "
                    + GetExceptionMessageAndStackTrace(exception.InnerException, includeInnerException);
            }

            return result;
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
