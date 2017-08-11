using Common.Generic;
using Common.Logging;
using Common.Reflection;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Common.SharePoint
{
    /// <summary>
    /// Manage a SharePoint List over CSOM
    /// https://msdn.microsoft.com/en-us/library/office/fp179912(v=office.15).aspx
    /// </summary>
    /// <typeparam name="T">Type of the object in the list</typeparam>
    internal class SPListManager<T>
        where T : class, ISPListItem, new()
    {
        // Class attributes depending only from the input object, but NONE from SharePoint objects which are linked to their context!
        private readonly Dictionary<string, PropertyInfo> objectPropertiesPerName;
        private readonly int itemCountToProcessLimit;
        private readonly ILogger logger;
        private int itemCountToProcess = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SPListManager{T}"/> class.
        /// </summary>
        /// <param name="logger">                   The logger.</param>
        /// <param name="filterWithIgnoreAttribute">True will read the <seealso cref="IgnoreSerializationAttribute"/>
        /// and ignore the properties with that attribute.</param>
        /// <param name="nbrItemsToPushTogether">   (Optional) Number of items to push together to SharePoint to avoid too many requests.</param>
        public SPListManager(ILogger logger, bool filterWithIgnoreAttribute, int nbrItemsToPushTogether = 10)
        {
            this.logger = logger;
            itemCountToProcessLimit = nbrItemsToPushTogether;
            Type type = typeof(T);
            objectPropertiesPerName = Utils.GetAllPropertiesOfClass(type, filterWithIgnoreAttribute).ToDictionary(p => p.Name);
        }

        #region Internal API to read and generate the objects or update a list

        /// <summary>
        /// Read the SharePoint list and generate objects from the list items.
        /// </summary>
        /// <param name="clientContext">The client context.</param>
        /// <param name="listTitle">The list title.</param>
        /// <param name="queryFilters">The filters specifying the query.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <returns>
        /// List of created objects.
        /// </returns>
        internal ListWithMetadata<T> TransformSPListToObjects(ClientContext clientContext, string listTitle, ICollection<SPListFilterBase> queryFilters, string cultureInfo)
        {
            var resultList = new ListWithMetadata<T>(queryFilters: queryFilters);

            // Read list
            var spList = LoadSPList(clientContext, listTitle, false);
            ListItemCollection items = LoadSPListItems(clientContext, spList, queryFilters);

            // Set description
            resultList.SetListProperties(spList.RootFolder.Properties);

            foreach (ListItem listItem in items)
            {
                T createdObj = CreateObjectFromListItem(listItem, cultureInfo);
                if (createdObj != null)
                    resultList.List.Add(createdObj);
            }

            return resultList;
        }

        /// <summary>
        /// Uploads the items to list.
        /// </summary>
        /// <param name="clientContext">        The client context.</param>
        /// <param name="listTitle">            The list title.</param>
        /// <param name="listSource">           The list source.</param>
        /// <param name="createListIfNotExists">if set to <c>true</c> create a list if it does not exist.</param>
        /// <returns>
        /// Number of items modified.
        /// </returns>
        internal int AddOrUploadItemsToList(ClientContext clientContext, string listTitle, ListWithMetadata<T> listSource, bool createListIfNotExists)
        {
            if (clientContext == null)
                throw new ArgumentNullException(nameof(clientContext));

            if (string.IsNullOrEmpty(listTitle))
                throw new ArgumentNullException(nameof(listTitle));

            if (listSource == null || listSource.List == null)
                throw new ArgumentNullException(nameof(listSource));

            if (listSource.List.Count == 0)
                return 0;

            if (createListIfNotExists)
            {
                clientContext.Load(clientContext.Web.Lists);
                clientContext.ExecuteQuery();
                var allListsByTitle = clientContext.Web.Lists.Select(l => l.Title).ToList();
                if (!allListsByTitle.Contains(listTitle))
                    CreateList(clientContext, listTitle, listTitle, objectPropertiesPerName.Values);
            }

            // load the list hier as local variable since it depends from the Context!
            var spList = LoadSPList(clientContext, listTitle, true);

            //#if DEBUG
            //            clientContext.Load(spList.Fields);
            //            clientContext.ExecuteQuery();
            //            var createableFields = FilterFieldsFromObjectProperties(spList.Fields).ToDictionary(f => f.InternalName);
            //            int fieldsOk = createableFields.Count;
            //#endif

            // Update the custom description of the list
            if (listSource.SetSourceDateProperty(spList.RootFolder.Properties))
            {
                spList.RootFolder.Update();
                clientContext.ExecuteQuery();
            }

            HashSet<int> idUpdated = new HashSet<int>();
            int itemsModified = 0;

            if (listSource.QueryFilters != null)
            {
                // item source in dictionary
                Dictionary<int, T> dictSource = listSource.List.Where(src => src.ID > 0).ToDictionary(src => src.ID);

                // Read list
                ListItemCollection items = LoadSPListItems(clientContext, spList, listSource.QueryFilters);
                var itemsToDelete = new List<ListItem>();

                foreach (ListItem listItem in items)
                {
                    // Get source item for that ID
                    if (dictSource.ContainsKey(listItem.Id))
                    {
                        if (UpdateSPListItem(clientContext, dictSource[listItem.Id], listItem, true))
                            itemsModified++;
                        idUpdated.Add(listItem.Id);
                    }

                    //else
                    //{
                    //    // delete row with Id not found in the source
                    //    itemsToDelete.Add(listItem);
                    //}
                }

                // Ensure all items get committed
                ExecuteQueryAndResetCounter(clientContext);

                // Delete items
                //if (itemsToDelete.Count > 0)
                //{
                //    itemsModified += itemsToDelete.Count;
                //    itemsToDelete.DeleteItemsInList(item => item.DeleteObject());
                //    ExecuteQueryAndWait(clientContext);  // delete all at once
                //}
            }

            // Create items
            var itemCreateInfo = new ListItemCreationInformation();
            foreach (T itemSource in listSource.List)
            {
                // id defined in the source but not found in the current SP list: create item!
                ListItem newItem = spList.AddItem(itemCreateInfo);
                if (UpdateSPListItem(clientContext, itemSource, newItem, false))
                    itemsModified++;
            }

            // Ensure all items get committed
            ExecuteQueryAndResetCounter(clientContext);

            logger.WriteInfo($"{itemsModified} items have been updated to the SharePoint list {listTitle}.");

            return itemsModified;
        }

        /// <summary>
        /// Updates the given items in the list identified by their ID by setting the given property with the given value.
        /// </summary>
        /// <param name="clientContext">        The client context.</param>
        /// <param name="listTitle">            The list title.</param>
        /// <param name="idsOflistItemToUpdate">The list items to update.</param>
        /// <param name="propertyName">         Name of the property.</param>
        /// <param name="propertyValue">        The property value.</param>
        /// <returns>
        /// Number of items modified.
        /// </returns>
        internal int UpdateAPropertyOnList(ClientContext clientContext, string listTitle, IList<int> idsOflistItemToUpdate, string propertyName, string propertyValue)
        {
            if (clientContext == null)
                throw new ArgumentNullException(nameof(clientContext));

            if (string.IsNullOrEmpty(listTitle))
                throw new ArgumentNullException(nameof(listTitle));

            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            if (idsOflistItemToUpdate == null || idsOflistItemToUpdate.Count == 0)
                return 0;  // do not do anything for empty list!

            int itemsModified = 0;
            var spList = LoadSPList(clientContext, listTitle, true);

            IList<string> propertiesToLoad = new List<string> { propertyName };
            // Get the items filtered by the ids. The id list shall not be empty, otherwise, all items will be retrieved here!
            var itemsToUpdate = LoadSPListItems(clientContext, spList, "ID", idsOflistItemToUpdate, null);

            foreach (ListItem targetListItem in itemsToUpdate)
            {
                if (targetListItem.FieldValues.ContainsKey(propertyName))
                {
                    targetListItem[propertyName] = propertyValue;
                    targetListItem.Update();
                    itemsModified++;
                }
            }

            /*
            foreach (var id in idsOflistItemToUpdate)
            {
                ListItem targetListItem = spList.GetItemById(id);
                clientContext.Load(targetListItem, item => item[propertyName]);
                clientContext.ExecuteQuery();
                if (targetListItem.FieldValues.ContainsKey(propertyName))
                {
                    targetListItem[propertyName] = propertyValue;
                    targetListItem.Update();
                    itemsModified++;
                }
            }*/

            if (itemsModified > 0)
            {
                ExecuteQueryAndWait(clientContext);
                logger.WriteInfo($"The property {propertyName} has been updated to {propertyValue} for {itemsModified} items on the SharePoint list {listTitle}.");
            }

            return itemsModified;
        }

        /// <summary>
        /// Creates the list.
        /// </summary>
        /// <param name="clientContext">  The client context.</param>
        /// <param name="listTitle">      The list title.</param>
        /// <param name="listDescription">The list description.</param>
        /// <param name="properties">     The properties.</param>
        /// <returns>
        /// <c>True</c> if successfully created.
        /// </returns>
        internal static bool CreateList(ClientContext clientContext, string listTitle, string listDescription, IEnumerable<PropertyInfo> properties)
        {
            // The SharePoint web at the URL.
            Web web = clientContext.Web;

            // Create list
            var creationInfo = new ListCreationInformation();
            creationInfo.Title = listTitle;
            creationInfo.TemplateType = (int)ListTemplateType.GenericList;
            List list = web.Lists.Add(creationInfo);
            list.Description = listDescription;

            list.Update();
            clientContext.ExecuteQuery();

            // Add fields
            foreach (var prop in properties)
            {
                string fieldType = SPConnector.GetSharePointFieldType(prop.PropertyType);
                if (fieldType == null)
                    continue;

                list.Fields.AddFieldAsXml($"<Field DisplayName='{prop.Name}' Type='{fieldType}' />", true, AddFieldOptions.DefaultValue);
            }

            clientContext.ExecuteQuery();

            return true;
        }

        /// <summary>
        /// Moves all items between list source and list destination. Both list must have exactly the same fields!
        /// </summary>
        /// <param name="clientContext">The client context.</param>
        /// <param name="listSourceTitle">The list source title.</param>
        /// <param name="listDestinationTitle">The list destination title.</param>
        /// <returns>Number of items copied</returns>
        internal int MoveAllItemsBetweenList(ClientContext clientContext, string listSourceTitle, string listDestinationTitle)
        {
            // Read lists
            var listDest = LoadSPList(clientContext, listDestinationTitle, false);
            clientContext.Load(listDest.Fields);
            var spList = LoadSPList(clientContext, listSourceTitle, false);
            var itemInListSrc = LoadSPListItems(clientContext, spList, null);
            clientContext.Load(spList.Fields);
            ExecuteQueryAndResetCounter(clientContext);
            var fieldToCopy = FilterCopiableFields(listDest.Fields, spList.Fields);
            //  Copy items
            int copiedItems = 0;
            var itemCreateInfo = new ListItemCreationInformation();
            foreach (var item in itemInListSrc)
            {
                var newItem = listDest.AddItem(itemCreateInfo);
                foreach (var field in fieldToCopy)
                {
                    newItem[field.InternalName] = item[field.InternalName];
                }

                // Update all fields at once
                newItem.Update();

                ExecuteQueryWithCounterCondition(clientContext);
                copiedItems++;
            }

            ExecuteQueryAndResetCounter(clientContext);
            // Remove items from listSource
            for (int intIndex = itemInListSrc.Count - 1; intIndex > -1; intIndex--)
            {
                itemInListSrc[intIndex].DeleteObject();
                ExecuteQueryWithCounterCondition(clientContext);
            }

            ExecuteQueryAndResetCounter(clientContext);
            return copiedItems;
        }
        #endregion

        #region Load list private logic
        private static List LoadSPList(ClientContext clientContext, string listTitle, bool executeQuery)
        {
            var list = clientContext.Web.Lists.GetByTitle(listTitle);
            // load the list and its description.
            clientContext.Load(list, l => l.Description, l => l.RootFolder.Properties);
            if (executeQuery)
                clientContext.ExecuteQuery();

            return list;
        }

        /// <summary>
        /// Loads the item in the main list, either filtered or all if no filter defined.
        /// </summary>
        /// <param name="clientContext">          The client context.</param>
        /// <param name="spList">                 The SharePoint list.</param>
        /// <param name="queryFilters">            A filter specifying the query. If null, all items will be loaded.</param>
        /// <param name="loadThesePropertiesOnly">(Optional) loads these properties only.</param>
        /// <returns>
        /// The list items.
        /// </returns>
        private ListItemCollection LoadSPListItems(
            ClientContext clientContext, List spList, ICollection<SPListFilterBase> queryFilters, IList<string> loadThesePropertiesOnly = null)
        {
            Func<CamlQuery> queryGenerator = () =>
            {
                if (queryFilters != null && queryFilters.Count > 0)
                {
                    clientContext.Load(spList.Fields);
                    clientContext.ExecuteQuery();
                    // transform queryFilter
                    ICollection<SPListFilter> queryFiltersWithType = new List<SPListFilter>(queryFilters.Count);
                    foreach (var filter in queryFilters)
                    {
                        var field = spList.Fields.FirstOrDefault(f => f.Filterable && f.InternalName == filter.FieldName);
                        if (field != null)
                        {
                            queryFiltersWithType.Add(new SPListFilter(filter, field.FieldTypeKind));
                        }
                        else
                        {
                            logger.WriteWarning($"No field found in the list for {filter.FieldName}!");
                        }
                    }

                    var query = new CamlQuery();
                    query.ViewXml = SPConnector.DefineWhereQueryString(queryFiltersWithType);
                    return query;
                }
                else
                {
                    return null;
                }
            };

            return LoadSPListItems(clientContext, spList, queryGenerator, loadThesePropertiesOnly);
        }

        /// <summary>
        /// Loads the item in the main list, either filtered or all if no filter defined.
        /// </summary>
        /// <typeparam name="TVal">Type of the value.</typeparam>
        /// <param name="clientContext">          The client context.</param>
        /// <param name="spList">                 The SharePoint list.</param>
        /// <param name="fieldName">              Name of the field.</param>
        /// <param name="values">                 The values.</param>
        /// <param name="loadThesePropertiesOnly">(Optional) loads these properties only.</param>
        /// <returns>
        /// The list items.
        /// </returns>
        private ListItemCollection LoadSPListItems<TVal>(
            ClientContext clientContext, List spList, string fieldName, ICollection<TVal> values, IList<string> loadThesePropertiesOnly = null)
        {
            Func<CamlQuery> queryGenerator = () =>
            {
                if (!string.IsNullOrEmpty(fieldName) && values != null && values.Count > 0)
                {
                    CamlQuery query = new CamlQuery();
                    query.ViewXml = SPConnector.DefineInQueryString(fieldName, values);
                    return query;
                }
                else
                {
                    return null;
                }
            };

            return LoadSPListItems(clientContext, spList, queryGenerator, loadThesePropertiesOnly);
        }

        private static ListItemCollection LoadSPListItems(
            ClientContext clientContext, List spList, Func<CamlQuery> queryGenerator, IList<string> loadThesePropertiesOnly)
        {
            // Load items, either filtered or all
            CamlQuery query = queryGenerator();
            if (query == null)
                query = CamlQuery.CreateAllItemsQuery();

            ListItemCollection listItems = spList.GetItems(query);
            if (loadThesePropertiesOnly == null || loadThesePropertiesOnly.Count == 0)
            {
                // load all properties
                clientContext.Load(listItems);
            }
            else
            {
                // load only given properties
                var propertiesToLoad = new System.Linq.Expressions.Expression<Func<ListItem, object>>[loadThesePropertiesOnly.Count + 1];
                //propertiesToLoad[0] = item => item.Id;
                for (int i = 0; i < loadThesePropertiesOnly.Count; i++)
                {
                    propertiesToLoad[i] = item => item[loadThesePropertiesOnly[i]];
                }

                clientContext.Load(listItems, items => items.Include(propertiesToLoad));
            }

            // Execute query
            clientContext.ExecuteQuery();

            return listItems;
        }
        #endregion

        #region create logic
        /// <summary>
        /// Creates the object from list item.
        /// </summary>
        /// <param name="listItem">The list item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <returns>The created object</returns>
        private T CreateObjectFromListItem(ListItem listItem, string cultureInfo)
        {
            Dictionary<string, object> currentSpValues = listItem.FieldValues;
            T result = new T();
            foreach (string propertyName in objectPropertiesPerName.Keys)
            {
                if (currentSpValues != null && currentSpValues.ContainsKey(propertyName))
                {
                    object currentSpValue = currentSpValues[propertyName];
                    if (currentSpValue != null)
                    {
                        // check complex type and retrieve only the label
                        if (currentSpValue is Dictionary<string, object>)
                        {
                            currentSpValue = ((Dictionary<string, object>)currentSpValue)["Label"];
                        }

                        // Convert value and set property
                        if (currentSpValue is DateTime || currentSpValue is DateTime?)
                        {
                            var spDate = currentSpValue as DateTime?;
                            if (spDate.HasValue)
                            {
                                // SharePoint saves the date in UTC, we need to convert them back to local time!
                                var shiftedTime = spDate.Value.ToLocalTime();
                                objectPropertiesPerName[propertyName].SetValue(result, shiftedTime);
                            }
                        }
                        else
                        {
                            // convert value to type
                            Type type = objectPropertiesPerName[propertyName].PropertyType;
                            object convertedValue = TypeObjectConverter.ConvertObjectRecord(currentSpValue.ToString(), type, cultureInfo);
                            objectPropertiesPerName[propertyName].SetValue(result, convertedValue);
                        }
                    }
                }
            }

            return result;
        }
        #endregion

        #region Update logic
        /// <summary>
        /// Updates the list item from SharePoint for all properties changed.
        /// </summary>
        /// <param name="clientContext">      The client context.</param>
        /// <param name="itemSource">         The item source.</param>
        /// <param name="listItem">           The list item.</param>
        /// <param name="readCurrentSPValues">if set to <c>true</c> read the current SP values.</param>
        /// <returns>
        /// <c>True</c> if really updated.
        /// </returns>
        private bool UpdateSPListItem(
            ClientContext clientContext, T itemSource, ListItem listItem, bool readCurrentSPValues)
        {
            try
            {
                var result = UpdateSPListItem(itemSource, listItem, readCurrentSPValues);
                if (result)
                    ExecuteQueryWithCounterCondition(clientContext);
                return result;
            }
            catch (Exception ex)
            {
                logger.WriteError(ex, $"Error with item {itemSource.ToString()}");
                return false;
            }
        }

        private static void ExecuteQueryAndWait(ClientContext clientContext)
        {
            clientContext.ExecuteQuery();
#if DEBUG
            //System.Threading.Thread.Sleep(1000 * 3); // wait 3s
#endif
        }

        private void ExecuteQueryWithCounterCondition(ClientContext clientContext)
        {
            // shift execution of the query, but not if limit is only one item.
            if (itemCountToProcess == itemCountToProcessLimit)
                ExecuteQueryAndResetCounter(clientContext);
            else
                itemCountToProcess++;
        }

        private void ExecuteQueryAndResetCounter(ClientContext clientContext)
        {
            // commit
            clientContext.ExecuteQuery();
            itemCountToProcess = 0;
        }

        /// <summary>
        /// Updates the list item from SharePoint for all properties changed
        /// </summary>
        /// <param name="itemSource">The item source.</param>
        /// <param name="listItem">The list item.</param>
        /// <param name="readCurrentSPValues">if set to <c>true</c> read the current SP values.</param>
        /// <returns>
        ///   <c>True</c> if really updated
        /// </returns>
        private bool UpdateSPListItem(T itemSource, ListItem listItem, bool readCurrentSPValues)
        {
            bool updated = false;

            Dictionary<string, object> currentSpValues = null;
            if (readCurrentSPValues)
                currentSpValues = listItem.FieldValues;

            foreach (string propertyName in objectPropertiesPerName.Keys)
            {
                // Ignore properties managed by SharePoint
                if (propertyName == nameof(itemSource.ID) || propertyName == nameof(itemSource.Modified))
                    continue;

                // read source value
                object sourceValue = objectPropertiesPerName[propertyName].GetValue(itemSource);

                if (sourceValue != null)
                {   // convert value to a SharePoint friendly format!
                    if (sourceValue is DateTime || sourceValue is DateTime?)
                    {
                        // SharePoint requires the universal time!
                        sourceValue = ((DateTime)sourceValue).ToUniversalTime();
                    }
                    else if (sourceValue is Enum)
                    {   // save Enum as string instead of int!
                        sourceValue = sourceValue.ToString();
                    }

                    //else if (sourceValue is string)
                    //{   // remove special characters, actually not needed for the SharePoint list, and also not for sending the Email through MS Flow!
                    //    sourceValue = ((string)sourceValue).RemoveOrReplaceSpecialCharactersSharePoint();
                    //}
                }

                // Default update for new source value without any previous value
                bool updateField = currentSpValues == null && sourceValue != null;

                object currentSpValue;
                if (currentSpValues != null && currentSpValues.ContainsKey(propertyName))
                {
                    currentSpValue = currentSpValues[propertyName];
                    updateField = (currentSpValue == null && sourceValue != null)
                        || (sourceValue != null && currentSpValue != null && currentSpValue.ToStringOrNull() != sourceValue.ToStringOrNull());
                }

                if (updateField)
                {
                    listItem[propertyName] = sourceValue;
                    if (!updated)
                        updated = true;
                }
            }

            // Update all fields at once. Otherwise, some get saved and trigger Flow before all are set!
            if (updated)
                listItem.Update();

            return updated;
        }

        private Field[] FilterFieldsFromObjectProperties(FieldCollection fields)
        {
            var sourceInternNames = objectPropertiesPerName.Keys.ToArray();
            return FilterCopiableFields(fields, sourceInternNames);
        }

        private static Field[] FilterCopiableFields(FieldCollection fields, FieldCollection fieldsSource)
        {
            var sourceInternNames = fieldsSource.Select(f => f.InternalName).ToArray();
            return FilterCopiableFields(fields, sourceInternNames);
        }

        private static Field[] FilterCopiableFields(FieldCollection fields, string[] sourceInternNames)
        {
            return fields.Where(field => !field.ReadOnlyField && !field.Hidden
                && field.InternalName != "Attachments" && field.InternalName != "ContentType"
                && sourceInternNames.Contains(field.InternalName)).ToArray();
        }
        #endregion
    }
}
