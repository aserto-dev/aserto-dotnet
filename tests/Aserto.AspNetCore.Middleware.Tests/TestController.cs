using Aserto.AspNetCore.Middleware.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Aserto.AspNetCore.Middleware.Tests
{
    internal class TestController
    {
        [Aserto]
        [Route("/test")]
        public class RouteController: ControllerBase
        {
        }
    }
}