using Common.Generic;
using System;
using System.Net;
using System.Security;

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

            var targetMachinePwd = GetPasswordFromConsole();

            return new NetworkCredential
            {
                UserName = targetMachineUser,
                SecurePassword = targetMachinePwd,
                Domain = domain
            };
        }

        /// <summary>
        /// Gets the password from the console.
        /// </summary>
        /// <returns>Read key from the console and hide them with * character</returns>
        public static SecureString GetPasswordFromConsole()
        {
            SecureString pass = new SecureString();

            Console.Write("Enter your password: ");
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass.AppendChar(key.KeyChar);
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass.RemoveAt(pass.Length - 1);  //pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
            // Stops Receiving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();

            return pass;
        }
    }
}
