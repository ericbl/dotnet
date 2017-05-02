using Common.Logging;
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

            NetworkCredential networkCredentials = WebCredentialMgr.GetCredential(parameter.SharePointUserName);
            T result = default(T);
            using (var clientContext = new ClientContext(parameter.SharePointUri))
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
            Dictionary<string, string> queryFilter = new Dictionary<string, string>();
            queryFilter.Add("FileDirRef", parameter.SharePointFolderPath);
            queryFilter.Add("File_x0020_Type", fileExtension);
            camlQuery.ViewXml = DefineWhereQueryString(queryFilter);

            ListItemCollection listItems = documentList.GetItems(camlQuery);
            clientContext.Load(listItems);
            clientContext.ExecuteQuery();

            return listItems;
        }

        /// <summary>
        /// Define query string.
        /// </summary>
        /// <param name="queryFilter">A filter specifying the query.</param>
        /// <returns>
        /// A string.
        /// </returns>
        internal static string DefineWhereQueryString(Dictionary<string, string> queryFilter)
        {
            if (queryFilter == null || queryFilter.Count == 0)
                return null;

            string queryString = "<View Scope='Recursive'><Query><Where>";

            bool multiple = queryFilter.Count > 1;
            if (multiple)
                queryString += "<And>";

            foreach (var kvp in queryFilter)
            {
                queryString += $"<Eq><FieldRef Name='{kvp.Key}'/><Value Type='Text'>{kvp.Value}</Value></Eq>";
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
