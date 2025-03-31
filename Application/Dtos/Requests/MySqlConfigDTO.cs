namespace Exceltomysql.Application.Dtos.Requests
{
    public class MySqlConfigDTO
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}