using Common.Generic;
using Common.Helper;
using Common.Logging;
using Common.Strings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Common.Files
{
    /// <summary>
    /// Static utilities for i/o operation, i.e. dealing with the file system.
    /// </summary>
    public static class IOUtils
    {
        #region Path combine / temp folder
        /// <summary>
        /// Combine folder path and filename, throwing exception if directory or file not found.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="fileNameOrSubFolder">Name of the file or sub folder.</param>
        /// <param name="ensureFileOrSubFolderExists">if set to <c>true</c> throw exception if file does not exist.</param>
        /// <param name="isSubFolder">if set to <c>true</c> fileNameOrSubFolder is a folder.</param>
        /// <returns>The file path</returns>
        /// <exception cref="DirectoryNotFoundException">if folderPath not found</exception>
        /// <exception cref="FileNotFoundException">if fileName not found</exception>
        public static string CheckAndCombinePath(string folderPath, string fileNameOrSubFolder, bool ensureFileOrSubFolderExists, bool isSubFolder = false)
        {
            CheckDirectoryExists(folderPath);
            var filePath = Path.Combine(folderPath, fileNameOrSubFolder);
            if (ensureFileOrSubFolderExists)
            {
                if (isSubFolder)
                    CheckDirectoryExists(filePath);
                else
                    CheckFileExists(filePath);
            }
            return filePath;
        }

        /// <summary>
        /// Checks if the file exists and throw exception if not.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>
        /// The file path
        /// </returns>
        /// <exception cref="ArgumentNullException">file path not set</exception>
        /// <exception cref="FileNotFoundException">if the file does not exists</exception>
        public static string CheckFileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File does not exist: {filePath}");
            }
            return filePath;
        }

        /// <summary>
        /// Checks the directory exists and throw exception if not.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <returns>the folder path</returns>
        /// <exception cref="ArgumentNullException">folder path not set</exception>
        /// <exception cref="DirectoryNotFoundException">{nameof(folderPath)} {folderPath} n</exception>
        public static string CheckDirectoryExists(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                throw new ArgumentNullException(nameof(folderPath));
            }

            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"{nameof(folderPath)} {folderPath} not found!");
            }
            return folderPath;
        }

        /// <summary>
        /// Generates a temporary folder path with GUID.
        /// </summary>
        /// <returns>A sub folder in system temp path</returns>
        public static string GenerateTempFolderPath()
        {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Generates the temporary folder path and create directory.
        /// </summary>
        /// <returns>The directoryInfo of the new created directory</returns>
        public static DirectoryInfo GenerateTempFolderPathAndCreateDirectory()
        {
            string tmpGuid = GenerateTempFolderPath();
            return Directory.CreateDirectory(tmpGuid);
        }

        /// <summary>
        /// Generates the temporary folder path and create directory.
        /// </summary>
        /// <returns>The fullName of the new created directory</returns>
        public static string GenerateTempFolderPathAndCreateDirectoryStr()
        {
            return GenerateTempFolderPathAndCreateDirectory().FullName;
        }

        /// <summary>
        /// Extracts the file system folder from URI.
        /// </summary>
        /// <param name="fileSystemLetter">The file system letter.</param>
        /// <param name="uriPath">The URI.</param>
        /// <returns>the folder path</returns>
        public static string ExtractFileSystemFolderFromUri(string fileSystemLetter, string uriPath)
        {
            if (string.IsNullOrEmpty(fileSystemLetter) || string.IsNullOrEmpty(uriPath))
                return null;

            var result = Path.Combine(fileSystemLetter, uriPath.Split('/').Last());
            return CheckDirectoryExists(result);
        }
        #endregion

        #region Get files of type
        /// <summary>
        /// Enumerates the files in the directory.
        /// </summary>
        /// <param name="sourceDriverFolder">The source driver folder.</param>
        /// <returns>The files in the directory as <seealso cref="FileInfo"/></returns>
        /// <exception cref="ArgumentNullException">Source folder is null or empty</exception>
        /// <exception cref="DirectoryNotFoundException">Source directory does not exist or could not be found</exception>
        public static IEnumerable<FileInfo> EnumerateFilesInDir(string sourceDriverFolder)
        {
            if (string.IsNullOrEmpty(sourceDriverFolder))
            {
                throw new ArgumentNullException("driverFolder");
            }
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDriverFolder);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDriverFolder);
            }

            return dir.EnumerateFiles();
        }

        /// <summary>
        /// Filters the files in the directory.
        /// </summary>
        /// <param name="sourceDriverFolder">The source driver folder.</param>
        /// <param name="filterPredicate">The filter predicate.</param>
        /// <param name="throwExceptionIfNoInfFound">if set to <c>true</c> throws an FileNotFoundException if no file is found for the criteria.</param>
        /// <returns>The files in the directory as <seealso cref="FileInfo"/></returns>
        /// <exception cref="ArgumentNullException">Source folder is null or empty</exception>
        /// <exception cref="DirectoryNotFoundException">Source directory does not exist or could not be found</exception>
        /// <exception cref="FileNotFoundException">No file found in the source folder</exception>
        public static IEnumerable<FileInfo> FilterFilesInDir(string sourceDriverFolder, Func<FileInfo, bool> filterPredicate, bool throwExceptionIfNoInfFound)
        {
            var files = EnumerateFilesInDir(sourceDriverFolder).Where(filterPredicate);
            var count = files.Count();
            if (count == 0)
            {
                if (throwExceptionIfNoInfFound)
                    throw new FileNotFoundException($"No file found in the folder {sourceDriverFolder} for the given criteria");
                else
                    return null;
            }

            return files;
        }

        /// <summary>
        /// Get all files in the driver folder of the given extension.
        /// </summary>
        /// <param name="sourceDriverFolder">The driver folder.</param>
        /// <param name="searchedExtension">The searched extension.</param>
        /// <param name="throwExceptionIfNoInfFound">if set to <c>true</c> throws an FileNotFoundException if no file is found for the criteria.</param>
        /// <returns>
        /// Full file path of the inf file if one and only one is found. Null string if none.
        /// </returns>        
        /// <exception cref="ArgumentNullException">Source folder is null or empty</exception>
        /// <exception cref="DirectoryNotFoundException">Source directory does not exist or could not be found</exception>
        /// <exception cref="FileNotFoundException">No file found in the source folder</exception>
        public static IEnumerable<FileInfo> GetAllFilesOfType(string sourceDriverFolder, string searchedExtension, bool throwExceptionIfNoInfFound)
        {
            return FilterFilesInDir(sourceDriverFolder, f => f.Extension.ToLower() == searchedExtension.ToLower(), throwExceptionIfNoInfFound);
        }

        /// <summary>
        /// Finds all inf files in the driver folder.
        /// </summary>
        /// <param name="sourceDriverFolder">The driver folder.</param>
        /// <param name="throwExceptionIfNoInfFound">if set to <c>true</c> throws an FileNotFoundException if no inf file is found.</param>
        /// <returns>
        /// Full file path of the inf file if one and only one is found. Null string if none.
        /// </returns>
        /// <exception cref="ArgumentNullException">Source folder is null or empty</exception>
        /// <exception cref="DirectoryNotFoundException">Source directory does not exist or could not be found</exception>
        /// <exception cref="FileNotFoundException">No inf file found in the source folder</exception>
        public static IEnumerable<string> FindInfFiles(string sourceDriverFolder, bool throwExceptionIfNoInfFound)
        {
            return GetAllFilesOfType(sourceDriverFolder, ".inf", throwExceptionIfNoInfFound).Select(fi => fi.FullName);
        }


        /// <summary>
        /// Finds the unique inf file in the driver folder.
        /// </summary>
        /// <param name="sourceDriverFolder">The driver folder.</param>
        /// <param name="throwExceptionIfNoInfFound">if set to <c>true</c> throws an FileNotFoundException if no inf file is found.</param>
        /// <returns>
        /// Full file path of the inf file if one and only one is found. Null string if none.
        /// </returns>
        /// <exception cref="ArgumentException">More than one inf files found in the source folder</exception>
        /// <exception cref="ArgumentNullException">Source folder is null or empty</exception>
        /// <exception cref="DirectoryNotFoundException">Source directory does not exist or could not be found</exception>
        /// <exception cref="FileNotFoundException">No inf file found in the source folder</exception>
        public static string FindInfFile(string sourceDriverFolder, bool throwExceptionIfNoInfFound = true)
        {
            var fileFullNames = FindInfFiles(sourceDriverFolder, throwExceptionIfNoInfFound);
            if (fileFullNames == null)
            {
                return null;
            }
            var count = fileFullNames.Count();
            if (count > 1)
            {
                throw new ArgumentException($"{count} inf files found in the folder {sourceDriverFolder}");
            }

            return fileFullNames.First();
        }


        /// <summary>
        /// Gets the latest file in folder of the given extension.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="suffix">The suffix incl. the extension.</param>        
        /// <returns>
        /// Full path of the latest file of given extension found
        /// </returns>
        public static string GetLatestFileInFolder(string folderPath, string suffix)
        {
            IEnumerable<FileInfo> files = FilterFilesInDir(folderPath, f => f.Name.EndsWith(suffix), false);
            if (files != null && files.Count() > 0)
                return files.OrderBy(f => f.LastWriteTime).LastOrDefault().FullName;
            return null;
        }
        #endregion

        #region Copy/Del Folder
        /// <summary>
        /// Copies the drivers from the network path to the temp folder
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <returns>
        /// The full path of the driver folder
        /// </returns>
        public static string CopyFolderToLocalTemp(string folderPath)
        {
            return CopyFolderToTargetSubDrivers(folderPath, Path.GetTempPath());
        }

        /// <summary>
        /// Copies the given folder to the C:\Temp of the target machine.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="targetMachine">The target machine.</param>
        /// <param name="useAdminShare">if set to <c>true</c> add c$ to the path to use admin share (require admin rights). 
        /// Otherwise, the given path should be a network share with writing allowed.</param>
        /// <returns>
        /// The full path of the copied folder if successful, error message otherwise
        /// </returns>
        /// <remarks>
        /// If the folder already exists in the target, let it and return it (no fresh copy)
        /// </remarks>
        public static string CopyFolderToRemoteCTemp(string folderPath, string targetMachine, bool useAdminShare)
        {
            var target = "\\\\" + targetMachine;
            if (useAdminShare)
                target += "\\c$";
            target += "\\temp";
            return CopyFolderToTargetSubDrivers(folderPath, target);
        }

        /// <summary>
        /// Copies the given folder to the C:\Temp of the target machine.
        /// </summary>
        /// <param name="folderPath">The folder path (can be an UNC path).</param>
        /// <param name="targetTempFolder">The target temporary folder (UNC path).</param>
        /// <returns>The full path of the copied folder if successful, error message otherwise</returns>
        /// <remarks>If the folder already exists in the target, let it and return it (no fresh copy)</remarks>
        /// <exception cref="Exception">Error copying from {folderPath} to {targetTempFolder}",</exception>
        private static string CopyFolderToTargetSubDrivers(string folderPath, string targetTempFolder)
        {
            string result = string.Empty;
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    string error = folderPath + " was not found!";
                    Trace.TraceError(error);
                    throw new DirectoryNotFoundException(error);
                }
                else
                {
                    string copiedDir = Path.Combine(targetTempFolder, "Drivers", new DirectoryInfo(folderPath).Name);
                    DirectoryCopy(folderPath, copiedDir, true, true, false);
                    if (!Directory.Exists(copiedDir))
                    {
                        copiedDir += " was not found!";
                        Trace.TraceWarning(copiedDir);
                    }
                    return copiedDir;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error copying from {folderPath} to {targetTempFolder}", ex);
            }
        }

        /// <summary>
        /// Connects the given computer over the network.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException">Thrown when the requested directory is not present.</exception>
        /// <param name="uri">The URI.</param>
        /// <returns>
        /// True if it succeeds, false if it fails.
        /// </returns>
        public static bool ConnectNetworkComputer(string uri)
        {
            if (!Network.Utils.PingHost(uri))
            {
                return false;
            }

            DirectoryInfo dir = new DirectoryInfo(uri);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + uri);
            }

            return true;
        }

        /// <summary>
        /// Copy files and sub directories from the source directory to the target path.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException">Thrown when the requested directory is not present.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">Source directory does not exist or could not be found.</exception>
        /// <param name="sourceDirName">Name of the source dir.</param>
        /// <param name="destDirName">Name of the destination directory.</param>
        /// <param name="copySubDirs">if set to <c>true</c> copy sub directories.</param>
        /// <param name="quitOnExistingDestDir">if set to <c>true</c> ignore existing  destination directory and exit doing nothing.</param>
        /// <param name="overwriteExistingFiles">if set to <c>true</c> overwrite existing files.</param>
        /// <param name="fileFilter">(Optional) The file filter.</param>
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool quitOnExistingDestDir, bool overwriteExistingFiles, string fileFilter = "*")
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
            else if (quitOnExistingDestDir)
            {   // nothing to do if already present!
                return;
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles(fileFilter);
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, overwriteExistingFiles);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                DirectoryInfo[] dirs = dir.GetDirectories();
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs, quitOnExistingDestDir, overwriteExistingFiles);
                }
            }
        }
        #endregion

        #region Delete folder or files
        /// <summary>
        /// Generates the search pattern. *filename*extension
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="extension">The extension.</param>
        /// <returns>The search pattern</returns>
        public static string GenerateSearchPattern(string filename, string extension)
        {
            return $"*{filename}*{extension}";
        }

        /// <summary>
        /// Deletes the folder and all its content
        /// </summary>
        /// <param name="folder">The folder path.</param>
        public static void DeleteFolderOnly(string folder)
        {
            Directory.Delete(folder, true);  // DirectoryInfo.Delete calls Directory.Delete!
        }

        /// <summary>
        /// Deletes the folder, all its content, and the parent folder if it gets empty
        /// </summary>
        /// <param name="folderPathUNC">The folder path unc.</param>
        /// <exception cref="Exception">Error deleting {folderPathUNC}</exception>
        public static void DeleteFolder(string folderPathUNC)
        {
            try
            {
                if (!Directory.Exists(folderPathUNC))
                {
                    Trace.TraceWarning(folderPathUNC + " was not found!");
                }
                else
                {
                    DirectoryInfo di = new DirectoryInfo(folderPathUNC);
                    var parent = di.Parent;
                    di.Delete(true);
                    if (parent.EnumerateFileSystemInfos().Count() == 0)
                        parent.Delete(); // remove parent only if empty
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting {folderPathUNC}", ex);
            }
        }

        /// <summary>
        /// Delete all files matching the pattern from the current folder only.
        /// </summary>
        /// <param name="folderPathUNC">The folder path.</param>
        /// <param name="fileFilter">The file filter.</param>
        /// <exception cref="Exception">Error deleting {folderPathUNC}</exception>
        public static void EmptyFolder(string folderPathUNC, string fileFilter)
        {
            try
            {
                if (Directory.Exists(folderPathUNC))
                {
                    Directory.EnumerateFiles(folderPathUNC, fileFilter, SearchOption.TopDirectoryOnly).ToList().DeleteItemsInList(File.Delete);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting {folderPathUNC}", ex);
            }
        }

        private static void RemoveOlderFiles(string folderPath, string filename, string extension, DateTime currentDate,
           bool removeAll, bool shortDate = true)
        {
            // Get all files (collection of the full names including paths!)
            string csvFilePattern = GenerateSearchPattern(filename, extension);
            var previousFiles = Directory.EnumerateFiles(folderPath, csvFilePattern, SearchOption.TopDirectoryOnly);
            if (previousFiles != null)
            {
                string[] fileToRemove;
                if (removeAll)
                {
                    fileToRemove = previousFiles.ToArray();
                }
                else
                {
                    string dateStr = GetDateTimeString(currentDate, shortDate);
                    fileToRemove = previousFiles.Where(fp => !fp.Contains(dateStr)).ToArray();
                }
                fileToRemove.DeleteItemsInList(File.Delete);
            }
        }
        #endregion

        #region Read file

        /// <summary>
        /// Reads text file lines.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="fileFullPath">         Full pathname of the file full file.</param>
        /// <param name="throwExceptionOnError">True to throw exception error.</param>
        /// <returns>
        /// An array of string.
        /// </returns>
        public static string[] ReadTxtFileLines(string fileFullPath, bool throwExceptionOnError)
        {
            if (!File.Exists(fileFullPath))
            {
                return null;
            }

            //init a new string array to store the file lines
            string[] lines = null;
            try
            {
                //load lines from file, on error throws an exception
                lines = File.ReadAllLines(fileFullPath, Encoding.Default);
            }
            catch (Exception)
            {
                if (throwExceptionOnError)
                    throw;
                // ignore Errors otherwise
            }

            return lines;
        }

        #endregion

        #region HashCode

        /// <summary>
        /// Gets the SHA256 hash from the file defined by its path
        /// </summary>
        /// <param name="fileFullPath">Full pathname of the file full file.</param>
        /// <returns>
        /// The SHA256 hash.
        /// </returns>
        public static string GetSha256(string fileFullPath)
        {
            if (!File.Exists(fileFullPath))
            {
                return null;
            }
            string hash;
            using (Stream st = File.OpenRead(fileFullPath))
            {
                hash = BitConverter.ToString(SHA256.Create().ComputeHash(st)).Replace("-", string.Empty);
            }
            return hash;
        }


        #endregion

        #region Serialize list
        /// <summary>
        /// Read the list t the list append.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="targetFolderPath">The target folder path.</param>
        /// <param name="list">The list.</param>
        /// <param name="fileName">Name of the file.</param>
        public static void SerializeListAppend<T>(string targetFolderPath, IEnumerable<T> list, string fileName = null)
         where T : new()
        {
            if (list != null && list.Count() > 0)
            {
                string csvFileName = GenerateFilenameFromType<T>(fileName, ".csv");
                var itemToAppend = CSVReader.FilterCollectionToSerializeByRemovingExistingItems(targetFolderPath, csvFileName, list);
                if (itemToAppend != null && itemToAppend.Count() > 0)
                    CsvSerializer.SerializeCollection(targetFolderPath, csvFileName, itemToAppend, true, true);
            }
        }

        /// <summary>
        /// Serializes the list with date.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="targetFolderPath">The target folder path.</param>
        /// <param name="list">The list.</param>
        /// <param name="modelVersion">The model version.</param>
        /// <param name="fileName">Name of the file.</param>
        public static void SerializeListWithDate<T>(string targetFolderPath, IEnumerable<T> list, DateTime modelVersion, string fileName = null)
        {
            if (list != null && list.Count() > 0)
            {
                fileName = GenerateFilenameFromType<T>(fileName);
                string fileNameWithExt = CreateCSVFileName(fileName, modelVersion);
                CsvSerializer.SerializeCollection(targetFolderPath, fileNameWithExt, list);
            }
        }
        #endregion

        #region Zip file
        private const string zipExtension = ".zip";

        /// <summary>
        /// Extracts the zip file to a temporary directory.
        /// </summary>
        /// <param name="zipFilePath">The zip file path.</param>
        /// <returns>Path of the temporary directory.</returns>
        public static DirectoryInfo ExtractZipFileToTmpDirectory(string zipFilePath)
        {
            DirectoryInfo di = GenerateTempFolderPathAndCreateDirectory();
            var tempFilePath = di.FullName;

            ZipFile.ExtractToDirectory(zipFilePath, tempFilePath);

            return di;
        }

        /// <summary>
        /// Apply the action for the CSV file inside the lastest zip file in the given folder.
        /// </summary>
        /// <typeparam name="T">Type of the object in the collection</typeparam>
        /// <param name="folderPathWithZipFile">The folder path with the zip file.</param>
        /// <param name="zipFileSuffix">The file suffix of the zip file.</param>
        /// <param name="csvFileSuffix">The file suffix of the csv file.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="actionOnFile">The action to run on the file.</param>
        /// <returns>
        /// return value of the action
        /// </returns>
        /// <exception cref="System.ArgumentNullException">No action method defined in GetCSVFileFromLatestZipInFolder</exception>
        public static T GetCSVFileFromLatestZipInFolder<T>(string folderPathWithZipFile, string zipFileSuffix, string csvFileSuffix,
            ILogger logger, Func<string, T> actionOnFile)
        {
            if (actionOnFile == null)
                throw new ArgumentNullException("No action method defined in GetCSVFileFromLatestZipInFolder");

            CheckDirectoryExists(folderPathWithZipFile);
            string zipFilePath = GetLatestFileInFolder(folderPathWithZipFile, zipFileSuffix + zipExtension);
            if (string.IsNullOrEmpty(zipFilePath))
            {
                logger.WriteError($"no file name ending with {zipFileSuffix}{zipExtension} found in {folderPathWithZipFile}!");
                return default(T);
            }

            CheckFileExists(zipFilePath);
            DirectoryInfo di = ExtractZipFileToTmpDirectory(zipFilePath);
            string csvFilePattern = GenerateSearchPattern(csvFileSuffix, ".csv");
            var csvFiles = di.GetFiles(csvFilePattern, SearchOption.TopDirectoryOnly).ToArray();
            if (csvFiles == null || csvFiles.Length == 0)
            {
                logger.WriteError($"No file name matching {csvFilePattern} found in {folderPathWithZipFile}!");
                return default(T);
            }
            // Get the first file matching the pattern!
            var csvFilePath = csvFiles[0].FullName;
            if (csvFiles.Length > 1)
            {
                logger.WriteInfo($"{csvFiles.Length} filenames matching {csvFilePattern}, only the first {csvFilePath} will be considered!");
            }
            // Read the CSV file
            T result = actionOnFile(csvFilePath);
            // delete temp files
            Directory.Delete(di.FullName, true);

            return result;
        }

        /// <summary>
        /// Creates a zip archive from the files in the source path WITHOUT sub folder! Copy file to a temp folder and zip it to avoid sub folders in the zip!
        /// </summary>
        /// <param name="sourceFolderPath">The source folder path.</param>
        /// <param name="sourceFilesFilter">The source file filter.</param>
        /// <param name="targetZipFilePath">The target zip file path.</param>
        public static void CreateZipArchiveFromFiles(string sourceFolderPath, string sourceFilesFilter, string targetZipFilePath)
        {
            CreateZipFromFilesGeneratedByAction(targetZipFilePath, tmpFolderForAction
                => DirectoryCopy(sourceFolderPath, tmpFolderForAction, false, false, false, sourceFilesFilter));
        }

        /// <summary>
        /// Create a temp folder, run the action, copy files to target, zip the temp folder to the target path, delete the temp folder.
        /// </summary>
        /// <param name="targetFolderForGeneratedFiles">The target folder for the files generated by the action.</param>
        /// <param name="generatedFilePattern">The generated file pattern. Files will be deleted and re created</param>
        /// <param name="subArchiveFolderForZip">Name of the sub archive folder for the zip file.</param>
        /// <param name="zipFileSuffix">The zip file suffix.</param>
        /// <param name="modelVersion">The model version.</param>
        /// <param name="createFileAction">The action to create the files in the temp folder.</param>
        /// <param name="keepAndRenameExistingFileIfDifferent">if set to <c>true</c> and the zip is different, keep that file and rename it or make a copy.</param>
        /// <param name="copyInsteadOfRename">if set to <c>true</c> copy file and remove instead of move. Move/Rename is not allowed on SharePoint!.</param>
        /// <param name="copyToTargetAndRemoveOlderFiles">if set to <c>true</c> copy to target and remove older files.</param>
        /// <returns>
        /// A tuple with the zip full path and a flag whether the zip file has been created.
        /// </returns>
        /// <exception cref="ArgumentNullException">No action method defined in CreateCSVFileAndZipIt</exception>
        public static Tuple<string, bool> FilesGeneratedByActionToTargetAndCreateZipArchive(string targetFolderForGeneratedFiles, string generatedFilePattern,
            string subArchiveFolderForZip, string zipFileSuffix, DateTime modelVersion, Action<string> createFileAction,
            bool keepAndRenameExistingFileIfDifferent = false, bool copyInsteadOfRename = false, bool copyToTargetAndRemoveOlderFiles = false)
        {
            // Run the action to generate the files in the temp folder
            string tmpFolderForAction = GenerateFilesOnTempFolder(createFileAction);

            return CreateZipFromFilesInTmpFolderAndCopyFilesToTargetFolder(tmpFolderForAction, targetFolderForGeneratedFiles, generatedFilePattern,
                subArchiveFolderForZip, zipFileSuffix, modelVersion, keepAndRenameExistingFileIfDifferent, copyInsteadOfRename, copyToTargetAndRemoveOlderFiles);
        }

        /// <summary>
        /// From the files created in a temp folder, copy them to target, zip the temp folder to the target path, delete the temp folder.
        /// </summary>
        /// <param name="tmpFolderForAction">The temporary folder for action.</param>
        /// <param name="targetFolderForGeneratedFiles">The target folder for the files generated by the action.</param>
        /// <param name="generatedFilePattern">The generated file pattern. Files will be deleted and re created</param>
        /// <param name="subArchiveFolderForZip">Name of the sub archive folder for the zip file.</param>
        /// <param name="zipFileSuffix">The zip file suffix.</param>
        /// <param name="modelVersion">The model version.</param>
        /// <param name="keepAndRenameExistingFileIfDifferent">if set to <c>true</c> and the zip is different, keep that file and rename it or make a copy.</param>
        /// <param name="copyInsteadOfRename">if set to <c>true</c> copy file and remove instead of move. Move/Rename is not allowed on SharePoint!.</param>
        /// <param name="copyToTarget">if set to <c>true</c> copy files to target (default). If <c>false</c> same behavior as <seealso cref="CreateZipFromFilesInTmpFolder"/></param>
        /// <returns>
        /// A tuple with the zip full path and a flag whether the zip file has been created.
        /// </returns>
        /// <exception cref="ArgumentNullException">No action method defined in CreateCSVFileAndZipIt</exception>
        public static Tuple<string, bool> CreateZipFromFilesInTmpFolderAndCopyFilesToTargetFolder(string tmpFolderForAction,
            string targetFolderForGeneratedFiles, string generatedFilePattern, string subArchiveFolderForZip, string zipFileSuffix, DateTime modelVersion,
            bool keepAndRenameExistingFileIfDifferent = false, bool copyInsteadOfRename = false, bool copyToTarget = true)
        {
            CheckDirectoryExists(tmpFolderForAction);
            CheckDirectoryExists(targetFolderForGeneratedFiles);

            if (copyToTarget)
            {
                // Copy the files to the target folder             
                EmptyFolder(targetFolderForGeneratedFiles, generatedFilePattern); // remove all former files to avoid remanent files not relevant anymore
                DirectoryCopy(tmpFolderForAction, targetFolderForGeneratedFiles, false, false, true);
            }
            return CreateZipFromFilesInTmpFolder(tmpFolderForAction,
                targetFolderForGeneratedFiles, subArchiveFolderForZip, zipFileSuffix, modelVersion,
                keepAndRenameExistingFileIfDifferent, copyInsteadOfRename);
        }

        /// <summary>
        /// From the files created in a temp folder, zip the temp folder to the target path, delete the temp folder.
        /// </summary>
        /// <param name="tmpFolderForAction">The temporary folder for action.</param>
        /// <param name="targetFolderForGeneratedFiles">The target folder for the files generated by the action.</param>
        /// <param name="subArchiveFolderForZip">Name of the sub archive folder for the zip file.</param>
        /// <param name="zipFileSuffix">The zip file suffix.</param>
        /// <param name="modelVersion">The model version.</param>
        /// <param name="keepAndRenameExistingFileIfDifferent">if set to <c>true</c> and the zip is different, keep that file and rename it or make a copy.</param>
        /// <param name="copyInsteadOfRename">if set to <c>true</c> copy file and remove instead of move. Move/Rename is not allowed on SharePoint!.</param>
        /// <returns>
        /// A tuple with the zip full path and a flag whether the zip file has been created.
        /// </returns>
        /// <exception cref="ArgumentNullException">No action method defined in CreateCSVFileAndZipIt</exception>
        public static Tuple<string, bool> CreateZipFromFilesInTmpFolder(string tmpFolderForAction,
           string targetFolderForGeneratedFiles, string subArchiveFolderForZip, string zipFileSuffix, DateTime modelVersion,
           bool keepAndRenameExistingFileIfDifferent = false, bool copyInsteadOfRename = false)
        {
            string zipFolderPath = CheckAndCombinePath(targetFolderForGeneratedFiles, subArchiveFolderForZip, true, true);
            string zipFilePath = CreateFilePathZip(zipFolderPath, modelVersion, true, zipFileSuffix); // might already exists!        
            // Create the zip file
            bool newlyCreated = CreateZipFromFilesInFolder(tmpFolderForAction, zipFilePath, keepAndRenameExistingFileIfDifferent, copyInsteadOfRename);

            return new Tuple<string, bool>(zipFilePath, newlyCreated);
        }

        /// <summary>
        /// Create a temp folder, run the action, zip the folder to the target path, delete the temp folder.
        /// </summary>
        /// <param name="folderPathForZipFile">The path of the target folder for the zip file.</param>
        /// <param name="zipFileSuffix">The zip file suffix.</param>
        /// <param name="modelVersion">The model version.</param>
        /// <param name="createFileAction">The action to create the files in the temp folder.</param>
        /// <returns>A tuple with the zip full path and a flag whether the zip file has been created.</returns>
        /// <exception cref="ArgumentNullException">No action method defined in CreateCSVFileAndZipIt</exception>
        public static Tuple<string, bool> CreateZipFromFilesGeneratedByAction(string folderPathForZipFile, string zipFileSuffix,
            DateTime modelVersion, Action<string> createFileAction)
        {
            string zipFilePath = CreateFilePathZip(folderPathForZipFile, modelVersion, true, zipFileSuffix); // might already exists!            
            bool newlyCreated = CreateZipFromFilesGeneratedByAction(zipFilePath, createFileAction);
            return new Tuple<string, bool>(zipFilePath, newlyCreated);
        }

        /// <summary>
        /// Create a temp folder, run the action, zip the folder to the target path, delete the temp folder.
        /// </summary>
        /// <param name="zipFileFullPath">The full target path for the resulting zip file.</param>
        /// <param name="createFileAction">The action to create the files in the temp folder.</param>
        /// <param name="keepAndRenameExistingFileIfDifferent">if set to <c>true</c> and the zip is different, keep that file and rename it or make a copy.</param>
        /// <param name="copyInsteadOfRename">if set to <c>true</c> copy file and remove instead of move. Move/Rename is not allowed on SharePoint!.</param>
        /// <returns>
        /// Flag whether the zip file has been created.
        /// </returns>
        /// <exception cref="ArgumentNullException">No action method defined in CreateCSVFileAndZipIt</exception>
        public static bool CreateZipFromFilesGeneratedByAction(string zipFileFullPath, Action<string> createFileAction,
            bool keepAndRenameExistingFileIfDifferent = false, bool copyInsteadOfRename = false)
        {
            string tmpFolderForAction = GenerateFilesOnTempFolder(createFileAction);
            return CreateZipFromFilesInFolder(tmpFolderForAction, zipFileFullPath, keepAndRenameExistingFileIfDifferent, copyInsteadOfRename);
        }

        /// <summary>
        /// Creates a zip file from all files in the folder.
        /// </summary>
        /// <param name="folderWithFiles">The folder with files.</param>
        /// <param name="zipFileFullPath">The full target path for the resulting zip file.</param>
        /// <param name="keepAndRenameExistingFileIfDifferent">(Optional) if set to <c>true</c> and the zip is different, keep that file and
        /// rename it or make a copy. Otherwise, ignore zip creation (by full compare of files)</param>
        /// <param name="copyInsteadOfRename">(Optional) if set to <c>true</c> copy file and remove instead of move.
        /// Move/Rename is not allowed on SharePoint!.</param>
        /// <returns>
        /// True if it succeeds, false if it fails.
        /// </returns>
        public static bool CreateZipFromFilesInFolder(string folderWithFiles, string zipFileFullPath,
            bool keepAndRenameExistingFileIfDifferent = false, bool copyInsteadOfRename = false)
        {
            // Zip the folder to the given target folder: create only if not already exists with same content
            bool zipCreated = CreateZipFileOnlyIfNewContent(folderWithFiles, zipFileFullPath, keepAndRenameExistingFileIfDifferent, copyInsteadOfRename);
            // remove the temporary directory
            DeleteFolderOnly(folderWithFiles);
            // return flag if zip created
            return zipCreated;
        }

        /// <summary>
        /// Creates the zip file from the source directory only if has new content:
        /// if the zip file already exists, unzip it and compare to the files in the source folder.
        /// if same content, do nothing (return false)
        /// if different, delete former zip file and create a new one.
        /// </summary>
        /// <param name="sourceFolderPath">The source folder path.</param>
        /// <param name="zipFilePath">The zip file path.</param>
        /// <param name="keepAndRenameExistingFileIfDifferent">if set to <c>true</c> and the zip is different, keep that file and rename it or make a copy.</param>
        /// <param name="copyInsteadOfRename">if set to <c>true</c> copy file and remove instead of move. Move/Rename is not allowed on SharePoint!.</param>
        /// <returns>
        /// Flag whether the zip file has been created.
        /// </returns>
        public static bool CreateZipFileOnlyIfNewContent(string sourceFolderPath, string zipFilePath,
            bool keepAndRenameExistingFileIfDifferent, bool copyInsteadOfRename)
        {
            bool folderEquals = CompareExistingZipFileWithSourceFolder(sourceFolderPath, zipFilePath);
            if (!folderEquals)
            {
                if (keepAndRenameExistingFileIfDifferent)
                    CopyOrRenameExistingFile(zipFilePath, null, copyInsteadOfRename);
                else
                    File.Delete(zipFilePath);
                // Finally safe to create the zip file
                ZipFile.CreateFromDirectory(sourceFolderPath, zipFilePath);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the given zip file path exists, unzip the file and compare the content with the source folder.        
        /// </summary>
        /// <param name="sourceFolderPath">The source folder path.</param>
        /// <param name="zipFilePath">The zip file path.</param>        
        /// <returns><c>True</c> if the zipFile already exists and has the same content</returns>
        public static bool CompareExistingZipFileWithSourceFolder(string sourceFolderPath, string zipFilePath)
        {
            bool folderEquals = false;
            if (File.Exists(zipFilePath))
            {
                // extract former zip file to an temp folder                                
                var tempFilePath = ExtractZipFileToTmpDirectory(zipFilePath).FullName;
                // compare folders
                folderEquals = Compare.DirectoryEquals(sourceFolderPath, tempFilePath);
                // delete temp files
                DeleteFolderOnly(tempFilePath);
            }

            return folderEquals;
        }
        #endregion

        #region File name management
        /// <summary>
        /// Renames the existing file by keeping the write date of the source
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="newFileNameSuffix">The new file name suffix added to the previous filename. If null, current date will be considered</param>
        /// <param name="copyInsteadOfRename">if set to <c>true</c> copy file and remove instead of move. Move/Rename is not allowed on SharePoint!.</param>
        /// <returns>The file path of the new file</returns>
        public static string CopyOrRenameExistingFile(string filePath, string newFileNameSuffix, bool copyInsteadOfRename = false)
        {
            string newPath = null;
            if (File.Exists(filePath))
            {
                FileInfo fi = new FileInfo(filePath);
                DateTime srcWriteDate = fi.LastWriteTime;
                string filenameWoExt = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                if (string.IsNullOrEmpty(newFileNameSuffix))
                {
                    newFileNameSuffix = GetDateTimeString(srcWriteDate, false);
                }
                newPath = CheckAndCombinePath(fi.DirectoryName, filenameWoExt + "_" + newFileNameSuffix + fi.Extension, false);
                if (File.Exists(newPath))
                    File.Delete(newPath);
                if (copyInsteadOfRename)
                {
                    var newFi = fi.CopyTo(newPath);
                    newFi.LastWriteTime = srcWriteDate; // keep the date of the source
                    fi.Delete();
                }
                else
                {
                    fi.MoveTo(newPath);
                    fi.LastWriteTime = srcWriteDate;
                }
            }
            return newPath;
        }

        /// <summary>
        /// Creates the file path and remove older files.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="date">The date.</param>
        /// <param name="extension">The extension.</param>
        /// <param name="removeAll">if set to <c>true</c> remove all.</param>
        /// <returns>The file path</returns>
        public static string CreateFilePathAndRemoveOlderFiles(string folderPath, string filename, DateTime date, string extension, bool removeAll)
        {
            RemoveOlderFiles(folderPath, filename, extension, date, removeAll);
            return CreateFilePath(folderPath, date, true, filename, extension);
        }

        /// <summary>
        /// Creates the full file path, checking the folder exists.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="date">The date.</param>
        /// <param name="shortDate">if set to <c>true</c> consider only the date for the time, otherwise the full date+time.</param>
        /// <returns>The full file path</returns>
        public static string CreateFilePathZip(string folderPath, DateTime date, bool shortDate, string filename)
        {
            return CreateFilePath(folderPath, date, shortDate, filename, zipExtension);
        }

        /// <summary>
        /// Creates the full file path, checking the folder exists.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="date">The date.</param>
        /// <param name="shortDate">if set to <c>true</c> consider only the date for the time, otherwise the full date+time.</param>
        /// <param name="extension">The extension.</param>
        /// <returns>The full file path</returns>
        public static string CreateFilePath(string folderPath, DateTime date, bool shortDate, string filename, string extension)
        {
            string fileName = CreateFileName(date, shortDate, filename, extension);
            return CheckAndCombinePath(folderPath, fileName, false);
        }

        /// <summary>
        /// Creates the name of the CSV file.
        /// </summary>
        /// <param name="fileSuffix">The filename without path.</param>
        /// <param name="date">The date.</param>
        /// <returns>The file name of the CSV file</returns>
        public static string CreateCSVFileName(string fileSuffix, DateTime date)
        {
            return CreateFileName(date, true, fileSuffix, ".csv");
        }

        /// <summary>
        /// Creates the file name, without folder/path check.
        /// </summary>>
        /// <param name="date">The date.</param>
        /// <param name="shortDate">if set to <c>true</c> consider only the date for the time, otherwise the full date+time.</param>
        /// <param name="filename">The filename without path.</param>
        /// <param name="extension">The extension.</param>
        /// <returns>The file name</returns>
        public static string CreateFileName(DateTime date, bool shortDate, string filename, string extension)
        {
            return GetDateTimeString(date, shortDate) + "_" + filename + extension;
        }

        private static string GetDateTimeString(DateTime date, bool shortDate)
        {
            return shortDate ? date.ToShortDateStringFixedYearFirst() : date.ToFileNameString();
        }

        /// <summary>
        /// Generates the filename of the type and add an optional extension: typeof(T).Name + extension
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="filename">The filename.</param>
        /// <param name="extension">The extension.</param>
        /// <returns>filename</returns>
        public static string GenerateFilenameFromType<T>(string filename = null, string extension = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                filename = typeof(T).Name;
                if (!string.IsNullOrEmpty(extension))
                {
                    filename += extension;
                }
            }

            return filename;
        }
        #endregion

        #region File Date
        /// <summary>
        /// Gets the date from the file, considering the last write time.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The LastWriteTime date of the file</returns>
        /// <exception cref="DirectoryNotFoundException">if folderPath not found</exception>
        /// <exception cref="FileNotFoundException">if fileName not found</exception>
        public static DateTime GetFileDate(string folderPath, string fileName)
        {
            return GetFileDate(CheckAndCombinePath(folderPath, fileName, true));
        }

        /// <summary>
        /// Gets the date from the file, considering the last write time.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>The LastWriteTime date of the file</returns>
        public static DateTime GetFileDate(string filePath)
        {
            return File.GetLastWriteTime(filePath);
        }
        #endregion

        #region Copy Stream
        /// <summary>
        /// Copies the stream from source to destination with a buffer of 32k
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        public static void CopyStream(Stream source, Stream destination)
        {
            byte[] buffer = new byte[32768];
            int bytesRead;
            do
            {
                bytesRead = source.Read(buffer, 0, buffer.Length);
                destination.Write(buffer, 0, bytesRead);
            }
            while (bytesRead != 0);
        }
        #endregion

        /// <summary>
        /// Generates the files on temporary folder.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the required action is null.</exception>
        /// <param name="createFileAction">The action to create the files in the temp folder.</param>
        /// <returns>
        /// The files on temporary folder.
        /// </returns>
        public static string GenerateFilesOnTempFolder(Action<string> createFileAction)
        {
            if (createFileAction == null)
                throw new ArgumentNullException("No action method defined in GenerateFilesOnTempFolder");
            // Create a temporary directory
            string tmpFolderForAction = GenerateTempFolderPathAndCreateDirectoryStr();
            // Run the action generating files in the temp folder
            createFileAction(tmpFolderForAction);
            return tmpFolderForAction;
        }
    }
}
