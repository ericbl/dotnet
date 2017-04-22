using Common.Files;
using Common.Reflection;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Common.Server.OpenXml
{
    /// <summary>
    /// Import Excel file (xlsx) to generic list using DocumentFormat.OpenXml
    /// Inspired/copied from Shenwei Liu 4/11/2014
    /// http://www.codeproject.com/Articles/770240/Importing-Excel-Data-to-a-Generic-List-Using-Open
    /// </summary>
    public static class ExcelReader
    {
        private const bool LowerAllCollumnNames = false;
        private const bool IgnoreEmptyCell = true; // absent = false in original code
        private const bool ReadInnerValueInsteadOfFormula = true;  // absent = false in original code

        /// <summary>
        /// Reads the Spreadsheet document and convert the data to a list of T.
        /// </summary>
        /// <typeparam name="T">type of the object in the list</typeparam>
        /// <param name="filePath">The file to open the SpreadsheetDocument.</param>
        /// <returns>
        /// A list of T
        /// </returns>
        /// <exception cref="FileNotFoundException">filePath does not exists</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "per design")]
        public static IList<T> GetDataToList<T>(string filePath)
            where T : new()
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            return GetDataToList<T>(new FilePathOrStream(filePath));
        }

        /// <summary>
        /// Reads the Spreadsheet document and convert the data to a list of T.
        /// </summary>
        /// <typeparam name="T">type of the object in the list</typeparam>
        /// <param name="filePathOrStream">The file to open the SpreadsheetDocument.</param>
        /// <returns>A list of T</returns>
        /// <exception cref="Exception">No worksheet.</exception>
        public static IList<T> GetDataToList<T>(FilePathOrStream filePathOrStream)
            where T : new()
        {
            return GetDataToList(filePathOrStream, null, CreateObjectFromStringArray.CreateObject<T>);
        }

        /// <summary>
        /// Reads the Spreadsheet document and convert the data to a list of T.
        /// </summary>
        /// <typeparam name="T">type of the object in the list</typeparam>
        /// <param name="filePathOrStream">The file path or stream.</param>
        /// <param name="sheetName">Name of the sheet. First sheet of the document if null or empty</param>
        /// <param name="addCustomObject">The function to add custom object.</param>
        /// <returns>
        /// A list of T
        /// </returns>
        /// <exception cref="ArgumentNullException">filePathOrStream</exception>
        /// <exception cref="Exception">No worksheet.</exception>
        public static IList<T> GetDataToList<T>(
            FilePathOrStream filePathOrStream,
            string sheetName,
            Func<IEnumerable<MemberInfo>, IList<string>, IList<string>, bool, T> addCustomObject)
        {
            if (filePathOrStream == null)
            {
                throw new ArgumentNullException(nameof(filePathOrStream));
            }

            IList<T> resultList = null;

            // Open the spreadsheet document for read-only access.
            if (!string.IsNullOrEmpty(filePathOrStream.FilePath) && File.Exists(filePathOrStream.FilePath))
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(filePathOrStream.FilePath, false))
                {
                    resultList = XLDocumentToList(document, sheetName, addCustomObject);
                }
            }
            else if (filePathOrStream.Stream != null)
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(filePathOrStream.Stream, false))
                {
                    resultList = XLDocumentToList(document, sheetName, addCustomObject);
                }
            }

            return resultList;
        }

        /// <summary>
        /// Reads the Spreadsheet document and convert the data to a list of T.
        /// </summary>
        /// <typeparam name="T">type of the object in the list</typeparam>
        /// <param name="document">The document.</param>
        /// <param name="sheetName">Name of the sheet. First sheet of the document if null or empty</param>
        /// <param name="addCustomObject">The function to add custom object.</param>
        /// <returns>
        /// A list of T
        /// </returns>
        /// <exception cref="ArgumentNullException">document</exception>
        /// <exception cref="Exception">No worksheet.</exception>
        private static IList<T> XLDocumentToList<T>(
            SpreadsheetDocument document,
            string sheetName,
            Func<IEnumerable<MemberInfo>, IList<string>, IList<string>, bool, T> addCustomObject)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            List<T> resultList = new List<T>();

            WorkbookPart wbPart = document.WorkbookPart;
            Sheet sheet = null;
            WorksheetPart wsPart = null;

            // Find the worksheet
            if (string.IsNullOrEmpty(sheetName))
            {
                // get the first per default
                sheet = wbPart.Workbook.Descendants<Sheet>().FirstOrDefault();
            }
            else
            {
                sheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == sheetName).FirstOrDefault();
            }

            if (sheet != null)
            {
                // Retrieve a reference to the worksheet part.
                wsPart = (WorksheetPart)wbPart.GetPartById(sheet.Id);
            }

            if (wsPart == null)
            {
                throw new ArgumentNullException($"No worksheet with name {sheetName}.");
            }
            else
            {
                //List to hold custom column names for mapping data to columns (index-free).
                var columnNames = new List<string>();

                //List to hold column address letters for handling empty cell issue (handle empty cell issue).
                var columnLetters = new List<string>();

                //Iterate cells of custom header row.
                foreach (Cell cell in wsPart.Worksheet.Descendants<Row>().ElementAt(0))
                {
                    //Get custom column names.
                    //Remove spaces, symbols (except underscore), and make lower cases and for all values in columnNames list.
                    var cellValue = GetCellValue(document, cell);
                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        var colName = Regex.Replace(cellValue, @"[^A-Za-z0-9_]", string.Empty);
                        columnNames.Add(LowerAllCollumnNames ? colName.ToLower() : colName);
                    }

                    //Get built-in column names by extracting letters from cell references.
                    columnLetters.Add(GetColumnAddress(cell.CellReference));
                }

                //Used for sheet row data to be added through delegation.
                var rowData = new List<string>();

                // Get object properties once
                var properties = Utils.GetAllFieldsAndPropertiesOfClassOrdered(typeof(T));

                foreach (var row in GetUsedRows(document, wsPart))
                {
                    rowData.Clear();

                    //Iterate through prepared enumerable.
                    foreach (var cell in GetCellsForRow(row, columnLetters))
                    {
                        rowData.Add(GetCellValue(document, cell));
                    }

                    //Calls the delegated function to add it to the collection.
                    resultList.Add(addCustomObject(properties, rowData, columnNames, LowerAllCollumnNames));
                }
            }

            return resultList;
        }

        private static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            if (cell == null)
                return null;
            if (IgnoreEmptyCell && string.IsNullOrEmpty(cell.InnerText))
                return null;
            string value = cell.InnerText;

            if (ReadInnerValueInsteadOfFormula && cell.CellValue != null)
                value = cell.CellValue.InnerText;

            //Process values particularly for those data types.
            if (cell.DataType != null)
            {
                switch (cell.DataType.Value)
                {
                    //Obtain values from shared string table.
                    case CellValues.SharedString:
                        var sstPart = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                        value = sstPart.SharedStringTable.ChildElements[int.Parse(value)].InnerText;
                        break;

                    //Optional boolean conversion.
                    case CellValues.Boolean:
                        //value = KCOCommon.Strings.Helper.ParseBoolean(value.ToString());
                        //var booleanToBit = ConfigurationManager.AppSettings["BooleanToBit"];
                        //if (booleanToBit != "Y")
                        //{
                        //    value = value == "0" ? "FALSE" : "TRUE";
                        //}
                        break;
                }
            }

            return value;
        }

        private static IEnumerable<Row> GetUsedRows(SpreadsheetDocument document, WorksheetPart wsPart)
        {
            bool hasValue;
            //Iterate all rows except the first one.
            foreach (var row in wsPart.Worksheet.Descendants<Row>().Skip(1))
            {
                hasValue = false;
                foreach (var cell in row.Descendants<Cell>())
                {
                    //Find at least one cell with value for a row
                    if (!string.IsNullOrEmpty(GetCellValue(document, cell)))
                    {
                        hasValue = true;
                        break;
                    }
                }

                if (hasValue)
                {
                    //Return the row and keep iteration state.
                    yield return row;
                }
            }
        }

        private static IEnumerable<Cell> GetCellsForRow(Row row, List<string> columnLetters)
        {
            int workIdx = 0;
            foreach (var cell in row.Descendants<Cell>())
            {
                //Get letter part of cell address.
                var cellLetter = GetColumnAddress(cell.CellReference);

                //Get column index of the matched cell.
                int currentActualIdx = columnLetters.IndexOf(cellLetter);

                //Add empty cell if work index smaller than actual index.
                for (; workIdx < currentActualIdx; workIdx++)
                {
                    var emptyCell = new Cell() { DataType = null, CellValue = new CellValue(string.Empty) };
                    yield return emptyCell;
                }

                //Return cell with data from Excel row.
                yield return cell;
                workIdx++;

                //Check if it's ending cell but there still is any unmatched columnLetters item.
                if (cell == row.LastChild)
                {
                    //Append empty cells to enumerable.
                    for (; workIdx < columnLetters.Count(); workIdx++)
                    {
                        var emptyCell = new Cell() { DataType = null, CellValue = new CellValue(string.Empty) };
                        yield return emptyCell;
                    }
                }
            }
        }

        private static string GetColumnAddress(string cellReference)
        {
            //Create a regular expression to get column address letters.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellReference);
            return match.Value;
        }
    }
}
