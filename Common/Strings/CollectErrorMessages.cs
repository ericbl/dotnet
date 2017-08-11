using Common.Exceptions;
using Common.Logging;
using Common.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Strings
{
    /// <summary>
    /// Collect string in a list
    /// </summary>
    public class MessagesCollector
    {
        private readonly StringBuilder messagesBuilder;

        /// <summary>
        /// Gets the messages.
        /// </summary>
        /// <value>
        /// The messages.
        /// </value>
        public IList<string> Messages { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagesCollector"/> class.
        /// </summary>
        public MessagesCollector()
        {
            messagesBuilder = new StringBuilder();
            Messages = new List<string>();
        }

        /// <summary>
        /// Gets all messages.
        /// </summary>
        /// <value>
        /// All messages.
        /// </value>
        public string AllMessages
        {
            get
            {
                return messagesBuilder.ToString();
                //var result = "\n";
                //foreach (var m in ErrorMessages)
                //    result += m;
                //result += "\n";
                //return result;
            }
        }

        /// <summary>
        /// Adds the exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public void AddException(Exception ex)
        {
            AddErrorMessage(ExceptionFormat.FormatException(ex));
        }

        /// <summary>
        /// Adds the error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public void AddErrorMessage(string errorMessage)
        {
            Messages.Add(errorMessage);
            messagesBuilder.AppendLine(errorMessage);
        }

        /// <summary>
        /// Logs the messages.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="loggerLevel">The logger level.</param>
        public void LogMessages(ILogger logger, LoggerLevel loggerLevel)
        {
            Helper.LogMessages(Messages, logger, loggerLevel);
        }

        /// <summary>
        /// Serializes all messages.
        /// </summary>
        /// <param name="folderFullPath">The folder full path.</param>
        /// <param name="fileName">Name of the file.</param>
        public void SerializeAllMessages(string folderFullPath, string fileName)
        {
            SerializeUtils.SerializeTxt(folderFullPath, fileName, Messages);
        }
    }
}
