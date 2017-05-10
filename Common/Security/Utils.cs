using System;
using System.Security;
using System.Security.Principal;

namespace Common.Security
{
    /// <summary>
    /// Security utilities
    /// </summary>
    public static class Utils
    {
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

        /// <summary>
        /// Determines whether the current user has administration role.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the current user has administration role; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCurrentUserAdmin()
        {
            bool isElevated = false;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return isElevated;
        }
    }
}
