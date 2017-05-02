namespace Common.SharePoint
{
    using Files;
    using Logging;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using ClientOM = Microsoft.SharePoint.Client;

    /// <summary>
    /// Upload file or stream to SharePoint.
    /// </summary>
    public class SPWriter
    {
        private readonly SPConnector spConnector;

        /// <summary>
        /// Initializes a new instance of the <see cref="SPWriter" /> class.
        /// </summary>
        /// <param name="spParameter">The sp parameter.</param>
        public SPWriter(SPParameter spParameter)
        {
            spConnector = new SPConnector(spParameter);
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
            return spConnector.PrepareContextAndRunActionOnSP(context => UploadFiles(context, files));
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
            ClientOM.File.SaveBinaryDirect(clientContext, spConnector.SharepointFolder + Path.AltDirectorySeparatorChar + fileName, stream, true);
        }
    }
}
