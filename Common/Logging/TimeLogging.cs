using System;

namespace Common.Logging
{
    /// <summary>
    /// A time logging: manage the logging of activities with the time elapsed from the beginning
    /// </summary>
    public class TimeLogging
    {
        private readonly ILogger logger;
        private readonly ILogger eventLogger;
        private readonly string callerActivity;
        private readonly DateTime startInitial; // set only once        
        private DateTime start; // reset after sub activity        

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeLogging" /> class.
        /// </summary>
        /// <param name="callerActivity">The main activity/action of the caller.</param>
        /// <param name="serviceEventLog">The service event log.</param>
        public TimeLogging(string callerActivity, System.Diagnostics.EventLog serviceEventLog = null)
            : this(null, callerActivity, serviceEventLog)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeLogging" /> class.
        /// </summary>
        /// <param name="logger">The logger. If null, a TraceLogger will be created.</param>
        /// <param name="callerActivity">The main activity/action of the caller.</param>
        /// <param name="serviceEventLog">The service event log.</param>
        public TimeLogging(ILogger logger, string callerActivity, System.Diagnostics.EventLog serviceEventLog = null)
        {
            this.logger = logger != null ? logger : new TraceLogger();
            this.logger.WriteInfo(Environment.NewLine); // new empty line to separate batches in the log files
            this.eventLogger = serviceEventLog != null
                ? new EventLogger(serviceEventLog)
                : Security.Utils.IsCurrentUserAdmin() ? new EventLogger() : null;
            startInitial = DateTime.Now;
            start = startInitial;
            this.callerActivity = callerActivity;
            // Log start            
            LogCurrentTime("starting");
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public ILogger Logger { get { return logger; } }

        /// <summary>
        /// Gets the event logger.
        /// </summary>
        /// <value>
        /// The event logger.
        /// </value>
        public ILogger EventLogger { get { return eventLogger; } }

        private void LogCurrentTime(string action)
        {
            logger.WriteInfo($"{callerActivity} {action} on {start.ToString("s")} ");
        }

        /// <summary>
        /// Logs the whole process time.
        /// </summary>
        public void LogWholeProcessTime()
        {
            StopTimeCounterLogAndRestart(startInitial, "The whole process");
            LogCurrentTime("ending");
        }

        /// <summary>
        /// Logs the time elapsed since the beginning.
        /// </summary>
        /// <param name="activityName">Name of the activity.</param>
        public void LogTimeElapsedSinceBeginning(string activityName)
        {
            LogTimeElapsedInSeconds(startInitial, logger, activityName);
        }

        /// <summary>
        /// Stops the time counter, log with message, and restart the counter.
        /// </summary>
        /// <param name="activityName">Name of the activity.</param>
        public void StopTimeCounterLogAndRestart(string activityName)
        {
            StopTimeCounterLogAndRestart(start, activityName);
        }

        /// <summary>
        /// Stops the time counter, log with message, and restart the counter.
        /// </summary>
        /// <param name="counterStart">The counter start.</param>
        /// <param name="activityName">Name of the activity.</param>
        public void StopTimeCounterLogAndRestart(DateTime counterStart, string activityName)
        {
            start = LogTimeElapsedInSeconds(counterStart, logger, activityName);
        }

        /// <summary>
        /// Logs the time elapsed (Now-start) in seconds.
        /// </summary>
        /// <param name="start">The start time of the activity.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="activityName">Name of the activity.</param>
        /// <returns>The now time</returns>
        public static DateTime LogTimeElapsedInSeconds(DateTime start, ILogger logger, string activityName)
        {
            DateTime nowTime = DateTime.Now;
            TimeSpan elapsed = nowTime - start;
            var seconds = Math.Round(elapsed.TotalSeconds, 1);
            logger.WriteInfo($"{activityName} took {seconds} seconds");
            return nowTime;
        }
    }
}
