using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Common.Files
{
    /// <summary>
    /// Compare file and directory
    /// </summary>
    public static class Compare
    {
        /// <summary>
        /// Determines if the given directories contains files with the same content, with full byte comparison.
        /// </summary>
        /// <param name="folderPath1">The folder path1.</param>
        /// <param name="folderPath2">The folder path2.</param>
        /// <returns><c>true</c> if both directories contains exact same files; false otherwise</returns>
        public static bool DirectoryEquals(string folderPath1, string folderPath2)
        {
            // Determine if the same folder was referenced two times.
            if (folderPath1 == folderPath2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            DirectoryInfo dirInfo1 = new DirectoryInfo(folderPath1);
            DirectoryInfo dirInfo2 = new DirectoryInfo(folderPath2);
            List<FileInfo> files1 = dirInfo1.GetFiles().OrderBy(fi => fi.Name).ToList();
            List<FileInfo> files2 = dirInfo2.GetFiles().OrderBy(fi => fi.Name).ToList();

            if (files1.Count != files2.Count)
            {
                return false;
            }

            bool filesAreEquals = true;
            for (int i = 0; i < files1.Count; i++)
            {
                filesAreEquals = FilesContentsAreEqual(files1[i], files2[i]);
                if (!filesAreEquals)
                {
                    return false;
                }
            }

            return filesAreEquals;
        }

        /// <summary>
        /// Files the compare.
        /// </summary>
        /// <param name="file1">The file1.</param>
        /// <param name="file2">The file2.</param>
        /// <returns><c>True</c> if both file content are equal</returns>
        public static bool FileEquals(string file1, string file2)
        {
            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            return FilesContentsAreEqual(new FileInfo(file1), new FileInfo(file2));
        }

        private static bool FilesContentsAreEqual(FileInfo fileInfo1, FileInfo fileInfo2)
        {
            bool result;

            if (fileInfo1.Length != fileInfo2.Length)
            {
                result = false;
            }
            else
            {
                if (fileInfo1.Length < 85000)
                {
                    // read all bytes at once for object in the small object heap
                    // https://blogs.msdn.microsoft.com/dotnet/2011/10/03/large-object-heap-improvements-in-net-4-5/
                    return File.ReadAllBytes(fileInfo1.FullName).SequenceEqual(File.ReadAllBytes(fileInfo2.FullName));
                }

                // Otherwise, do a 
                using (var file1 = fileInfo1.OpenRead())
                {
                    using (var file2 = fileInfo2.OpenRead())
                    {
                        result = StreamsContentsAreEqual(file1, file2);
                    }
                }
            }

            return result;
        }

        private const int int64BufferSize = 1024 * sizeof(long);

        private static bool StreamsContentsAreEqual(Stream stream1, Stream stream2)
        {
            var buffer1 = new byte[int64BufferSize];
            var buffer2 = new byte[int64BufferSize];

            while (true)
            {
                int count1 = stream1.Read(buffer1, 0, int64BufferSize);
                int count2 = stream2.Read(buffer2, 0, int64BufferSize);

                if (count1 != count2)
                {
                    return false;
                }

                if (count1 == 0)
                {
                    return true;
                }

                int iterations = (int)Math.Ceiling((double)count1 / sizeof(Int64));
                for (int i = 0; i < iterations; i++)
                {
                    if (BitConverter.ToInt64(buffer1, i * sizeof(Int64)) != BitConverter.ToInt64(buffer2, i * sizeof(Int64)))
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the different line in the text files.
        /// </summary>
        /// <param name="filePath1">The file path1.</param>
        /// <param name="filePath2">The file path2.</param>
        /// <returns>Error message or null if equals</returns>
        /// <exception cref="ArgumentNullException">Input file path is null</exception>
        /// <exception cref="FileNotFoundException">
        /// </exception>
        public static string GetDifferentLineInTextFiles(string filePath1, string filePath2)
        {
            if (string.IsNullOrEmpty(filePath1) || string.IsNullOrEmpty(filePath2))
            {
                throw new ArgumentNullException("Input file path is null");
            }
            string[] lines1 = IOUtils.ReadTxtFileLines(filePath1, true);
            string[] lines2 = IOUtils.ReadTxtFileLines(filePath2, true);
            string fileNotFound = "No file or text content found for {0}";
            if (lines1 == null)
            {
                throw new FileNotFoundException(string.Format(fileNotFound, filePath1));
            }
            if (lines2 == null)
            {
                throw new FileNotFoundException(string.Format(fileNotFound, filePath2));
            }

            if (lines1.Length != lines2.Length)
            {
                return $"Different number of lines in files, {lines1.Length} in {filePath1} vs {lines2.Length} in {filePath2}";
            }
            string linesDiffer = "Input files have a first different line {0}: " + Environment.NewLine + "{1}" + Environment.NewLine + "{2}";
            for (int i = 0; i < lines1.Length; i++)
            {
                if (lines1[i] != lines2[i])
                {
                    return string.Format(linesDiffer, i, lines1[i], lines2[i]);
                }
            }

            return null;
        }
    }
}
