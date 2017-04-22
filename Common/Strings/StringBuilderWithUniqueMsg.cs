using Common.Exceptions;
using Common.Extensions;
using Common.Generic;
using Common.Logging;
using System.Collections.Generic;
using System.Text;

namespace Common.Strings
{
    /// <summary>
    /// StringBuilder to append message only once, a Hashset of string ensure the message uniqueness
    /// </summary>
    public class StringBuilderWithUniqueMsg
    {
        /// <summary>
        /// Intern StringBuilder
        /// </summary>
        private readonly StringBuilder stringBuilder;

        /// <summary>
        /// Store each message as key to ensure uniqueness
        /// </summary>
        private readonly StringCollectionWithHistogram msgHistogram;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringBuilderWithUniqueMsg"/> class.
        /// </summary>
        public StringBuilderWithUniqueMsg()
        {
            stringBuilder = new StringBuilder();
            msgHistogram = new StringCollectionWithHistogram();
        }

        /// <summary>
        /// Formats the message and Appends it as a new line, only if not already added
        /// </summary>
        /// <param name="stringFormat">The string format.</param>
        /// <param name="args">The args.</param>
        public void AppendLine(string stringFormat, params object[] args)
        {
            AppendLine(string.Format(stringFormat, args));
        }

        /// <summary>
        /// Appends the message as a new line, if not null or empty, and only if not already added
        /// </summary>
        /// <param name="msg">The message.</param>
        public void AppendLine(string msg)
        {
            if (msgHistogram.Add(msg))
            {
                stringBuilder.AppendLine(msg.Trim());
            }
        }

        /// <summary>
        /// Gets the output string, with one line pro message
        /// </summary>
        /// <value>The output string.</value>
        public string OutputString
        {
            get
            {
                return HasMessage ? stringBuilder.ToString() : null;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return OutputString;
        }

        /// <summary>
        /// Gets all messages.
        /// </summary>
        /// <value>All messages.</value>
        /// <returns>The collection of message</returns>
        public IEnumerable<string> AllMessages
        {
            get
            {
                return msgHistogram.GetAllMessagesSorted(StringCollectionWithHistogram.SortOrder.None);
            }
        }

        /// <summary>
        /// Gets the number of errors.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get { return msgHistogram.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance contains message(s).
        /// </summary>
        /// <value><c>true</c> if this instance has message; otherwise, <c>false</c>.</value>
        public bool HasMessage
        {
            get { return Count > 0; }
        }

        /// <summary>
        /// Throws an UserException with the output string.
        /// </summary>
        public void ThrowExceptionWithOutputStringIfHasMessage()
        {
            if (HasMessage)
            {
                throw new UserException(OutputString, ExceptionType.Error);
            }
        }

        /// <summary>
        /// Logs the messages.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="loggerLevel">The logger level.</param>
        public void LogMessages(ILogger logger, LoggerLevel loggerLevel)
        {
            switch (loggerLevel)
            {
                case LoggerLevel.Info:
                    AllMessages.ForEach(msg => logger.WriteInfo(msg));
                    break;
                case LoggerLevel.Warning:
                    AllMessages.ForEach(msg => logger.WriteWarning(msg));
                    break;
                case LoggerLevel.Error:
                    AllMessages.ForEach(msg => logger.WriteError(msg));
                    break;
                default:
                    logger.WriteError($"Wrong enum value {nameof(loggerLevel)}: {loggerLevel}");
                    break;
            }
        }
    }
}
