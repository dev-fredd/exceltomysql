namespace Exceltomysql.Application.Dtos.Requests
{
    public class FileUploadRequest
    {
        public string UploaderName { get; set; }
        public string UploaderAdress { get; set; }
        public IFormFile File { get; set; }
    }
}