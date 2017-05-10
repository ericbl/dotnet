using Common.Generic;
using System;
using System.Net;

namespace Common.Security
{
    /// <summary>
    /// A simple credential store to keep the default credential in memory only.
    /// </summary>
    public class CredentialStore : AbstractGenericSingleton<CredentialStore>
    {
        private NetworkCredential defaultCredentials;

        /// <summary>
        /// Gets or sets the default credential.
        /// </summary>
        /// <value>
        /// The default credential.
        /// </value>
        public NetworkCredential DefaultCredential
        {
            get { return defaultCredentials; }
            set { defaultCredentials = value; }
        }

        /// <summary>
        /// Sets default credential.
        /// </summary>
        public static void SetDefaultCredential()
        {
            Instance.DefaultCredential = AskCredential();
        }

        /// <summary>
        /// Ask credential.
        /// </summary>
        /// <param name="domain">     (Optional) The domain.</param>
        /// <param name="defaultUser">(Optional) The default user.</param>
        /// <returns>
        /// A NetworkCredential.
        /// </returns>
        public static NetworkCredential AskCredential(string domain = "kloeckner.com", string defaultUser = null)
        {
            var targetMachineUser = defaultUser;
            if (string.IsNullOrEmpty(defaultUser))
            {
                Console.WriteLine("Admin UserName to work with on the remote machine?");
                targetMachineUser = Console.ReadLine();
            }

            var targetMachinePwd = Utils.GetPasswordFromConsole();

            return new NetworkCredential
            {
                UserName = targetMachineUser,
                SecurePassword = targetMachinePwd,
                Domain = domain
            };
        }


    }
}
