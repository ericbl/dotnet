using System;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
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
        /// <param name="checkCurrentRole">if set to <c>true</c> check current role, i.e with UAC enabled, it will return false if not explicitly run as Admin.</param>
        /// <returns>
        ///   <c>true</c> if the current user has administration role; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCurrentUserAdmin(bool checkCurrentRole = true)
        {
            bool isElevated = false;

            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                if (checkCurrentRole)
                {
                    // Even if the user is defined in the Admin group, UAC defines 2 roles: one user and one admin. 
                    // IsInRole consider the current default role as user, thus will return false!
                    // Will consider the admin role only if the app is explicitly run as admin!
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
                else
                {
                    // read all roles for the current identity name, asking ActiveDirectory
                    isElevated = IsAdministratorNoCache(identity.Name);
                }
            }

            return isElevated;
        }

        /// <summary>
        /// Determines whether the specified user is an administrator.
        /// </summary>
        /// <param name="username">The user name.</param>
        /// <returns>
        ///   <c>true</c> if the specified user is an administrator; otherwise, <c>false</c>.
        /// </returns>
        /// <seealso href="https://ayende.com/blog/158401/are-you-an-administrator"/>
        private static bool IsAdministratorNoCache(string username)
        {
            PrincipalContext ctx;
            try
            {
                Domain.GetComputerDomain();
                try
                {
                    ctx = new PrincipalContext(ContextType.Domain);
                }
                catch (PrincipalServerDownException)
                {
                    // can't access domain, check local machine instead 
                    ctx = new PrincipalContext(ContextType.Machine);
                }
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                // not in a domain
                ctx = new PrincipalContext(ContextType.Machine);
            }
            var up = UserPrincipal.FindByIdentity(ctx, username);
            if (up != null)
            {
                PrincipalSearchResult<Principal> authGroups = up.GetAuthorizationGroups();
                return authGroups.Any(principal =>
                                      principal.Sid.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid) ||
                                      principal.Sid.IsWellKnown(WellKnownSidType.AccountDomainAdminsSid) ||
                                      principal.Sid.IsWellKnown(WellKnownSidType.AccountAdministratorSid) ||
                                      principal.Sid.IsWellKnown(WellKnownSidType.AccountEnterpriseAdminsSid));
            }
            return false;
        }
    }
}
