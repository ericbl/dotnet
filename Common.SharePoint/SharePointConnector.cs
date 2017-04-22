using Common.Logging;
using Common.Windows;
using Microsoft.SharePoint.Client;
using System;
using System.Net;

namespace Common.SharePoint
{
    /// <summary>
    /// Base abstract class to connect to a SharePoint site and manage the client context
    /// </summary>
    public abstract class SharePointConnector
    {
        private readonly string sharepointFolder;
        private readonly ILogger logger;
        private readonly Uri sharepointUri;
        private readonly string credentialUserName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharePointConnector" /> class.
        /// </summary>
        /// <param name="sharepointUri">The URI of the SharePoint site. Must be set!</param>
        /// <param name="credentialUserName">Name of the credential user.</param>
        /// <param name="sharePointFolder">The folder containing the input Xlsx files.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">sharepointUri or folder is null</exception>
        protected SharePointConnector(Uri sharepointUri, string credentialUserName, string sharePointFolder, ILogger logger)
        {
            if (sharepointUri == null)
                throw new ArgumentNullException(nameof(sharepointUri));
            if (string.IsNullOrEmpty(sharePointFolder))
                throw new ArgumentNullException(nameof(sharePointFolder));

            this.sharepointUri = sharepointUri;
            this.credentialUserName = credentialUserName;
            this.sharepointFolder = sharePointFolder;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public ILogger Logger
        {
            get { return logger; }
        }

        /// <summary>
        /// Gets the sharepoint folder.
        /// </summary>
        /// <value>
        /// The sharepoint folder.
        /// </value>
        protected string SharepointFolder
        {
            get { return sharepointFolder; }
        }

        /// <summary>
        /// Read the SharePoint list and generate objects from the list items.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the list</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="listTitle">The list title.</param>
        /// <returns>
        /// List of created objects
        /// </returns>
        public ListWithMetadata<T, TKey> ReadAndTransformSPList<T, TKey>(string listTitle)
            where T : class, new()
        {
            var spListMgr = new SPListManager<T>();
            return PrepareContextAndRunActionOnSP(context => spListMgr.TransformSPListToObjects<TKey>(context, listTitle));
        }

        /// <summary>
        /// Moves the items from list source to list destination.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the list</typeparam>
        /// <param name="listSourceTitle">The list source title.</param>
        /// <param name="listDestinationTitle">The list destination title.</param>
        /// <returns>
        /// Number of items copied
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Required for the SPListManager")]
        public int MoveItemsFromListSourceToListDestination<T>(string listSourceTitle, string listDestinationTitle)
            where T : class, new()
        {
            var spListMgr = new SPListManager<T>();
            return PrepareContextAndRunActionOnSP(context => spListMgr.MoveAllItemsBetweenList(context, listSourceTitle, listDestinationTitle));
        }

        /// <summary>
        /// Prepare the SharePoint context and run the action.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="action">The action.</param>
        /// <returns>Object generated</returns>
        /// <exception cref="ArgumentNullException">no action defined</exception>
        protected T PrepareContextAndRunActionOnSP<T>(Func<ClientContext, T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            NetworkCredential networkCredentials = WebCredentialMgr.GetCredential(credentialUserName);
            T result = default(T);
            using (var clientContext = new ClientContext(sharepointUri))
            {
                clientContext.Credentials = new SharePointOnlineCredentials(networkCredentials.UserName, networkCredentials.SecurePassword);
                result = action(clientContext);
            }

            return result;
        }

        /// <summary>
        /// Reads files in document collection.
        /// <seealso href="https://msdn.microsoft.com/en-us/library/ee956524(office.14).aspx#SP2010ClientOMOpenXml_Retrieving"/>
        /// </summary>
        /// <param name="clientContext">The client context.</param>
        /// <param name="fileExtension">The file extension.</param>
        /// <returns>ListItemCollection</returns>
        /// <exception cref="ArgumentNullException">Any parameter null</exception>
        protected ListItemCollection GetExistingFilesInFolder(ClientContext clientContext, string fileExtension)
        {
            if (clientContext == null)
                throw new ArgumentNullException(nameof(clientContext));

            if (string.IsNullOrEmpty(fileExtension))
                throw new ArgumentNullException(nameof(fileExtension));

            List documentList = clientContext.Web.Lists.GetByTitle("Dokumente");
            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml =
                @"<View Scope='Recursive'>
                    <Query>
                      <Where>
                         <And>
                            <Eq>
                              <FieldRef Name='FileDirRef'/><Value Type='Text'>" + sharepointFolder + @"</Value>
                            </Eq>
                            <Eq>
                              <FieldRef Name='File_x0020_Type'/><Value Type='Text'>" + fileExtension + @"</Value>
                            </Eq>
                        </And>
                      </Where>
                    </Query>
                  </View>";
            ListItemCollection listItems = documentList.GetItems(camlQuery);
            clientContext.Load(listItems);
            clientContext.ExecuteQuery();

            return listItems;
        }
    }
}
