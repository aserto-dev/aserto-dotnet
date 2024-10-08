﻿using Aserto.Clients.Authorizer;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using WebApi.Support;

namespace WebApi.Controllers
{
    [RoutePrefix("api")]
    public class ApiController : System.Web.Http.ApiController
    {
       
        public ApiController()
        {                
        }

        [HttpGet]
        [Route("public")]
        public IHttpActionResult Public()
        {
            return Json(new
            {
                Message = "Hello from a public endpoint! You don't need to be authenticated to see this."
            });
        }

        [HttpGet]
        [Route("private")]
        [Authorize]
        [Aserto.Middleware.Extensions.Aserto]
        public IHttpActionResult Private()
        {
            return Json(new
            {
                Message = "Hello from a private endpoint! You need to be authenticated to see this."
            });
        }

        [Authorize]
        [Route("claims")]
        [HttpGet]
        public object Claims()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            return claimsIdentity.Claims.Select(c =>
            new
            {
                Type = c.Type,
                Value = c.Value
            });
        }
    }
}