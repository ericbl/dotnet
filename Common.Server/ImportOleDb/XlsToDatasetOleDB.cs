using System;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace Common.Server.ImportOleDb
{
    /// <summary>
    /// Liest Daten aus einer Excel oder CSV Datei via OleDB
    /// </summary>
    /// <remarks>Funktioniert mit ACE Treiber für xlsx oder in x64 für andere Format, oder mit dem Microsoft.Jet.OLEDB.4.0 Treiber in Win32 OS!
    /// ACE Treiber kann auch in Win32 laufen, wenn Office 2007/2010 Win32 installiert ist.</remarks>
    public class XlsToDatasetOleDB
    {
        /// <summary>
        /// Connection string for ACE in x64/xlsx, adapted from http://connectionstrings.com/excel
        /// </summary>
        private const string AceExcelConnectionFormat = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='{1};HDR={2};{3};{4}'";

        /// <summary>
        /// Connection string for ACE in x64/xlsx, adapted from http://connectionstrings.com/access
        /// </summary>
        private const string AceAccessConnectionFormat = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}";

        /// <summary>
        /// Connection string for Jet in x86, adapted from http://connectionstrings.com/excel
        /// </summary>
        private const string JetConnectionFormat = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='{1};HDR={2};{3};{4}'";

        /// <summary>
        /// Holt die Daten aus einer Excel- oder CSV-Datei in ein DataSet
        /// </summary>
        /// <param name="fileName">Pfad zur Excel-Datei</param>
        /// <param name="headers">Haben die Tabellenspalten Überschriften</param>
        /// <param name="guessColumnTypes">Sollen die Spalten Typen anerkannt werden? Leider oft nicht korrekt (double statt int, DateTime statt TimeSpan, etc)!</param>
        /// <param name="forceACEDriver">Wenn <c>true</c> wird immer den ACE Treiber benutzt (nur für x86 relevant, wenn Office 2007/2010 x86 installiert ist).
        /// Wenn false (default) wird in x86 immer Jet benutzt, und ACE nur in x64 Modus. xlsx erfordert ACE, also funktionniert nicht in x86 ohne Office 2007/2010.</param>
        /// <returns>DataSet</returns>
        public static DataSet GetExcelDataSet(string fileName, bool headers, bool guessColumnTypes, bool forceACEDriver = false)
        {
            if (File.Exists(fileName) == false)
            {
                throw new ArgumentException("Die Datei " + fileName + " exitiert nicht");
            }

            var ds = new DataSet();

            using (var con = new OleDbConnection())
            {
                string fileExtension = Path.GetExtension(fileName).ToLower();
                // x64: no Jet support, but ACE (Access Connectivity Engine) which is only x64 for binary format.
                // x86: no ACE support for CSV in Excel, thus Jet! Jet is however not possible with Access file in 2010.
                string connectionFormat = (Environment.Is64BitOperatingSystem || forceACEDriver) ? AceExcelConnectionFormat : JetConnectionFormat;
                string hdr = headers ? "No" : "Yes"; // "HDR=Yes;" indicates that the first row contains columnnames, not data. "HDR=No;" indicates the opposite.
                string imex = guessColumnTypes ? string.Empty : "IMEX=1;ImportMixedTypes=Text;MAXSCANROWS=0";
                switch (fileExtension)
                {
                    case ".csv":
                        con.ConnectionString = string.Format(connectionFormat, Path.GetDirectoryName(fileName), "Text", hdr, imex, "FMT=Delimited");
                        FillDataSetFromCsv(con, ds, fileName);
                        break;
                    case ".xls": // Excel 97 (8) format
                        con.ConnectionString = string.Format(connectionFormat, fileName, "Excel 8.0", hdr, imex, string.Empty);
                        FillDataSetFromExcel(con, ds);
                        break;
                    case ".xlsx": // Excel 2007 (12) und 2010 (14) format
                        con.ConnectionString = string.Format(AceExcelConnectionFormat, fileName, "Excel 12.0", hdr, imex, string.Empty);
                        FillDataSetFromExcel(con, ds);
                        break;
                    case "rl":
                    case "mdl":
                        con.ConnectionString = string.Format(AceAccessConnectionFormat, fileName);
                        FillDataSetFromExcel(con, ds);
                        break;

                    default:
                        throw new ApplicationException("Das Datei-Format '{0}' wird nicht unterstützt.");
                }
            }

            return ds;
        }

        /// <summary>
        /// Converts the data from the table to a CSV file defined in targetFile
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="csvSeparator">The CSV separator.</param>
        /// <param name="targetFile">The target file.</param>
        public static void ConvertTableToCsv(DataTable table, string csvSeparator, string targetFile)
        {
            if (File.Exists(targetFile))
            {
                File.Delete(targetFile);
            }

            using (var wrtr = new StreamWriter(targetFile))
            {
                for (int x = 0; x < table.Rows.Count; x++)
                {
                    string rowString = string.Empty;
                    for (int y = 0; y < table.Columns.Count; y++)
                    {
                        rowString += "\"" + table.Rows[x][y] + csvSeparator;
                    }

                    wrtr.WriteLine(rowString);
                }
            }
        }

        /// <summary>
        /// Fills the data set from excel.
        /// </summary>
        /// <param name="con">The con.</param>
        /// <param name="ds">The ds.</param>
        private static void FillDataSetFromExcel(OleDbConnection con, DataSet ds)
        {
            con.Open();
            DataTable sheets = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            if (sheets != null)
            {
                // return 4 sheets instead of 3, Sheet1, Sheet1_, Sheet2, Sheet3
                foreach (DataRow sheet in sheets.Rows)
                {
                    string tableName = sheet["Table_Name"].ToString();
                    // ignore the strange duplicate here, Sheet1_ contains the same data as Sheet1
                    if (tableName.EndsWith("_"))
                    {
                        continue;
                    }

                    string sql = "SELECT * FROM [" + tableName + "]";
                    using (var adap = new OleDbDataAdapter(sql, con))
                    {
                        adap.Fill(ds, tableName);
                    }

                    if (tableName.EndsWith("$"))
                    {
                        ds.Tables[tableName].TableName = tableName.Substring(0, tableName.Length - 1);
                    }
                }
            }

            con.Close();
        }

        /// <summary>
        /// Fills the data set from CSV.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="ds">The ds.</param>
        /// <param name="filename">The filename.</param>
        private static void FillDataSetFromCsv(OleDbConnection conn, DataSet ds, string filename)
        {
            var cmdString = string.Format("SELECT * FROM [{0}]", Path.GetFileName(filename));

            conn.Open();
            using (var adap = new OleDbDataAdapter(cmdString, conn))
            {
                adap.Fill(ds, "Test");
            }

            conn.Close();
        }
    }
}