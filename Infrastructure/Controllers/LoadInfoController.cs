using Exceltomysql.Application.Dtos.Requests;
using Exceltomysql.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoadInfoController : ControllerBase
    {
        private readonly ILoadInfoService _loadInfoService;

        public LoadInfoController(ILoadInfoService loadInfoService)
        {
            _loadInfoService = loadInfoService;
        }
        [HttpPost]
        [Route("process")]
        public async Task<IActionResult> ProcessFile([FromForm] IFormFile file, [FromForm] MySqlConfigDTO mySqlConfig)
        {
            var result = await _loadInfoService.ProcessExcelFile(file, mySqlConfig);

            return Ok(result);
        }
    }
}