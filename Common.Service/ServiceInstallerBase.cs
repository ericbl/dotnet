namespace Common.Service
{
    using System.Configuration.Install;
    using System.ServiceProcess;

    /// <summary>
    /// The service installer: a class must extends this one with the <seealso cref="System.ComponentModel.RunInstallerAttribute"/>!
    /// </summary>
    /// <seealso cref="T:System.Configuration.Install.Installer"/>    
    public class ServiceInstallerBase : Installer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceInstallerBase" /> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="serviceDescription">The service description.</param>
        public ServiceInstallerBase(string serviceName, string serviceDescription)
        {
            using (var serviceProcessInstaller = new ServiceProcessInstaller())
            using (var serviceInstaller = new ServiceInstaller())
            {
                serviceProcessInstaller.Account = ServiceAccount.User;

                serviceInstaller.ServiceName = serviceName;
                serviceInstaller.DisplayName = serviceName;
                serviceInstaller.Description = serviceDescription;
                serviceInstaller.StartType = ServiceStartMode.Automatic;
                serviceInstaller.DelayedAutoStart = true;

                Installers.Add(serviceProcessInstaller);
                Installers.Add(serviceInstaller);
            }
        }
    }
}
