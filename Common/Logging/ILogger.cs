using System;

namespace Common.Logging
{
    /// <summary>
    /// Interface for logging
    /// </summary>
    public interface ILogger
    {
        #region Public Methods        
        /// <summary>
        /// Writes an information to the log
        /// </summary>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        void WriteInfo(string format, params object[] parameter);

        /// <summary>
        /// Writes the information.
        /// </summary>
        /// <param name="infoLine">The information line.</param>
        void WriteInfo(string infoLine);

        /// <summary>
        /// Writes a warning to the log
        /// </summary>
        /// <param name="message">The message to log.</param>
        void WriteWarning(string message);

        /// <summary>
        /// Writes a warning to the log
        /// </summary>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        void WriteWarning(string format, params object[] parameter);

        /// <summary>
        /// Writes an exception as warning to the log
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        void WriteWarning(Exception ex);

        /// <summary>
        /// Writes an exception as warning to the log
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        void WriteWarning(Exception ex, string format, params object[] parameter);

        /// <summary>
        /// Writes an error to the log
        /// </summary>
        /// <param name="message">The message to log.</param>
        void WriteError(string message);

        /// <summary>
        /// Writes an error to the log
        /// </summary>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        void WriteError(string format, params object[] parameter);

        /// <summary>
        /// Writes an exception to the log
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        void WriteError(Exception ex);


        /// <summary>
        /// Writes an error to the log
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        /// <param name="format">Formating string</param>
        /// <param name="parameter">Parameters of the formating string</param>
        void WriteError(Exception ex, string format, params object[] parameter);

        #endregion
    }
}