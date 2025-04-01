using System.Globalization;
using Exceltomysql.Domain.Models;
using OfficeOpenXml;

namespace Exceltomysql.Domain.Utils
{
    public class ExcelHelper
    {
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


            return "TEXT";
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

        public List<ColumnInfo> GetMaxColumnLengths(ExcelWorksheet worksheet)
        {
            var maxColumnInfo = new List<ColumnInfo>();
            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;

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