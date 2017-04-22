using Common.Generic;
using Common.Logging;
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
    public class SPListManager<T>
        where T : class, new()
    {
        private readonly Dictionary<string, PropertyInfo> objectPropertiesPerName;
        private readonly int itemCountToProcessLimit = 10;
        private List spList;
        private int itemCountToProcess = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SPListManager{T}"/> class.
        /// </summary>
        public SPListManager()
        {
            Type type = typeof(T);
            objectPropertiesPerName = Reflection.Utils.GetAllPropertiesOfClass(type).ToDictionary(p => p.Name);
        }

        #region Internal API to read and generate the objects or update a list
        /// <summary>
        /// Read the SharePoint list and generate objects from the list items.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="clientContext">The client context.</param>
        /// <param name="listTitle">The list title.</param>
        /// <returns>
        /// List of created objects
        /// </returns>
        internal ListWithMetadata<T, TKey> TransformSPListToObjects<TKey>(ClientContext clientContext, string listTitle)
        {
            var resultList = new ListWithMetadata<T, TKey>();

            // Read list
            ListItemCollection items = LoadSPListAndItems(clientContext, listTitle);

            // Set description
            resultList.SetListProperties(spList.RootFolder.Properties);

            foreach (ListItem listItem in items)
            {
                T createdObj = CreateObjectFromListItem(listItem);
                if (createdObj != null)
                    resultList.List.Add(createdObj);
            }

            return resultList;
        }

        /// <summary>
        /// Uploads the items to list.
        /// </summary>
        /// <param name="clientContext">The client context.</param>
        /// <param name="listTitle">The list title.</param>
        /// <param name="listSource">The list source.</param>
        /// <param name="createListIfNotExists">if set to <c>true</c> create a list if it does not exist.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>
        /// Number of items modified
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "design")]
        internal int UploadItemsToList(
            ClientContext clientContext,
            string listTitle,
            ListWithMetadata<T, int> listSource,
            bool createListIfNotExists,
            ILogger logger)
        {
            if (createListIfNotExists)
            {
                clientContext.Load(clientContext.Web.Lists);
                clientContext.ExecuteQuery();
                var allListsByTitle = clientContext.Web.Lists.Select(l => l.Title).ToList();
                if (!allListsByTitle.Contains(listTitle))
                    CreateList(clientContext, listTitle, listTitle, objectPropertiesPerName.Values);
            }

            // Read list
            ListItemCollection items = LoadSPListAndItems(clientContext, listTitle);

            // Update the custom description of the list
            if (listSource.SetSourceDateProperty(spList.RootFolder.Properties))
            {
                spList.RootFolder.Update();
                clientContext.ExecuteQuery();
            }

            HashSet<int> idUpdated = new HashSet<int>();
            int itemsModified = 0;
            var itemsToDelete = new List<ListItem>();

            //var filteredItems = items.Where(u => u["Company"] != null && (u["Company"].ToString() == "DARO" || u["Company"].ToString() == "DKM")).ToList();
            //itemsToDelete.AddRange(filteredItems);
            var dictSource = listSource.GetDictionaryFromList();

            foreach (ListItem listItem in items)
            {
                // Get only for id!
                var idObj = listItem[listSource.IdFieldName];
                int itemId;
                if (idObj != null && int.TryParse(idObj.ToString(), out itemId))
                {
                    if (!string.IsNullOrEmpty(listSource.SecondIdFieldName) && !string.IsNullOrEmpty(listSource.SecondIdFieldValueFilter))
                    {
                        object secondValueItem = listItem[listSource.SecondIdFieldName];
                        bool filterOnSecondField = secondValueItem != null && secondValueItem.ToString() == listSource.SecondIdFieldValueFilter;
                        if (!filterOnSecondField)
                        {   // ad id to the set but ignore it from the update since the second requested field does not match!
                            idUpdated.Add(itemId);
                            continue;
                        }
                    }

                    if (dictSource.ContainsKey(itemId) && !idUpdated.Contains(itemId))
                    {   // only one item per Id allowed!
                        var itemSource = dictSource[itemId];
                        if (UpdateSPListItem(clientContext, itemSource, itemId, listItem, true, logger))
                            itemsModified++;
                        idUpdated.Add(itemId);
                    }
                    else
                    {
                        // delete row with Id not found in the source or if
                        itemsToDelete.Add(listItem);
                    }
                }
            }

            // Ensure all items get committed
            ExecuteQueryAndResetCounter(clientContext);

            // Delete items
            if (itemsToDelete.Count > 0)
            {
                itemsModified += itemsToDelete.Count;
                itemsToDelete.DeleteItemsInList(item => item.DeleteObject());
                ExecuteQueryAndResetCounter(clientContext);  // delete all at once
            }

            // Create items
            var itemCreateInfo = new ListItemCreationInformation();
            var allNewIds = dictSource.Keys.Where(id => !idUpdated.Contains(id)).ToList();
            foreach (int id in allNewIds)
            {
                // id defined in the source but not found in the current SP list: create item!
                var newItem = spList.AddItem(itemCreateInfo);
                if (UpdateSPListItem(clientContext, dictSource[id], id, newItem, false, logger))
                    itemsModified++;
            }

            // Ensure all items get committed
            ExecuteQueryAndResetCounter(clientContext);

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
                string fieldType = GetSharePointFieldType(prop.PropertyType);
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
            var listDest = LoadSPListOnlyWithoutExecution(clientContext, listDestinationTitle);
            clientContext.Load(listDest.Fields);
            var itemInListSrc = LoadSPListAndItems(clientContext, listSourceTitle);
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
                    newItem.Update();
                }

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
        private ListItemCollection LoadSPListAndItems(ClientContext clientContext, string listTitle)
        {
            spList = LoadSPListOnlyWithoutExecution(clientContext, listTitle);
            // Load all items
            CamlQuery query = CamlQuery.CreateAllItemsQuery();
            ListItemCollection items = spList.GetItems(query);
            clientContext.Load(items);
            // Execute query
            clientContext.ExecuteQuery();

            return items;
        }

        private static List LoadSPListOnlyWithoutExecution(ClientContext clientContext, string listTitle)
        {
            var list = clientContext.Web.Lists.GetByTitle(listTitle);
            // load the list and its description.
            clientContext.Load(list, l => l.Description, l => l.RootFolder.Properties);
            return list;
        }
        #endregion

        #region create logic
        private static string GetSharePointFieldType(Type propertyType)
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

        private T CreateObjectFromListItem(ListItem listItem)
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
                        // convert value to type
                        Type type = objectPropertiesPerName[propertyName].PropertyType;
                        object convertedValue = TypeObjectConverter.ConvertObjectRecord(currentSpValue.ToString(), type);
                        objectPropertiesPerName[propertyName].SetValue(result, convertedValue);
                    }
                }
            }

            return result;
        }
        #endregion

        #region Update logic
        /// <summary>
        /// Updates the list item from SharePoint for all properties changed
        /// </summary>
        /// <param name="clientContext">The client context.</param>
        /// <param name="itemSource">The item source.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="listItem">The list item.</param>
        /// <param name="readCurrentSPValues">if set to <c>true</c> read the current SP values.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>
        ///   <c>True</c> if really updated
        /// </returns>
        private bool UpdateSPListItem(
            ClientContext clientContext, T itemSource, int itemId, ListItem listItem, bool readCurrentSPValues, ILogger logger)
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
                logger.WriteError(ex, $"Error with item {itemId}");
                return false;
            }
        }

        private void ExecuteQueryWithCounterCondition(ClientContext clientContext)
        {
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
                // read source value
                object sourceValue = objectPropertiesPerName[propertyName].GetValue(itemSource);

                if (sourceValue != null)
                {   // convert value to a SharePoint friendly format!
                    if (sourceValue is DateTime || sourceValue is DateTime?)
                    {
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
                    listItem.Update();
                    if (!updated)
                        updated = true;
                }
            }

            return updated;
        }

        private static Field[] FilterCopiableFields(FieldCollection fields, FieldCollection fieldsSource)
        {
            var sourceInternNames = fieldsSource.Select(f => f.InternalName).ToArray();
            return fields.Where(field => !field.ReadOnlyField && !field.Hidden
                && field.InternalName != "Attachments" && field.InternalName != "ContentType"
                && sourceInternNames.Contains(field.InternalName)).ToArray();
        }
        #endregion
    }
}
