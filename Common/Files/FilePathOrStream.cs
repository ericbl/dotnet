using System;
using System.IO;

namespace Common.Files
{
    /// <summary>
    /// Manage a file path or a stream for I/O operations
    /// </summary>
    /// <seealso cref="T:System.IDisposable"/>
    public class FilePathOrStream : IDisposable
    {
        private readonly string filePath;
        private readonly string filenameNoExtension;
        private readonly string filenameWithExtension;
        private readonly Stream stream;
        private readonly DateTime fileLastWriteDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePathOrStream"/> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public FilePathOrStream(string filePath)
        {
            this.filePath = filePath;
            filenameNoExtension = Path.GetFileNameWithoutExtension(filePath);
            filenameWithExtension = Path.GetFileName(filePath);
            fileLastWriteDate = File.GetLastWriteTime(filePath);
            stream = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePathOrStream"/> class.
        /// </summary>
        /// <param name="filename">The filename with extension.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="fileLastWriteDate">The file last write date.</param>
        public FilePathOrStream(string filename, Stream stream, DateTime fileLastWriteDate)
        {
            filePath = null;
            this.filenameNoExtension = GetFileNameWithoutExtension(filename);
            this.filenameWithExtension = filename;
            this.stream = stream;
            this.fileLastWriteDate = fileLastWriteDate;
        }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public string FilePath { get { return filePath; } }

        /// <summary>
        /// Gets the file name without extension.
        /// </summary>
        /// <value>
        /// The file name without extension.
        /// </value>
        public string FileNameWithoutExtension { get { return filenameNoExtension; } }

        /// <summary>
        /// Gets the file name with extension.
        /// </summary>
        /// <value>
        /// The file name with extension.
        /// </value>
        public string FileNameWithExtension { get { return filenameWithExtension; } }

        /// <summary>
        /// Gets the stream.
        /// </summary>
        /// <value>
        /// The stream.
        /// </value>
        public Stream Stream { get { return stream; } }

        /// <summary>
        /// Gets or sets the file last write date.
        /// </summary>
        /// <value>
        /// The file last write date.
        /// </value>
        public DateTime FileLastWriteDate { get { return fileLastWriteDate; } }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (stream != null)
                        stream.Dispose();
                }
                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion


        /// <summary>
        /// Gets the file name without extension.
        /// From Path.GetFileNameWithoutExtension(filePath) https://referencesource.microsoft.com/#mscorlib/system/io/path.cs,e36b3edc1f3e2c8d
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        private static string GetFileNameWithoutExtension(string filename)
        {
            if (filename != null)
            {
                int i;
                if ((i = filename.LastIndexOf('.')) == -1)
                    return filename; // No path extension found
                else
                    return filename.Substring(0, i);
            }
            return null;
        }
    }
}
