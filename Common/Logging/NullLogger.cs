using System;
using System.Diagnostics;

namespace Common.Logging
{
    /// <summary>
    /// Instantiating a <see cref="ILogger"/> without initializing Enterprise Library and Unity will cause an exception.
    /// There are various situations where you don't want to deal with this overhead. This class implements the NullObject pattern
    /// and does exactly nothing.
    /// </summary>
    public class NullLogger : ILogger
    {
        /// <summary>
        /// Writes an error to the log
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void WriteError(string message)
        {
        }

        /// <summary>
        /// Writes an error to the log
        /// </summary>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        public void WriteError(string format, params object[] parameter)
        {
        }

        /// <summary>
        /// Writes an error and an associated exception to the log
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        public void WriteError(Exception ex, string format, params object[] parameter)
        {
        }

        /// <summary>
        /// Writes an exception to the log
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        public void WriteError(Exception ex)
        {
        }

        /// <summary>
        /// Writes a warning to the log
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void WriteWarning(string message)
        {
        }

        /// <summary>
        /// Writes a warning to the log
        /// </summary>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        public void WriteWarning(string format, params object[] parameter)
        {
        }

        /// <summary>
        /// Writes a warning and an associated exception to the log
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        public void WriteWarning(Exception ex, string format, params object[] parameter)
        {
        }

        /// <summary>
        /// Writes an exception as warning to the log
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        public void WriteWarning(Exception ex)
        {
        }

        /// <summary>
        /// Writes an information to the log
        /// </summary>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        public void WriteInfo(string format, params object[] parameter)
        {
        }

        /// <summary>
        /// Writes the information.
        /// </summary>
        /// <param name="infoLine">The information line.</param>
        public void WriteInfo(string infoLine)
        {            
        }
    }
}