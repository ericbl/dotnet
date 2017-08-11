namespace Common.Service
{
    using System;
    using System.Diagnostics;
    using System.ServiceProcess;

    /// <summary>
    /// The Windows Service to run the App
    /// </summary>
    /// <seealso cref="ServiceBase" />
    public class ServiceExtended : ServiceBase
    {
        private readonly ServiceStarterBase serviceStarter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceExtended" /> class.
        /// </summary>
        /// <param name="serviceStarter">The service starter.</param>
        /// <param name="serviceName">Name of the service.</param>
        public ServiceExtended(ServiceStarterBase serviceStarter, string serviceName)
        {
            InitializeComponent();
            ServiceName = serviceName;
            this.serviceStarter = serviceStarter;
        }

        ///// <summary>
        ///// Sets the exit code, and if not ok, log and stop.
        ///// </summary>
        ///// <param name="ok">if set to <c>true</c> ok.</param>
        //internal void SetExitCodeLogAndStopOnError(bool ok)
        //{
        //    ExitCode = ok ? 0 : 1;
        //    if (!ok)
        //    {
        //        EventLog.WriteEntry(ServiceStrings.Error + ServiceName, EventLogEntryType.Error);
        //        Stop(); // stop the service.
        //        //RestartService(); // restart the service, auto restart will not get triggered by the clean stop above                
        //    }
        //}

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or
        /// when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry(ServiceName + ServiceStrings.Starting, EventLogEntryType.Information);
            serviceStarter.Start(this);
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM).
        /// Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            serviceStarter.Stop();
            EventLog.WriteEntry(ServiceName + ServiceStrings.Stopped, EventLogEntryType.Information);
        }

        private void InitializeComponent()
        {
            this.AutoLog = false;
            this.CanPauseAndContinue = true;
            this.CanShutdown = true;
        }

        //private void RestartService()
        //{
        //    RestartService(ServiceName);
        //}

        //private static void RestartService(string serviceName)
        //{
        //    ServiceController service = new ServiceController(serviceName);
        //    TimeSpan timeout = TimeSpan.FromMinutes(1);
        //    if (service.Status != ServiceControllerStatus.Stopped)
        //    {
        //        // Stop Service
        //        service.Stop();
        //        service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
        //    }
        //    //Restart service
        //    service.Start();
        //    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
        //}
    }
}
