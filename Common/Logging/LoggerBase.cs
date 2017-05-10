using Common.Exceptions;
using System;

namespace Common.Logging
{
    /// <summary>
    /// Implements the <seealso cref="ILogger"/> for the formatting options
    /// </summary>
    public abstract class LoggerBase : ILogger
    {
        /// <summary>
        /// Writes the information.
        /// </summary>
        /// <param name="infoLine">The information line.</param>
        public abstract void WriteInfo(string infoLine);

        /// <summary>
        /// Writes a warning to the log
        /// </summary>
        /// <param name="message">The message to log.</param>
        public abstract void WriteWarning(string message);

        /// <summary>
        /// Writes an error to the log
        /// </summary>
        /// <param name="message">The message to log.</param>
        public abstract void WriteError(string message);

        /// <summary>
        /// Writes an information to the log
        /// </summary>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        public void WriteInfo(string format, params object[] parameter)
        {
            WriteInfo(FormatLogEntry(format, parameter));
        }

        /// <summary>
        /// Writes an exception as warning to the log
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        public void WriteWarning(Exception ex)
        {
            WriteWarning(FormatException(ex));
        }

        /// <summary>
        /// Writes a warning to the log
        /// </summary>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        public void WriteWarning(string format, params object[] parameter)
        {
            WriteWarning(FormatLogEntry(format, parameter));
        }

        /// <summary>
        /// Writes an exception as warning to the log
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        public void WriteWarning(Exception ex, string format, params object[] parameter)
        {
            WriteWarning(FormatLogEntry(ex, format, parameter));
        }

        /// <summary>
        /// Writes an exception to the log
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        public void WriteError(Exception ex)
        {
            WriteError(FormatException(ex));
        }

        /// <summary>
        /// Writes an error to the log
        /// </summary>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        public void WriteError(string format, params object[] parameter)
        {
            WriteError(FormatLogEntry(format, parameter));
        }

        /// <summary>
        /// Writes an error to the log
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        public void WriteError(Exception ex, string format, params object[] parameter)
        {
            WriteError(FormatLogEntry(ex, format, parameter));
        }

        /// <summary>
        /// Formats the log entry.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="format">The format.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>the formatted message</returns>
        internal static string FormatLogEntry(Exception ex, string format, params object[] parameter)
        {
            return FormatLogEntry(format, parameter) + Environment.NewLine + FormatException(ex);
        }

        /// <summary>
        /// Formats the log entry.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>the formatted message</returns>
        internal static string FormatLogEntry(string format, params object[] parameter)
        {
            return string.Format(format, parameter);
        }

        /// <summary>
        /// Formats the exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>the formatted message</returns>
        internal static string FormatException(Exception ex)
        {
            return ExceptionFormat.FormatException(ex);
        }
    }
}
