using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aserto.NETCore3.Controllers
{
    [ApiController]
    [Route("testwf")]
    public class TestWeatherForecastController : ControllerBase
    {
        private readonly ILogger<TestWeatherForecastController> _logger;

        public TestWeatherForecastController(ILogger<TestWeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            return "something";
        }
    }
}
