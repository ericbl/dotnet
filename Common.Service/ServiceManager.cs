namespace Common.Service
{
    using System;
    using System.ServiceProcess;

    /// <summary>
    /// Manages the start of the application, either direct or running as a service
    /// </summary>
    public static class ServiceManager
    {
        /// <summary>
        /// Manages the start of the application, either direct or running as a service.
        /// </summary>
        /// <param name="serviceStarter">The service starter.</param>
        /// <param name="serviceName">Name of the service.</param>
        public static void StartManager(ServiceStarterBase serviceStarter, string serviceName)
        {
            if (serviceStarter == null)
                throw new ArgumentNullException(nameof(serviceStarter));

            if (!Environment.UserInteractive)
            {
                // running as service
                using (var service = new ServiceExtended(serviceStarter, serviceName))
                {
                    ServiceBase.Run(service);
                }
            }
            else
            {
                // running as console app
                serviceStarter.Start(null);

                Console.WriteLine(ServiceStrings.FinishMsg);
                Console.ReadLine();

                serviceStarter.Stop();
            }
        }
    }
}
