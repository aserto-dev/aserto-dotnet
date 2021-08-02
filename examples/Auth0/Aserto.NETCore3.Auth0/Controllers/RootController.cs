using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aserto.NETCore3.Auth0.Controllers
{

    [ApiController]
    [Route("/")]
    public class RootController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return "This is an example app";
        }
    }
}
