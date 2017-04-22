namespace Common.SharePoint
{
    using Files;
    using Logging;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using ClientOM = Microsoft.SharePoint.Client;

    /// <summary>
    /// Base class to write document or list into SharePoint Online
    /// </summary>
    /// <seealso cref="Common.SharePoint.SharePointConnector" />
    public class SPWriter : SharePointConnector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SPWriter" /> class.
        /// </summary>
        /// <param name="sharepointUri">The URI of the SharePoint site. Must be set!</param>
        /// <param name="credentialUserName">Name of the credential user.</param>
        /// <param name="sharePointFolder">The folder to upload files.</param>
        /// <param name="logger">The logger.</param>
        public SPWriter(System.Uri sharepointUri, string credentialUserName, string sharePointFolder, ILogger logger)
            : base(sharepointUri, credentialUserName, sharePointFolder, logger)
        {
        }

        /// <summary>
        /// Uploads all files from the given folder.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <returns><c>True</c> if successfully uploaded</returns>
        public bool UploadFiles(string folderPath)
        {
            var fileList = IOUtils.EnumerateFilesInDir(folderPath).Select(fi => new FilePathOrStream(fi.FullName)).ToList();
            return UploadFiles(fileList);
        }

        /// <summary>
        /// Uploads the files.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns><c>True</c> if successfully uploaded</returns>
        public bool UploadFiles(IList<FilePathOrStream> files)
        {
            return PrepareContextAndRunActionOnSP(context => UploadFiles(context, files));
        }

        /// <summary>
        /// Uploads the items to list.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the list</typeparam>
        /// <param name="listTitle">The list title.</param>
        /// <param name="listSource">The list source.</param>
        /// <param name="createListIfNotExists">if set to <c>true</c> create a list if it does not exist.</param>
        /// <returns>
        /// Number of items modified
        /// </returns>
        public int UploadItemsToList<T>(string listTitle, ListWithMetadata<T, int> listSource, bool createListIfNotExists)
            where T : class, new()
        {
            var spListMgr = new SPListManager<T>();
            return PrepareContextAndRunActionOnSP(context => spListMgr.UploadItemsToList(
                context, listTitle, listSource, createListIfNotExists, Logger));
        }

        /// <summary>
        /// Uploads the files.
        /// </summary>
        /// <param name="clientContext">The client context.</param>
        /// <param name="files">The files.</param>
        /// <returns><c>True</c> if successfully uploaded</returns>
        private bool UploadFiles(ClientOM.ClientContext clientContext, IList<FilePathOrStream> files)
        {
            foreach (FilePathOrStream file in files)
            {
                if (!string.IsNullOrEmpty(file.FilePath) && File.Exists(file.FilePath))
                {
                    using (FileStream fileStream = new FileStream(file.FilePath, FileMode.Open))
                    {
                        SaveStreamToSP(clientContext, file.FileNameWithExtension, fileStream);
                    }
                }
                else if (file.Stream != null)
                {
                    SaveStreamToSP(clientContext, file.FileNameWithExtension, file.Stream);
                }
            }

            return true;
        }

        private void SaveStreamToSP(ClientOM.ClientContext clientContext, string fileName, Stream stream)
        {
            ClientOM.File.SaveBinaryDirect(clientContext, SharepointFolder + Path.AltDirectorySeparatorChar + fileName, stream, true);
        }
    }
}
