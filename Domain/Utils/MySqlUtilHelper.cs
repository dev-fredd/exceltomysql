using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Exceltomysql.Application.Dtos.Requests;
using MySql.Data.MySqlClient;
using OfficeOpenXml;

namespace Exceltomysql.Domain.Utils
{
    public class MySqlUtilHelper
    {
        private readonly ExcelHelper _excelHelper;
        private readonly IConfiguration _configuration;

        public MySqlUtilHelper(ExcelHelper excelHelper, IConfiguration configuration)
        {
            this._excelHelper = excelHelper;
            this._configuration = configuration;
        }
        public string BuildMySqlConnectionString(MySqlConfigDTO mySqlConfigDTO)
        {
            return $"server={mySqlConfigDTO.Server};database={mySqlConfigDTO.Database};user={mySqlConfigDTO.User};password={mySqlConfigDTO.Password};";
        }

        public string ExecMySqlQuery(string query, string connectionString)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        Console.WriteLine($"Executing Query: {query}...");
                        cmd.ExecuteNonQuery();
                    }
                }
                return "Execution successful!";
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error executing query: {e.Message}");
                return $"Execution failed: {e.Message}";
            }
        }

        public async Task<int> InsertRows(ExcelWorksheet worksheet, int rowCount, string tableName, string connectionString)
        {
            int columnCount = _excelHelper.GetColumnCount(worksheet);
            int rowsAffected = 0;
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                for (int row = 2; row <= rowCount; row++)
                {
                    string insertQuery = $"INSERT INTO {tableName} VALUES (NULL,";
                    for (int col = 1; col <= columnCount; col++)
                    {
                        string value = worksheet.Cells[row, col].Text.Trim();
                        DateTime parsedDate;
                        string[] dateFormats = { "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yy", "dd/MM/yyyy", "yyyy-MM-dd HH:mm:ss", "MM/dd/yyyy HH:mm:ss" };
                        insertQuery += string.IsNullOrEmpty(value.Replace(" ", ""))
                            ? "NULL, "
                            : int.TryParse(value, out _) || double.TryParse(value, out _)
                                ? $"{value}, "
                                : DateTime.TryParseExact(value, dateFormats, null, System.Globalization.DateTimeStyles.None, out parsedDate)
                                    ? $"'{parsedDate.ToString("yyyy-MM-dd HH:mm:ss")}', "
                                    : $"'{value.Replace("'", "")}', ";
                    }
                    insertQuery = insertQuery.TrimEnd(',', ' ') + ");";
                    //Console.WriteLine(insertQuery);
                    using (var cmd = new MySqlCommand(insertQuery, conn))
                    {
                        rowsAffected++;
                        Console.WriteLine($"Inserting row {rowsAffected} ...");
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            return rowsAffected;
        }

        public string GetQueryCreateTable(ExcelWorksheet worksheet, string tableName)
        {
            List<string> mysqlReservedWords = _configuration.GetSection("MySQLReservedWords").Get<List<string>>();
            int columnCount = _excelHelper.GetColumnCount(worksheet);
            int rowCount = worksheet.Dimension.End.Row;
            string createTableQuery = $"CREATE TABLE IF NOT EXISTS {tableName} (Id MEDIUMINT NOT NULL AUTO_INCREMENT PRIMARY KEY,";
            List<string> columns = new List<string>();

            for (int col = 1; col <= columnCount; col++)
            {
                string columnName = Regex.Replace(worksheet.Cells[1, col].Text, "[^0-9a-zA-ZÀ-ÿ ]", "").Replace(" ", "_")
                    .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                    .Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U")
                    .Replace("ñ", "n").Replace("Ñ", "N").ToLower();

                foreach (var word in mysqlReservedWords)
                {
                    if (word.ToLower().Equals(columnName))
                    {
                        columnName += "_1";
                    }
                }
                int countColumns = columns.Where(n => Regex.Replace(n, "[^a-zA-Z]", "").Replace("_", "") == Regex.Replace(columnName, "[^a-zA-Z]", "").Replace("_", "")).Count();
                if (countColumns > 0)
                {
                    columnName = $"{columnName}_{countColumns + 1}";
                    columnName = columnName.TrimStart('_').TrimEnd('_').ToLower();
                }
                worksheet.Cells[1, col].Value = CapitalizeString(columnName);
                columns.Add(columnName);



            }

            var maxLengths = _excelHelper.GetMaxColumnLengths(worksheet);

            for (int col = 1; col <= columnCount; col++)
            {
                string columnType = _excelHelper.DetectDataTypeByColumn(_excelHelper.GetColumnValues(worksheet, col), maxLengths.ElementAt(col - 1).MaxLength);

                createTableQuery += $"{maxLengths.ElementAt(col - 1).ColumnName} {columnType}, ";
            }

            createTableQuery = createTableQuery.TrimEnd(',', ' ') + ");";
            return createTableQuery;
        }

        public string CapitalizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input; 
            }
            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }
    }


}