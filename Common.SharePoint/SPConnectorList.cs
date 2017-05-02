using Common.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.SharePoint
{
    /// <summary>
    /// Link the <seealso cref="SPConnector"/> with the <seealso cref="SPListManager{T}"/>
    /// </summary>
    /// <typeparam name="T">Type of the object in the list.</typeparam>
    public class SPConnectorList<T>
         where T : class, ISPListItem, new()
    {
        private readonly SPConnector spConnector;
        private readonly SPListManager<T> spListMgr;
        private readonly string listTitle;
        private readonly bool createListIfNotExists;
        private readonly Queue<Action> listQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SPConnectorList{T}"/> class.
        /// </summary>
        /// <param name="spParameter">              The sp parameter.</param>
        /// <param name="logger">                   The logger.</param>
        /// <param name="listTitle">                The list title.</param>
        /// <param name="filterWithIgnoreAttribute">True will read the <seealso cref="Common.Reflection.IgnoreSerializationAttribute"/>
        /// and ignore the properties with that attribute.</param>
        /// <param name="createListIfNotExists">    (Optional) if set to <c>true</c> create the list if it does not exist.</param>
        public SPConnectorList(SPParameter spParameter, ILogger logger, string listTitle, bool filterWithIgnoreAttribute, bool createListIfNotExists = false)
        {
            spConnector = new SPConnector(spParameter);
            spListMgr = new SPListManager<T>(logger, filterWithIgnoreAttribute);
            this.listTitle = listTitle;
            this.createListIfNotExists = createListIfNotExists;
            listQueue = new Queue<Action>();
        }

        /// <summary>
        /// Read the SharePoint list and generate objects from the list items.
        /// </summary>
        /// <param name="queryFilter">A filter specifying the query.</param>
        /// <returns>
        /// List of created objects.
        /// </returns>
        public ListWithMetadata<T> ReadAndTransformSPList(Dictionary<string, string> queryFilter)
        {
            return spConnector.PrepareContextAndRunActionOnSP(context => spListMgr.TransformSPListToObjects(context, listTitle, queryFilter));
        }

        /// <summary>
        /// Moves the items from list source to list destination.
        /// </summary>
        /// <param name="listDestinationTitle">The list destination title.</param>
        public void EnqueueMoveItemsFromListSourceToListDestination(string listDestinationTitle)
        {
            listQueue.Enqueue(() => MoveItemsFromListSourceToListDestination(listDestinationTitle));
        }

        /// <summary>
        /// Updates the given property on list items.
        /// </summary>
        /// <param name="idsOflistItemToUpdate">The identifiers of list item to update.</param>
        /// <param name="propertyName">         Name of the property.</param>
        /// <param name="propertyValue">        The property value.</param>
        public void EnqueueUpdateOnePropertyOnListItems(IList<int> idsOflistItemToUpdate, string propertyName, string propertyValue)
        {
            listQueue.Enqueue(() => UpdateOnePropertyOnListItems(idsOflistItemToUpdate, propertyName, propertyValue));
        }

        /// <summary>
        /// Enqueue a list to upload.
        /// </summary>
        /// <param name="listSource">The list source.</param>
        public void EnqueueListToUpload(ListWithMetadata<T> listSource)
        {
            listQueue.Enqueue(() => UploadItemsToList(listSource));
        }

        /// <summary>
        /// Proceed queue.
        /// </summary>
        public void ProceedQueue()
        {
            while (listQueue.Count > 0)
            {
                var action = listQueue.Dequeue();
                action();
            }
        }

        /// <summary>
        /// Proceed queue.
        /// </summary>
        /// <returns>
        /// The asynchronous result that yields an int.
        /// </returns>
        public async Task ProceedQueueAsync()
        {
            while (listQueue.Count > 0)
            {
                Task t = new Task(listQueue.Dequeue());
                t.Start();
                await t;
            }
        }

        private int UploadItemsToList(ListWithMetadata<T> listSource)
        {
            return spConnector.PrepareContextAndRunActionOnSP(context => spListMgr.AddOrUploadItemsToList(
                context, listTitle, listSource, createListIfNotExists));
        }

        /// <summary>
        /// Moves the items from list source to list destination.
        /// </summary>
        /// <param name="listDestinationTitle">The list destination title.</param>
        /// <returns>
        /// Number of items copied
        /// </returns>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Required for the SPListManager")]
        private int MoveItemsFromListSourceToListDestination(string listDestinationTitle)
        {
            return spConnector.PrepareContextAndRunActionOnSP(context => spListMgr.MoveAllItemsBetweenList(context, listTitle, listDestinationTitle));
        }

        /// <summary>
        /// Updates the given property on list items.
        /// </summary>
        /// <param name="idsOflistItemToUpdate">The identifiers of list item to update.</param>
        /// <param name="propertyName">         Name of the property.</param>
        /// <param name="propertyValue">        The property value.</param>
        /// <returns>
        /// Number of items modified.
        /// </returns>
        private int UpdateOnePropertyOnListItems(IList<int> idsOflistItemToUpdate, string propertyName, string propertyValue)
        {
            return spConnector.PrepareContextAndRunActionOnSP(context
                => spListMgr.UpdateAPropertyOnList(context, listTitle, idsOflistItemToUpdate, propertyName, propertyValue));
        }
    }
}
