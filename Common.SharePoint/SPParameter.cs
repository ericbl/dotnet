namespace Common.SharePoint
{
    using System;

    /// <summary>
    /// SharePoint parameters: uri, username to log into the page (requires Credentials to be stored in Windows!) and paths to the relevant pages.
    /// </summary>
    public struct SPParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SPParameter"/> struct.
        /// </summary>
        /// <param name="sharepointUri">       The SharePoint URI.</param>
        /// <param name="sharePointUserName">  Name of the share point user.</param>
        /// <param name="sharePointFolderPath">The default approver email.</param>
        public SPParameter(string sharepointUri, string sharePointUserName, string sharePointFolderPath = null)
        {
            if (sharepointUri == null)
                throw new ArgumentNullException(nameof(sharepointUri));
            if (string.IsNullOrEmpty(sharePointUserName))
                throw new ArgumentNullException(nameof(sharePointUserName));

            SharePointUri = new Uri(sharepointUri);
            SharePointUserName = sharePointUserName;
            SharePointFolderPath = sharePointFolderPath;
        }

        /// <summary>
        /// Gets the URI to the SharePoint site.
        /// </summary>
        /// <value>
        /// The SharePoint URI.
        /// </value>
        public Uri SharePointUri { get; private set; }

        /// <summary>
        /// Gets the name of user for SharePoint. His credentials should be saved in Windows.WebCredentials!
        /// </summary>
        /// <value>
        /// The name of the SharePoint user.
        /// </value>
        public string SharePointUserName { get; private set; }

        /// <summary>
        /// Gets the full pathname of the SharePoint folder file.
        /// </summary>
        /// <value>
        /// The full pathname of the SharePoint folder file.
        /// </value>
        public string SharePointFolderPath { get; private set; }
    }
}
