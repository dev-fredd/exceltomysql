using Exceltomysql.Application.Dtos.Requests;
using Exceltomysql.Application.Dtos.Responses;
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
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ProcessResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Produces("application/json", "text/plain")]
        public async Task<IActionResult> ProcessFile([FromForm] FileUploadRequest fileUploadRequest, [FromForm] MySqlConfigDTO mySqlConfig)
        {
            var result = await _loadInfoService.ProcessExcelFile(fileUploadRequest.File, mySqlConfig);

            return Ok(result);
        }
    }
}