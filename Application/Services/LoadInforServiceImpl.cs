using System.Diagnostics;
using Exceltomysql.Application.Dtos.Requests;
using Exceltomysql.Application.Dtos.Responses;
using Exceltomysql.Domain.Ports;
using OfficeOpenXml;
using Exceltomysql.Domain.Utils;


namespace Exceltomysql.Application.Services
{
    public class LoadInforServiceImpl : ILoadInfoService
    {
        private readonly FileHelper _fileHelper;
        private readonly MySqlUtilHelper _mySqlHelper;

        public LoadInforServiceImpl(FileHelper fileHelper,MySqlUtilHelper mySqlHelper)
        {
            this._fileHelper = fileHelper;
            this._mySqlHelper = mySqlHelper;
        }
        public async Task<ProcessResponse> ProcessExcelFile(IFormFile file, MySqlConfigDTO mySqlConfig)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Console.WriteLine("... Start ...");
            var tempFilePath = _fileHelper.GetTempFile(file);
            Console.WriteLine("Reading excel file...");
            ExcelWorksheet worksheet;
            ProcessResponse processResponse = new ProcessResponse("Failed",500,0,"00:00","");
            using (var package = new ExcelPackage(new FileInfo(tempFilePath)))
            {
                try
                {
                    if (package.Workbook.Worksheets.Count == 0)
                    {
                        throw new InvalidOperationException("Excel file contains no worksheets");
                    }

                    var workbook = package.Workbook;

                    if (workbook == null || workbook.Worksheets.Count == 0)
                    {
                        throw new InvalidOperationException(
                            "Excel file is empty or corrupt. " +
                            "Please verify the file contains data and is a valid Excel file.");
                    }
                    worksheet = workbook.Worksheets[0];
                    if (worksheet == null)
                        throw new Exception("WorkSheet is Empty!");
                    if (worksheet.Dimension == null)
                    {
                        throw new InvalidOperationException(
                            "First worksheet appears to be empty. " +
                            "Please check the file contains data in the first sheet.");
                    }
                    int rowsAffected = 0;
                    int columnCount = worksheet.Dimension.End.Column;
                    int rowCount = worksheet.Dimension.End.Row;
                    string timestamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
                    string tableName = file.FileName.Substring(0, file.FileName.IndexOf(".")).ToLower() + timestamp;
                    string createTableQuery = _mySqlHelper.GetQueryCreateTable(worksheet, tableName);
                    string connectionString = _mySqlHelper.BuildMySqlConnectionString(mySqlConfig);
                    string tableCreated = _mySqlHelper.ExecMySqlQuery(createTableQuery, connectionString);
                    Console.WriteLine($"Creating table {tableName} result:{tableCreated}");
                    rowsAffected = _mySqlHelper.InsertRows(worksheet, rowCount, columnCount, tableName, connectionString);
                    stopwatch.Stop();
                    TimeSpan elapsed = stopwatch.Elapsed;
                    Console.WriteLine($"Table {tableName} created - {rowsAffected} rows inserted - Execution time: {elapsed.Minutes} min {elapsed.Seconds} sec");
                    processResponse = new ProcessResponse("Success", 200, rowsAffected,$"{elapsed.Minutes} min {elapsed.Seconds} sec",tableName);
                    _fileHelper.TryDeleteFile(tempFilePath);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
                return processResponse;
                
            }
        }




  
        

       
    }
}