using System.Text;

namespace Common.Serialization
{
    /// <summary>
    /// The file parameter for serialization.
    /// </summary>
    public class SerializationFileParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationFileParameter"/> class, where the file shall be overwritten using the default encoding.
        /// </summary>
        /// <param name="folderFullPath">The full pathname of the folder.</param>
        /// <param name="fileName">The name of the file.</param>
        public SerializationFileParameter(string folderFullPath, string fileName)
            : this(folderFullPath, fileName, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationFileParameter"/> class using the default encoding
        /// </summary>
        /// <param name="folderFullPath">The full pathname of the folder.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="appendFile">True to append file, false to overwrite.</param>
        public SerializationFileParameter(string folderFullPath, string fileName, bool appendFile)
            : this(folderFullPath, fileName, appendFile, Encoding.Default)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationFileParameter"/> class.
        /// </summary>
        /// <param name="folderFullPath">The full pathname of the folder.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="appendFile">True to append file, false to overwrite.</param>
        /// <param name="fileEncoding">The file encoding.</param>
        public SerializationFileParameter(string folderFullPath, string fileName, bool appendFile, Encoding fileEncoding)
        {
            FolderFullPath = folderFullPath;
            FileName = fileName;
            AppendFile = appendFile;
            FileEncoding = fileEncoding;
        }

        /// <summary>
        /// Gets the full pathname of the folder.
        /// </summary>
        /// <value>
        /// The full pathname of the folder.
        /// </value>
        public string FolderFullPath { get; private set; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the file should be appended or overwritten.
        /// </summary>
        /// <value>
        /// True to append file, false to overwrite.
        /// </value>
        public bool AppendFile { get; private set; }

        /// <summary>
        /// Gets the file encoding.
        /// </summary>
        /// <value>
        /// The file encoding.
        /// </value>
        public Encoding FileEncoding { get; private set; }
    }
}
