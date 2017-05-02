using Common.Files;
using Common.Generic;
using Common.Logging;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using ClientOM = Microsoft.SharePoint.Client;

namespace Common.SharePoint
{
    /// <summary>
    /// Generic base class to read files from SharePoint
    /// </summary>
    /// <typeparam name="TModelContainer">Type of the model</typeparam>
    /// <seealso cref="Common.SharePoint.SPConnector" />
    public abstract class SPReader<TModelContainer>
        where TModelContainer : class
    {
        private readonly SPConnector spConnector;

        /// <summary>
        /// Initializes a new instance of the <see cref="SPReader{TModelContainer}"/> class.
        /// </summary>
        /// <param name="spParameter">The sp parameter.</param>
        protected SPReader(SPParameter spParameter)
        {
            spConnector = new SPConnector(spParameter);
        }

        /// <summary>
        /// Reads the input streams.
        /// </summary>
        /// <returns>The container is not null</returns>
        public bool ReadInputStreams()
        {
            return ReadInputStreamsFromSP() != null;
        }

        /// <summary>
        /// Reads the input streams.
        /// </summary>
        /// <returns>the container of the data read from SP</returns>
        public abstract TModelContainer ReadInputStreamsFromSP();

        /// <summary>
        /// Reads the input streams.
        /// </summary>
        /// <param name="convertMethod">The convert method.</param>
        /// <param name="fileExtension">The file extension without the point, e.g. 'xlsx'.</param>
        /// <returns>
        /// the container of the data read from SP.
        /// </returns>
        protected TModelContainer ReadInputStreamsFromSP(Func<List<FilePathOrStream>, TModelContainer> convertMethod, string fileExtension)
        {
            return spConnector.PrepareContextAndRunActionOnSP(clientContext => ReadAndConvertFilesAsStream(clientContext, convertMethod, fileExtension));
        }

        /// <summary>
        /// Reads files as stream, convert them to the output container and dispose those streams. Inspired from https://msdn.microsoft.com/en-
        /// us/library/ee956524(office.14).aspx#SP2010ClientOMOpenXml_Retrieving.
        /// </summary>
        /// <exception cref="ArgumentNullException">Any parameter not set.</exception>
        /// <param name="clientContext">The client context.</param>
        /// <param name="convertMethod">The convert method.</param>
        /// <param name="fileExtension">The file extension without the point, e.g. 'xlsx'.</param>
        /// <returns>
        /// The model container.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "design")]
        private TModelContainer ReadAndConvertFilesAsStream(ClientContext clientContext, Func<List<FilePathOrStream>, TModelContainer> convertMethod, string fileExtension)
        {
            if (clientContext == null)
                throw new ArgumentNullException(nameof(clientContext));

            if (convertMethod == null)
                throw new ArgumentNullException(nameof(convertMethod));

            ListItemCollection listItems = spConnector.GetExistingFilesInFolder(clientContext, fileExtension);
            TModelContainer result = null;

            using (var listOfStreams = new DisposableList<FilePathOrStream>())
            {
                foreach (ListItem item in listItems)
                {
                    string filename = item["FileLeafRef"].ToString();
                    string filePath = item["FileRef"].ToString();
                    DateTime writeDate = DateTime.Parse(item["Last_x0020_Modified"].ToString());
                    using (FileInformation fileInformation = ClientOM.File.OpenBinaryDirect(clientContext, filePath))
                    {
                        var memoryStream = new MemoryStream(); // must be disposed in a 2nd step!
                        IOUtils.CopyStream(fileInformation.Stream, memoryStream);
                        listOfStreams.Add(new FilePathOrStream(filename, memoryStream, writeDate));
                    }
                }

                // Convert streams
                result = convertMethod(listOfStreams);
            } // dispose streams!

            return result;
        }
    }
}
