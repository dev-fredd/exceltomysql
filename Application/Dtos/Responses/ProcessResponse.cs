namespace Exceltomysql.Application.Dtos.Responses
{
    public class ProcessResponse
    {
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public int Rowsaffected { get; set; }
        public string ExecutionTime { get; set; }
        public string TableName { get; set; }

        public ProcessResponse(string message,int statusCode,int rowsaffected,string executionTime,string tableName)
        {
            this.Message =message;
            this.StatusCode = statusCode;
            this.Rowsaffected = rowsaffected;
            this.ExecutionTime = executionTime;
            this.TableName = tableName;
        }
        public ProcessResponse()
        {
            
        }
    }
}