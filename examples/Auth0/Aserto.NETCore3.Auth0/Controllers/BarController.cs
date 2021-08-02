using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aserto.NETCore3.Auth0.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BarController : ControllerBase
    {
        private static readonly string[] Messages = new[]
        {
            "Bar", "Bar1", "Bar2", "Bar3", "Bar4", "Bar5", "Bar6", "Bar7", "Bar8", "Bar9"
        };

        private readonly ILogger<BarController> _logger;

        public BarController(ILogger<BarController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Msg> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new Msg
            {
                Date = DateTime.Now.AddDays(index),
                Message = Messages[rng.Next(Messages.Length)]
            })
            .ToArray();
        }
    }
}
