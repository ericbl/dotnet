using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Common.Files
{
    /// <summary>
    /// A C# class generator from a CSV file.
    /// </summary>
    public class CSharpClassGeneratorFromCSV
    {
        /// <summary>
        /// Generates a C# class from a CSV file and write the cs file to the same folder as the CSV file
        /// </summary>
        /// <param name="csvFilePath">The CSV file path.</param>
        /// <param name="classAttribute">The class attribute.</param>
        /// <param name="propertyAttribute">The property attribute.</param>
        /// <exception cref="System.IO.FileNotFoundException">CSV file not found</exception>
        public static void WriteCSharpClassCodeFromCsvFile(string csvFilePath, string classAttribute = "", string propertyAttribute = "")
        {
            if (!File.Exists(csvFilePath))
                throw new FileNotFoundException(csvFilePath);
            string code = CSharpClassCodeFromCsvFile(csvFilePath, classAttribute, propertyAttribute);

            string outputFile = Path.GetDirectoryName(csvFilePath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(csvFilePath) + ".cs";
            using (var sw = new StreamWriter(outputFile, false, System.Text.Encoding.Default))
            {
                sw.Write(code);
            }
        }

        /// <summary>
        /// Generates a C# class from a CSV file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="classAttribute">The class attribute.</param>
        /// <param name="propertyAttribute">The property attribute.</param>
        /// <returns>The generated class as string</returns>
        public static string CSharpClassCodeFromCsvFile(string filePath, string classAttribute = "", string propertyAttribute = "")
        {
            if (string.IsNullOrWhiteSpace(propertyAttribute) == false)
                propertyAttribute += "\n\t";
            if (string.IsNullOrWhiteSpace(propertyAttribute) == false)
                classAttribute += "\n";

            char delimiter = CSVDelimiter.DelimiterCharFromCurrentCulture;

            string[] lines = File.ReadAllLines(filePath);
            string[] columnNames = lines.First().Split(delimiter).Select(str => str.Trim()).ToArray();
            string[] data = lines.Skip(1).ToArray();

            string className = Path.GetFileNameWithoutExtension(filePath);
            // use StringBuilder for better performance
            string code = String.Format("{0}public class {1} {{ \n", classAttribute, className);

            for (int columnIndex = 0; columnIndex < columnNames.Length; columnIndex++)
            {
                var columnName = Regex.Replace(columnNames[columnIndex], @"[\s\.]", string.Empty, RegexOptions.IgnoreCase);
                if (string.IsNullOrEmpty(columnName))
                    columnName = "Column" + (columnIndex + 1);
                code += "\t" + GetVariableDeclaration(data, delimiter, columnIndex, columnName, propertyAttribute) + "\n\n";
            }

            code += "}\n";
            return code;
        }

        /// <summary>
        /// Gets the variable declaration.
        /// </summary>
        /// <param name="data">       The data.</param>
        /// <param name="delimiter">  The delimiter.</param>
        /// <param name="columnIndex">Index of the column.</param>
        /// <param name="columnName"> Name of the column.</param>
        /// <param name="attribute">  (Optional) The attribute.</param>
        /// <returns>
        /// The variable declaration.
        /// </returns>
        public static string GetVariableDeclaration(string[] data, char delimiter, int columnIndex, string columnName, string attribute = null)
        {
            string[] columnValues = data.Select(line => line.Split(delimiter)[columnIndex].Trim()).ToArray();
            string typeAsString;

            if (AllDateTimeValues(columnValues))
            {
                typeAsString = "DateTime";
            }
            else if (AllDateTimeNullableValues(columnValues))
            {
                typeAsString = "DateTime?";
            }
            else if (AllIntValues(columnValues))
            {
                typeAsString = "int";
            }
            else if (AllDoubleValues(columnValues))
            {
                typeAsString = "double";
            }
            else
            {
                typeAsString = "string";
            }

            string declaration = string.Format("{0}public {1} {2} {{ get; set; }}", attribute, typeAsString, columnName);
            return declaration;
        }

        /// <summary>
        /// Check whether all values are double.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns><c>True</c> if all values are double</returns>
        public static bool AllDoubleValues(string[] values)
        {
            double d;
            return values.All(val => double.TryParse(val, out d));
        }

        /// <summary>
        /// Check whether all values are int.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns><c>True</c> if all values are int</returns>
        public static bool AllIntValues(string[] values)
        {
            int d;
            return values.All(val => int.TryParse(val, out d));
        }

        /// <summary>
        /// Check whether all values are DateTime.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns><c>True</c> if all values are DateTime</returns>
        public static bool AllDateTimeValues(string[] values)
        {
            DateTime d;
            return values.All(val => DateTime.TryParse(val, out d));
        }

        /// <summary>
        /// Check if the values are null-able DateTime: check first of they are a DateTime or null, and then if more than 80% are DateTime.
        /// Slow because check all values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns><c>True</c> if over 80% of the values are date time</returns>
        public static bool AllDateTimeNullableValues(string[] values)
        {
            DateTime d;
            bool nullOrDateTime = values.All(val => DateTime.TryParse(val, out d) || string.IsNullOrEmpty(val));
            if (nullOrDateTime)
            {
                return values.Count(val => DateTime.TryParse(val, out d)) > 0.8 * values.Length;
            }
            return false;
        }

        // add other types if you need...
    }
}
