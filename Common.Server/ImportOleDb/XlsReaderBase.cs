using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Common.Strings;

namespace Common.Server.ImportOleDb
{
    /// <summary>
    /// Helper class to read an excel file and its sheet as a generic list of objects
    /// </summary>
    public abstract class XlsReaderBase : IDisposable
    {
        /// <summary>
        /// The path to the file
        /// </summary>
        private readonly string filePath;

        /// <summary>
        /// Flag if the file was generated from a stream in this class. If yes, it will be removed in dispose
        /// </summary>
        private readonly bool fileGenerated;

        /// <summary>
        /// Format Provider for converting the data (CultureInfo)
        /// </summary>
        private readonly IFormatProvider formatProvider;

        /// <summary>
        /// List or warnings and errors
        /// </summary>
        private readonly StringBuilderWithUniqueMsg errorLogging;

        /// <summary>
        /// Initializes a new instance of the <see cref="XlsReaderBase"/> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="formatProvider">The format provider for the conversion of the data (CultureInfo).</param>
        protected XlsReaderBase(string filePath, IFormatProvider formatProvider)
            : this(formatProvider)
        {
            this.filePath = filePath;
            this.fileGenerated = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XlsReaderBase"/> class. Generates a temporary file into which the memory stream is stored
        /// </summary>
        /// <param name="stream">The memory stream containing the excel data.</param>
        /// <param name="formatProvider">The format provider for the conversion of the data (CultureInfo).</param>
        /// <param name="fileExtension">The file extension.</param>
        protected XlsReaderBase(MemoryStream stream, IFormatProvider formatProvider, string fileExtension)
            : this(formatProvider)
        {
            this.filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "tempExcelFile" + fileExtension);
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }

            // Create the file from the given stream
            using (var fs = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite))
            {
                stream.WriteTo(fs);
            }

            this.fileGenerated = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XlsReaderBase"/> class.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        private XlsReaderBase(IFormatProvider formatProvider)
        {
            this.errorLogging = new StringBuilderWithUniqueMsg();
            this.formatProvider = formatProvider;
        }

        /// <summary>
        /// Gets the data set.
        /// </summary>
        /// <value>The data set.</value>
        public DataSet DataSet { get; private set; }

        /// <summary>
        /// Gets the error logging.
        /// </summary>
        /// <value>The error logging.</value>
        public IEnumerable<string> ErrorLogging
        {
            get
            {
                return this.errorLogging != null ? this.errorLogging.AllMessages : null;
            }
        }

        /// <summary>
        /// Gets the path to the file.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        protected string FilePath
        {
            get
            {
                return filePath;
            }
        }

        /// <summary>
        /// Reads the file into a dataset, depending of the readerMethod
        /// </summary>
        public void ReadFile()
        {
            if (File.Exists(FilePath))
            {
                this.DataSet = ReadDataSet();
            }

            RemoveGeneratedFile();
        }

        /// <summary>
        /// Reads the data set.
        /// </summary>
        /// <returns>The Dataset</returns>
        public abstract DataSet ReadDataSet();

        /// <summary>
        /// Gets the sheet.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="sheetIndex">Index of the sheet.</param>
        /// <returns>The list of object</returns>
        public IList<T> ReadSheet<T>(int sheetIndex)
            where T : IDataTableToList
        {
            if (this.DataSet == null)
            {
                ReadFile();
            }

            return DataTableToList<T>.ReadSheet(this.DataSet, formatProvider, sheetIndex, errorLogging);
        }

        /// <summary>
        /// Reads the sheet given by its name
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <returns>List of objects created by the Sheet Loader</returns>
        public IList<T> ReadSheet<T>(string sheetName)
            where T : IDataTableToList
        {
            if (this.DataSet == null)
            {
                ReadFile();
            }

            return DataTableToList<T>.ReadSheet(this.DataSet, formatProvider, sheetName, errorLogging);
        }

        /// <summary>
        /// Removes the generated file.
        /// </summary>
        public void RemoveGeneratedFile()
        {
            try
            {
                if (fileGenerated && File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }
            }
            catch (System.IO.IOException)
            {
                // Ignore (Remedy against TFS Build Server error: The process cannot access the file because it is being used by another process.)
            }
        }

        #region IDisposable Member
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            RemoveGeneratedFile();
            if (DataSet != null)
            {
                DataSet.Clear();
            }
        }
        #endregion
    }
}