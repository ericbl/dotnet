using Common.Strings;
using System;

namespace Common.Logging
{
    /// <summary>
    /// A time logging: manage the logging of activities with the time elapsed from the beginning
    /// </summary>
    public class TimeLogging
    {
        private readonly ILogger logger;
        private readonly EventLogger eventLogger;
        private readonly string callerActivity;
        private DateTime startInitial; // set at the beginning and only reset when the activity is finished        
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
                : new EventLogger();
            startInitial = DateTime.Now;
            start = startInitial;
            this.callerActivity = callerActivity;
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
        public EventLogger EventLogger { get { return eventLogger; } }

        /// <summary>
        /// Logs the whole process time.
        /// </summary>
        /// <param name="logAlsoOnEventLog">if set to <c>true</c> log also on the event log.</param>
        /// <param name="isStart">if set to <c>true</c> log starting; otherwise log ending and the whole process time!</param>
        public void LogAndResetStartInitial(bool logAlsoOnEventLog, bool isStart)
        {
            if (!isStart)
                StopTimeCounterLogAndRestart(startInitial, "The whole process");
            startInitial = DateTime.Now;
            string action = isStart ? "starting" : "ending";
            LogCallerActivityOnStartInitial(action, logAlsoOnEventLog);
        }

        /// <summary>
        /// Logs the time elapsed since the beginning.
        /// </summary>
        /// <param name="activityName">Name of the activity.</param>
        /// <param name="logAlsoOnEventLog">if set to <c>true</c> log also on the event log.</param>
        /// <param name="logDateTimeAsPrefix">if set to <c>true</c> log the date time as prefix.</param>
        public void LogTimeElapsedSinceBeginning(string activityName, bool logAlsoOnEventLog = false, bool logDateTimeAsPrefix = false)
        {
            LogTimeElapsedInSeconds(startInitial, activityName, logger, GetEventLog(logAlsoOnEventLog), logDateTimeAsPrefix);
        }

        /// <summary>
        /// Stops the time counter, log with message, and restart the counter.
        /// </summary>
        /// <param name="activityName">Name of the activity.</param>
        /// <param name="logAlsoOnEventLog">if set to <c>true</c> log also on the event log.</param>
        /// <param name="logDateTimeAsPrefix">if set to <c>true</c> log the date time as prefix.</param>
        public void StopTimeCounterLogAndRestart(string activityName, bool logAlsoOnEventLog = false, bool logDateTimeAsPrefix = false)
        {
            StopTimeCounterLogAndRestart(start, activityName, logAlsoOnEventLog, logDateTimeAsPrefix);
        }

        /// <summary>
        /// Stops the time counter, log with message, and restart the counter.
        /// </summary>
        /// <param name="counterStart">The counter start.</param>
        /// <param name="activityName">Name of the activity.</param>
        /// <param name="logAlsoOnEventLog">if set to <c>true</c> log also on the event log.</param>
        /// <param name="logDateTimeAsPrefix">if set to <c>true</c> log the date time as prefix.</param>
        private void StopTimeCounterLogAndRestart(DateTime counterStart, string activityName, bool logAlsoOnEventLog = false, bool logDateTimeAsPrefix = false)
        {
            start = LogTimeElapsedInSeconds(counterStart, activityName, logger, GetEventLog(logAlsoOnEventLog), logDateTimeAsPrefix);
        }

        private ILogger GetEventLog(bool logAlsoOnEventLog)
        {
            return logAlsoOnEventLog ? eventLogger : null;
        }

        /// <summary>
        /// Logs the time elapsed (Now-start) in seconds.
        /// </summary>
        /// <param name="start">The start time of the activity.</param>
        /// <param name="activityName">Name of the activity.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="logger2">The logger2.</param>
        /// <param name="logDateTimeAsPrefix">if set to <c>true</c> log the date time as prefix.</param>
        /// <returns>
        /// The now time
        /// </returns>
        private static DateTime LogTimeElapsedInSeconds(DateTime start, string activityName, ILogger logger, ILogger logger2 = null, bool logDateTimeAsPrefix = false)
        {
            DateTime nowTime = DateTime.Now;
            TimeSpan elapsed = nowTime - start;
            var seconds = Math.Round(elapsed.TotalSeconds, 1);
            string logMsg = $"{activityName} took {seconds} seconds";
            if (logDateTimeAsPrefix)
                logMsg = nowTime.ToString("s") + ": " + logMsg;
            logger.WriteInfo(logMsg);
            if (logger2 != null)
                logger2.WriteInfo(logMsg);
            return nowTime;
        }

        private void LogCallerActivityOnStartInitial(string action, bool logAlsoOnEventLog = false)
        {
            string msg = $"{callerActivity} {action} on {startInitial.ToString("s")}";
            logger.WriteInfo(msg);
            if (logAlsoOnEventLog && eventLogger != null)
                eventLogger.WriteInfo(msg);
        }
    }
}
