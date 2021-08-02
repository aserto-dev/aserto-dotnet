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
    public class FooController : ControllerBase
    {
        private static readonly string[] Messages = new[]
        {
            "Foo", "Foo1", "Foo2", "Foo3", "Foo4", "Foo5", "Foo6", "Foo7", "Foo8", "Foo9"
        };

        private readonly ILogger<BarController> _logger;

        public FooController(ILogger<BarController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Authorize("Aserto")]
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
