namespace Common.Credentials
{
    using System.Net;
    using CredentialManagement;

    /// <summary>
    /// Manage the Windows Credentials, as saved in Windows
    /// </summary>
    public static class WindowsCredentialMgr
    {
        /// <summary>
        /// Gets the credential.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>NetworkCredential</returns>
        public static NetworkCredential GetCredential(string userName)
        {
            NetworkCredential result = null;

            using (var cred = new Credential())
            {
                cred.Target = userName;
                cred.Type = CredentialType.Generic;
                bool loaded = cred.Load();
                if (loaded)
                    result = new NetworkCredential(cred.Username, cred.Password);
            }

            return result;
        }

        /// <summary>
        /// Sets the credentials.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="persistenceType">Type of the persistence.</param>
        /// <returns><c>True</c> if saved</returns>
        public static bool SetCredentials(
             string target, string username, string password, PersistanceType persistenceType)
        {
            return new Credential
            {
                Target = target,
                Username = username,
                Password = password,
                PersistanceType = persistenceType
            }.Save();
        }

        /// <summary>
        /// Removes the credentials.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns><c>True</c> if removed</returns>
        public static bool RemoveCredentials(string target)
        {
            return new Credential { Target = target }.Delete();
        }
    }
}
