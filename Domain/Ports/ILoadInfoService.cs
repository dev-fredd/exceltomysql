using Exceltomysql.Application.Dtos.Requests;
using Exceltomysql.Application.Dtos.Responses;


namespace Exceltomysql.Domain.Ports
{
    public interface ILoadInfoService
    {
        Task<ProcessResponse> ProcessExcelFile(IFormFile file, MySqlConfigDTO mySqlConfig);
    }
}