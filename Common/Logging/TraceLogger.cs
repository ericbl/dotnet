using System;
using System.Diagnostics;

namespace Common.Logging
{
    /// <summary>
    /// TraceLogger
    /// </summary>
    /// <seealso cref="Common.Logging.ILogger" />
    public class TraceLogger : ILogger
    {
        //TraceSource traceSource; 

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceLogger"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public TraceLogger(string name)
        {
            //traceSource = new TraceSource(name);            
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TraceLogger"/> class.
        /// </summary>
        public TraceLogger()
        {
        }

        /// <summary>
        /// Writes the information.
        /// </summary>
        /// <param name="infoLine">The information line.</param>
        public void WriteInfo(string infoLine)
        {
            Trace.TraceInformation(infoLine);
        }

        /// <summary>
        /// Writes an information to the log
        /// </summary>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        public void WriteInfo(string format, params object[] parameter)
        {
            Trace.TraceInformation(format, parameter);
        }

        /// <summary>
        /// Writes a warning to the log
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void WriteWarning(string message)
        {
            Trace.TraceWarning(message);
        }

        /// <summary>
        /// Writes a warning to the log
        /// </summary>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        public void WriteWarning(string format, params object[] parameter)
        {
            Trace.TraceWarning(format, parameter);
        }

        /// <summary>
        /// Writes an exception as warning to the log
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        public void WriteWarning(Exception ex)
        {
            Trace.TraceWarning(LoggerBase.FormatException(ex));
        }

        /// <summary>
        /// Writes an exception as warning to the log
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        public void WriteWarning(Exception ex, string format, params object[] parameter)
        {
            Trace.TraceWarning(LoggerBase.FormatLogEntry(ex, format, parameter));
        }

        /// <summary>
        /// Writes an error to the log
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void WriteError(string message)
        {
            Trace.TraceError(message);
        }

        /// <summary>
        /// Writes an error to the log
        /// </summary>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        public void WriteError(string format, params object[] parameter)
        {
            Trace.TraceError(format, parameter);
        }

        /// <summary>
        /// Writes an exception to the log
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        public void WriteError(Exception ex)
        {
            Trace.TraceError(LoggerBase.FormatException(ex));
        }

        /// <summary>
        /// Writes an error to the log
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        public void WriteError(Exception ex, string format, params object[] parameter)
        {
            Trace.TraceError(LoggerBase.FormatLogEntry(ex, format, parameter));
        }
    }
}