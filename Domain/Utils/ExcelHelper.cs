using System.Globalization;
using System.Text.RegularExpressions;
using Exceltomysql.Domain.Models;
using OfficeOpenXml;

namespace Exceltomysql.Domain.Utils
{
    public class ExcelHelper
    {
        private readonly IConfiguration _configuration;
        public ExcelHelper(IConfiguration configuration)
        {
            this._configuration = configuration;
        }



        public List<string> GetColumnTypes(ExcelWorksheet worksheet)
        {
            int columnCount = GetColumnCount(worksheet);
            var maxLengths = GetMaxColumnLengths(worksheet);
            List<string> types = new List<string>();
            for (int col = 1; col <= columnCount; col++)
            {
                string columnType = DetectDataTypeByColumn(GetColumnValues(worksheet, col), maxLengths.ElementAt(col - 1).MaxLength);
                types.Add(columnType);

            }
            return types;
        }

        public string DetectDataTypeByColumn(List<string> columnValues, int maxLength)
        {
            bool hasLetters = columnValues.Any(value => value.Any(char.IsLetter));

            if (columnValues.All(value => int.TryParse(value, out _)))
                return "INT";

            if (columnValues.All(value => double.TryParse(value, out _)))
                return "DOUBLE";

            string[] dateFormats = { "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yy", "dd/MM/yyyy", "yyyy-MM-dd HH:mm:ss", "MM/dd/yyyy HH:mm:ss" };
            if (columnValues.All(value =>
                dateFormats.Any(format =>
                    DateTime.TryParseExact(value.Replace("'", ""), format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out _))))
            {
                return "DATETIME";
            }

            if (hasLetters)
            {
                if (maxLength <= 255)
                {
                    return $"VARCHAR({maxLength})";
                }
                else if (maxLength <= 65535)
                {
                    return "TEXT";
                }
                else if (maxLength <= 16777215)
                {
                    return "MEDIUMTEXT";
                }
                else
                {
                    return "LONGTEXT";
                }
            }


            return "LONGTEXT";
        }

        public List<string> GetColumnValues(ExcelWorksheet worksheet, int column)
        {
            List<string> values = new List<string>();

            if (worksheet.Dimension == null)
                throw new ArgumentException("Worksheet is empty.");

            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;

            if (column < 1 || column > colCount)
                throw new ArgumentOutOfRangeException(nameof(column), $"Column index {column} is out of range. Max column: {colCount}");

            for (int row = 2; row <= rowCount; row++) // Read each row in the column
            {

                string cellValue = worksheet.Cells[row, column].Text.Trim();
                values.Add(cellValue);
            }

            return values;
        }


        public int GetColumnCount(ExcelWorksheet worksheet)
        {
            int lastColumn = worksheet.Dimension.End.Column;
            int row = 1;

            int finalColumn = 1;

            for (int col = 1; col <= lastColumn; col++)
            {
                string cellValue = worksheet.Cells[row, col].Text.Trim();

                if (cellValue.Length > 0 && !cellValue.Trim().Equals(""))
                {
                    finalColumn = col;
                }
                else
                {
                    return finalColumn;
                }
            }
            return finalColumn;
        }
        public List<ColumnInfo> GetMaxColumnLengths(ExcelWorksheet worksheet)
        {
            List<string> mysqlReservedWords = _configuration.GetSection("MySQLReservedWords").Get<List<string>>();
            var maxColumnInfo = new List<ColumnInfo>();
            int rowCount = worksheet.Dimension.Rows;
            int colCount = GetColumnCount(worksheet);

            for (int col = 1; col <= colCount; col++)
            {
                string columnName = worksheet.Cells[1, col].Text.Trim();
                int maxLength = 0;
                string maxValue = "";

                for (int row = 2; row <= rowCount; row++)
                {
                    string cellValue = worksheet.Cells[row, col].Text.Trim();
                    if (cellValue.Length > maxLength)
                    {
                        maxLength = cellValue.Length;
                        maxValue = cellValue;
                    }
                }


                foreach (var word in mysqlReservedWords)
                {
                    if (word.ToLower().Equals(columnName))
                    {
                        columnName = columnName + "_1";
                    }
                }
                columnName = Regex.Replace(columnName, "[^a-zA-Z0-9_]", "");
                // maxLength = maxLength == 0 ? 100 : maxLength;
                maxColumnInfo.Add(new ColumnInfo
                {
                    ColumnName = columnName,
                    MaxLength = maxLength,
                    MaxValue = maxValue
                });
            }

            return maxColumnInfo;
        }
    }
}