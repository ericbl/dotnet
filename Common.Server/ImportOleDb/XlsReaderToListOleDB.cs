namespace Common.Server.ImportOleDb
{
    using System;
    using System.Data;
    using System.IO;

    /// <summary>
    /// Helper class to read an excel file and its sheet as a generic list of objects
    /// </summary>
    public class XlsReaderToListOleDB : XlsReaderBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XlsReaderToListOleDB"/> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="formatProvider">The format provider.</param>
        public XlsReaderToListOleDB(string filePath, IFormatProvider formatProvider)
             : base(filePath, formatProvider)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XlsReaderToListOleDB"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="fileExtension">The file extension.</param>
        public XlsReaderToListOleDB(MemoryStream stream, IFormatProvider formatProvider, string fileExtension)
            : base(stream, formatProvider, fileExtension)
        {
        }

        /// <summary>
        /// Reads the file into a dataset.
        /// </summary>
        /// <returns>
        /// The Dataset
        /// </returns>
        public override DataSet ReadDataSet()
        {
            // read the file but does not let the dataset interpret wrong types!
            return XlsToDatasetOleDB.GetExcelDataSet(FilePath, false, false);
        }
    }
}