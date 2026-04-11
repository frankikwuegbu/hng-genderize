using hng_genderizeApp.Common;
using hng_genderizeApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace hng_genderizeApp.Controllers
{
    [ApiController]
    [Route("api")]
    public class NamesController : ControllerBase
    {
        private readonly GenderizeServices _genderizeService;

        public NamesController(GenderizeServices genderizeService)
        {
            _genderizeService = genderizeService;
        }

        [HttpGet("helloworld")]
        public Task<string> HelloWorld()
        {
            return Task.FromResult("Hello World!");
        }

        [HttpGet("classify")]
        public async Task<ActionResult<Result>> Classify([FromQuery]string? name)
        {
            return await _genderizeService.GetGender(name);
        }
    }
}
