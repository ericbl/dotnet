using Common.Windows;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Net;

namespace Common.SharePoint
{
    /// <summary>
    /// Connect to a SharePoint site and manage the client context
    /// </summary>
    internal class SPConnector
    {
        private readonly SPParameter parameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SPConnector" /> class.
        /// </summary>
        /// <param name="spParameter">The URI of the SharePoint site. Must be set!</param>
        internal SPConnector(SPParameter spParameter)
        {
            this.parameter = spParameter;
        }

        /// <summary>
        /// Gets the SharePoint folder.
        /// </summary>
        /// <value>
        /// The SharePoint folder.
        /// </value>
        internal string SharepointFolder
        {
            get { return parameter.SharePointFolderPath; }
        }

        /// <summary>
        /// Prepare the SharePoint context and run the action.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="action">The action.</param>
        /// <returns>Object generated</returns>
        /// <exception cref="ArgumentNullException">no action defined</exception>
        internal T PrepareContextAndRunActionOnSP<T>(Func<ClientContext, T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            string sharePointUser = !string.IsNullOrEmpty(parameter.SharePointUserName) ? parameter.SharePointUserName : Environment.UserName;
            if (!string.IsNullOrEmpty(sharePointUser))
                sharePointUser = sharePointUser.ToLowerInvariant();
            else
                throw new Exceptions.HostException($"No user defined for SharePoint authentication!");

            // Retrieve the credentials from CredentialManager, but only from the 'Web Credentials'.
            NetworkCredential networkCredentials = WebCredentialMgr.GetCredential(sharePointUser);

            if (networkCredentials == null)
                throw new Exceptions.HostException($"No credentials found for user {sharePointUser}. Ensure they are stored in the Credential Manager as Web Credentials!");

            // Create the client context and run the action
            T result = default(T);
            using (var clientContext = new ClientContext(parameter.SharePointUri))
            {
                if (networkCredentials != null && !string.IsNullOrEmpty(networkCredentials.UserName))
                {
                    clientContext.Credentials = new SharePointOnlineCredentials(networkCredentials.UserName, networkCredentials.SecurePassword);
                }
                else
                {
                    // default code from MSDN, does not work for SharePoint Online
                    clientContext.AuthenticationMode = ClientAuthenticationMode.Default;
                    clientContext.Credentials = CredentialCache.DefaultCredentials;
                }

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
        internal ListItemCollection GetExistingFilesInFolder(ClientContext clientContext, string fileExtension)
        {
            if (clientContext == null)
                throw new ArgumentNullException(nameof(clientContext));

            if (string.IsNullOrEmpty(fileExtension))
                throw new ArgumentNullException(nameof(fileExtension));

            List documentList = clientContext.Web.Lists.GetByTitle("Dokumente");
            CamlQuery camlQuery = new CamlQuery();
            //camlQuery.ViewXml =
            //    @"<View Scope='Recursive'>
            //        <Query>
            //          <Where>
            //             <And>
            //                <Eq>
            //                  <FieldRef Name='FileDirRef'/><Value Type='Text'>" + parameter.SharePointFolderPath + @"</Value>
            //                </Eq>
            //                <Eq>
            //                  <FieldRef Name='File_x0020_Type'/><Value Type='Text'>" + fileExtension + @"</Value>
            //                </Eq>
            //            </And>
            //          </Where>
            //        </Query>
            //      </View>";
            ICollection<SPListFilter> queryFilters = new List<SPListFilter>(2);
            queryFilters.Add(new SPListFilter { FieldName = "FileDirRef", FieldType = FieldType.Text, FieldValue = parameter.SharePointFolderPath });
            queryFilters.Add(new SPListFilter { FieldName = "File_x0020_Type", FieldType = FieldType.Text, FieldValue = fileExtension });
            camlQuery.ViewXml = DefineWhereQueryString(queryFilters);

            ListItemCollection listItems = documentList.GetItems(camlQuery);
            clientContext.Load(listItems);
            clientContext.ExecuteQuery();

            return listItems;
        }

        /// <summary>
        /// Define query string.
        /// </summary>
        /// <param name="queryFilters">A filter specifying the query.</param>
        /// <returns>
        /// A string.
        /// </returns>
        internal static string DefineWhereQueryString(ICollection<SPListFilter> queryFilters)
        {
            if (queryFilters == null || queryFilters.Count == 0)
                return null;

            string queryString = "<View Scope='Recursive'><Query><Where>";

            //var firstDict = new Dictionary<string, string>(1);
            //var firstKvp = queryFilter.Last();
            //firstDict.Add(firstKvp.Key, firstKvp.Value);
            //queryFilter = firstDict;

            // Use And condition for multiple filters
            bool multiple = queryFilters.Count > 1;
            if (multiple)
                queryString += "<And>";

            foreach (var filter in queryFilters)
            {
                queryString += $"<Eq><FieldRef Name='{filter.FieldName}'/><Value Type='{filter.FieldType}'>{filter.FieldValue}</Value></Eq>";
            }

            if (multiple)
                queryString += "</And>";

            queryString += "</Where></Query></View>";
            return queryString;
        }

        /// <summary>
        /// Define the string for Where/In CAML query.
        /// </summary>
        /// <typeparam name="TVal">Type of the value.</typeparam>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="values">   The values.</param>
        /// <returns>
        /// A string.
        /// </returns>
        internal static string DefineInQueryString<TVal>(string fieldName, ICollection<TVal> values)
        {
            if (string.IsNullOrEmpty(fieldName) || values == null || values.Count == 0)
                return null;

            string valueType = GetSharePointFieldType(typeof(TVal));

            string queryString = $"<View Scope='Recursive'><Query><Where><In><FieldRef Name='{fieldName}'/><Values>";
            foreach (TVal val in values)
            {
                queryString += $"<Value Type='{valueType}'>{val}</Value>";
            }

            queryString += "</Values></In></Where></Query></View>";
            return queryString;
        }

        /// <summary>
        /// Gets the SharePoint field type.
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns>
        /// The SharePoint field type.
        /// </returns>
        internal static string GetSharePointFieldType(Type propertyType)
        {
            string fieldType = null;
            if (propertyType == typeof(string)) // text
                fieldType = "Text";
            else if (propertyType == typeof(int) || propertyType == typeof(int?))
                fieldType = "Integer";
            else if (propertyType == typeof(bool) || propertyType == typeof(bool?))
                fieldType = "Boolean";
            else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                fieldType = "DateTime";
            else if (propertyType == typeof(double) || propertyType == typeof(double?)
                || propertyType == typeof(float) || propertyType == typeof(float?)
                || propertyType == typeof(decimal) || propertyType == typeof(decimal?))
                fieldType = "Number";
            return fieldType;
        }
    }
}
