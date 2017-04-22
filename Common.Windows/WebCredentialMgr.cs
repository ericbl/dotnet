using System;
using System.Collections.Generic;
using System.Net;
using Windows.Security.Credentials;

namespace Common.Windows
{
    /// <summary>
    /// Manager of Web credentials saved into Windows
    /// </summary>
    public static class WebCredentialMgr
    {
        /// <summary>
        /// Gets the credential.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>NetworkCredential</returns>
        public static NetworkCredential GetCredential(string userName)
        {
            PasswordCredential credential = GetCredentialFromLocker(userName);
            if (credential == null)
                return null;

            credential.RetrievePassword();

            var networkCred = new NetworkCredential(credential.UserName, credential.Password);
            return networkCred;
        }

        private static PasswordCredential GetCredentialFromLocker(string userName)
        {
            PasswordCredential credential = null;
            IReadOnlyList<PasswordCredential> credentialList = null;

            var vault = new PasswordVault();
            try
            {
                credentialList = vault.FindAllByUserName(userName);
            }
            catch
            {
            }

            if (credentialList == null)
                credentialList = vault.RetrieveAll();

            if (credentialList != null && credentialList.Count > 0)
            {
                if (credentialList.Count == 1)
                {
                    credential = credentialList[0];
                }
                else
                {
                    //// When there are multiple usernames,
                    //// retrieve the default username. If one doesn’t
                    //// exist, then display UI to have the user select
                    //// a default username.

                    //defaultUserName = GetDefaultUserNameUI();

                    //credential = vault.Retrieve(resourceName, defaultUserName);
                    foreach (var cred in credentialList)
                    {
                        Console.WriteLine(cred.UserName);
                    }
                }
            }

            return credential;
        }
    }
}
