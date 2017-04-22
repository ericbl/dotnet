using Common.Logging;
using System;
using System.Collections.Generic;

namespace Common.Files
{
    /// <summary>
    /// Manage the CSV file serialization within a ZIP file.
    /// </summary>
    public class CSVFileInZip
    {
        private readonly string folderPathWithZipFile;
        private readonly string zipFileSuffix;
        private readonly string csvFileSuffix;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CSVFileInZip" /> class.
        /// </summary>
        /// <param name="folderPathWithZipFile">The folder path with the zip file.</param>
        /// <param name="zipFileSuffix">The file suffix of the zip file.</param>
        /// <param name="csvFileSuffix">The file suffix of the CSV file.</param>
        /// <param name="logger">The logger.</param>
        public CSVFileInZip(string folderPathWithZipFile, string zipFileSuffix, string csvFileSuffix, ILogger logger)
        {
            this.folderPathWithZipFile = folderPathWithZipFile;
            this.zipFileSuffix = zipFileSuffix;
            this.csvFileSuffix = csvFileSuffix;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the list from the CSV file inside the lastest zip file in the given folder.
        /// </summary>
        /// <typeparam name="T">Type of the object in the list</typeparam>
        /// <returns>The created list</returns>
        public IList<T> GetListFromCSVInsideZipFile<T>()
            where T : new()
        {
            // extract former zip file to an temp folder, get the file
            return IOUtils.GetCSVFileFromLatestZipInFolder(folderPathWithZipFile, zipFileSuffix, csvFileSuffix, logger,
                csvFilePath => CSVReader.ReadCsvFile<T>(csvFilePath));
        }

        /// <summary>
        /// Reads all ch users from Active Directory and serialize their properties to a list.
        /// </summary>
        /// <typeparam name="T">Type of the object in the list</typeparam>
        /// <param name="modelVersion">The model version.</param>
        /// <param name="listSerializable">The list to serialize.</param>
        /// <returns>A tuple with the zip full path and a flag whether the zip file has been created.</returns>
        public Tuple<string, bool> SerializeListToCSVAndZipFile<T>(DateTime modelVersion, IReadOnlyList<T> listSerializable)
            where T : new()
        {
            return IOUtils.CreateZipFromFilesGeneratedByAction(folderPathWithZipFile, zipFileSuffix, modelVersion, tmpFolderForCsv =>
            {
                string fileNameCsv = IOUtils.CreateCSVFileName(csvFileSuffix, modelVersion);
                // Run the action
                CsvSerializer.SerializeCollection(tmpFolderForCsv, fileNameCsv, listSerializable);
            });
        }
    }
}
