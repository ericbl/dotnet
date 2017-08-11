using System;
using System.Diagnostics;
using System.Security;

namespace Common.Logging
{
    /// <summary>
    /// Logger to Windows Application Event logs
    /// </summary>
    /// <seealso cref="Common.Logging.ILogger" />
    public class EventLogger : LoggerBase, IDisposable
    {
        private readonly string currentAppName;
        private readonly EventLog eventLog;
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogger" /> class.
        /// </summary>
        public EventLogger()
            : this(AppDomain.CurrentDomain.FriendlyName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogger"/> class, only makes sense if the calling user has administration role!
        /// </summary>
        /// <param name="currentAppName">Name of the current application.</param>
        public EventLogger(string currentAppName)
        {
            if (string.IsNullOrEmpty(currentAppName))
                throw new ArgumentNullException(nameof(currentAppName));

            this.currentAppName = currentAppName;

            // Check if the event source exists, create it if admin role, or use default application
            string eventSource = CreateEventSource(currentAppName);
            // Create an instance of EventLog
            this.eventLog = new EventLog();
            // Set the source name for writing log entries.
            eventLog.Source = eventSource;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogger"/> class.
        /// </summary>
        /// <param name="eventLog">The event log.</param>
        public EventLogger(EventLog eventLog)
        {
            this.eventLog = eventLog;
            this.currentAppName = eventLog.Source;
        }

        /// <summary>
        /// Writes the information.
        /// </summary>
        /// <param name="infoLine">The information line.</param>
        public override void WriteInfo(string infoLine)
        {
            WriteEventLogEntry(infoLine, EventLogEntryType.Information, 22201);
        }

        /// <summary>
        /// Writes a warning to the log
        /// </summary>
        /// <param name="message">The message to log.</param>
        public override void WriteWarning(string message)
        {
            WriteEventLogEntry(message, EventLogEntryType.Warning, 22202);
        }

        /// <summary>
        /// Writes an error to the log
        /// </summary>
        /// <param name="message">The message to log.</param>
        public override void WriteError(string message)
        {
            WriteEventLogEntry(message, EventLogEntryType.Error, 22203);
        }

        #region IDisposable Support
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (eventLog != null)
                        eventLog.Dispose();
                }
                disposedValue = true;
            }
        }
        #endregion

        private void WriteEventLogEntry(string message, EventLogEntryType level, int eventId)
        {
            if (eventLog != null)
            {
                // Write an entry to the event log.
                eventLog.WriteEntry(message, level, eventId);
                // Don't close the eventLog here, it will be done at the end when the instance is disposed            
            }
        }

        private string CreateEventSource(string currentAppName)
        {
            string eventSource = currentAppName;
            bool sourceExists;
            try
            {
                // searching the source throws a security exception ONLY if not exists!
                sourceExists = EventLog.SourceExists(eventSource);
                if (!sourceExists && Security.Utils.IsCurrentUserAdmin(true)) // role check should always return true here, otherwise, the exception is thrown
                {
                    EventLog.CreateEventSource(eventSource, "Application");
                }
            }
            catch (SecurityException)
            {
                eventSource = "Application";
            }

            return eventSource;
        }
    }
}
