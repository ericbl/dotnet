namespace Common.Service
{
    using Exceptions;
    using Logging;
    using System;
    using System.Globalization;
    using System.Timers;

    /// <summary>
    /// Base class to define the logic to run in the application, manage via a timer so that the application is started only on the given time.
    /// </summary>
    public abstract class ServiceStarterBase
    {
        private static Timer mainTimer = new Timer(60 * 60 * 1000); // 1 hour timer        
        private readonly TimeSpan? runningTime;
        private readonly bool considerTimer;
        private TimeLogging timeLogger;

        /// <summary>
        /// The service caller
        /// </summary>
        private ServiceExtended serviceCaller;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceStarterBase" /> class.
        /// </summary>
        /// <param name="runningTime">If set, run the logic only at the given running time;
        /// otherwise (if null), ignore that and run the logic every hour if considerTimer is true</param>
        /// <param name="considerTimer">if set to <c>true</c> consider the 1 hour timer. Set false to run only once.</param>
        protected ServiceStarterBase(TimeSpan? runningTime, bool considerTimer)
        {
            this.runningTime = runningTime;
            this.considerTimer = considerTimer;
        }

        /// <summary>
        /// Gets the time logger.
        /// </summary>
        /// <value>
        /// The time logger.
        /// </value>
        protected TimeLogging TimeLogger
        {
            get
            {
                if (timeLogger == null)
                {
                    CreateTimeLogger();
                }

                return timeLogger;
            }
        }

        /// <summary>
        /// Starts the program.
        /// </summary>
        /// <param name="windowsService">The Windows service.</param>
        internal void Start(ServiceExtended windowsService)
        {
            serviceCaller = windowsService;
            // instantiate the Logger:
            CreateTimeLogger();
            PrepareParameter();

            if (considerTimer)
            {
                // Launch via the event handler of a 1h timer: only start when the time hour matches the one in the settings, so that it is started only once a day!
                // Run with or without windows service!
                mainTimer.Elapsed += new ElapsedEventHandler(RunWithinTimer);
            }

            if (!considerTimer || windowsService == null)
            {
                // Direct call and launch immediately. Necessary for cmd line start (without windows service), OR for launching a WCF service that runs continuously!
                ExecuteRunAndSetTime();
            }

            mainTimer.Enabled = considerTimer;
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        internal void Stop()
        {
            StopApplicationLogic();
        }

        /// <summary>
        /// Prepares the input parameters.
        /// </summary>
        protected abstract void PrepareParameter();

        /// <summary>
        /// Defines the main activity for logger.
        /// </summary>
        /// <returns>The string defining the activity</returns>
        protected abstract string DefineMainActivityForLogger();

        /// <summary>
        /// Run the main application logic, either at program start or once a day on the given time
        /// </summary>
        /// <returns>Flag whether the program is run successfully</returns>
        protected abstract bool RunApplicationLogic();

        /// <summary>
        /// Stops the application.
        /// </summary>
        /// <returns>Flag whether the program is stopped successfully</returns>
        protected abstract bool StopApplicationLogic();

        /// <summary>
        /// Executes the run and set time.
        /// </summary>
        private void ExecuteRunAndSetTime()
        {
            LogMessage(ServiceStrings.StartInfo);
            if (considerTimer)
                timeLogger.LogAndResetStartInitial(true, true);
            bool ok = RunApplicationLogic();

            //if (serviceCaller != null)
            //{
            //    serviceCaller.SetExitCodeLogAndStopOnError(ok);
            //}

            //if (!ok) // restart the logic in case of failure!
            // RunApplicationLogic();
            if (considerTimer)
                timeLogger.LogAndResetStartInitial(true, false);
        }

        /// <summary>
        /// Event handler of the timer to starts the program only on the given time.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "considerTimer", Justification = "ok")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)", Justification = "ok")]
        private void RunWithinTimer(object source, ElapsedEventArgs e)
        {
            if (!considerTimer)
                throw new HostException($"logic error: the flag '{nameof(considerTimer)}' should be true, is {considerTimer}!");

            if (!runningTime.HasValue || (runningTime.HasValue && DateTime.Now.TimeOfDay.Hours == runningTime.Value.Hours))
            {
                ExecuteRunAndSetTime();
            }
            else
            {
                LogMessage(ServiceStrings.WaitingInfo);
            }
        }

        private void CreateTimeLogger()
        {
            string mainActivity = DefineMainActivityForLogger();
            var eventLog = serviceCaller == null ? null : serviceCaller.EventLog;
            timeLogger = new TimeLogging(mainActivity, eventLog);
        }

        private void LogMessage(string message)
        {
            var currentDate = Strings.Helper.GetCurrentDateFileNameString();
            // Create the EventLogger if required
            if (serviceCaller != null && TimeLogger.EventLogger != null)
                TimeLogger.EventLogger.WriteInfo(message); // time info already in the EventLog
            TimeLogger.Logger.WriteInfo(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", currentDate, message));
        }
    }
}
