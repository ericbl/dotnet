using System.Runtime.InteropServices;
using System.Text;

namespace Common.Files
{
    /// <summary>
    /// Manage reading and writing an .ini file using kernel32 methods
    /// </summary>
    class INIFile
    {
        [DllImport("kernel32.dll")]
        private static extern int WritePrivateProfileString(string ApplicationName, string KeyName, string StrValue, string FileName);
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string ApplicationName, string KeyName, string DefaultValue, StringBuilder ReturnString, int nSize, string FileName);

        /// <summary>
        /// Writes a value.
        /// </summary>
        /// <param name="SectionName">Name of the section.</param>
        /// <param name="KeyName">    Name of the key.</param>
        /// <param name="KeyValue">   The key value.</param>
        /// <param name="FileName">   Filename of the file.</param>
        public static void WriteValue(string SectionName, string KeyName, string KeyValue, string FileName)
        {
            WritePrivateProfileString(SectionName, KeyName, KeyValue, FileName);
        }

        /// <summary>
        /// Reads a value.
        /// </summary>
        /// <param name="SectionName">Name of the section.</param>
        /// <param name="KeyName">    Name of the key.</param>
        /// <param name="FileName">   Filename of the file.</param>
        /// <returns>
        /// The value.
        /// </returns>
        public static string ReadValue(string SectionName, string KeyName, string FileName)
        {
            StringBuilder szStr = new StringBuilder(255);
            GetPrivateProfileString(SectionName, KeyName, "", szStr, 255, FileName);
            return szStr.ToString().Trim();
        }
    }
}
