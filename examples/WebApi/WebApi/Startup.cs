using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Owin;
using System.Configuration;
using WebApi.Support;
using Microsoft.IdentityModel.Tokens;
using Aserto.Clients.Authorizer;
using System.Reflection;
using Aserto.Clients.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Owin.Logging;
using Aserto.Middleware.Options;
using Aserto.Middleware;
using log4net.Appender;
using System;
using Microsoft.Owin.Extensions;
using System.Web.Http;
using Ninject;

[assembly: OwinStartup(typeof(WebApi.Startup))]

namespace WebApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {            
            var domain = $"https://{ConfigurationManager.AppSettings["Auth0Domain"]}/";
            var apiIdentifier = ConfigurationManager.AppSettings["Auth0ApiIdentifier"];

            var keyResolver = new OpenIdConnectSigningKeyResolver(domain);
            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {                                   
                    AuthenticationMode = AuthenticationMode.Active,
                    TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidAudience = apiIdentifier,
                        ValidIssuer = domain,
                        IssuerSigningKeyResolver = (token, securityToken, kid, parameters) => keyResolver.GetSigningKey(kid)
                    }
                });


            // Example setup using the aserto owin middleware
            // app.Use<Aserto.Middleware.AsertoMiddleware>(LoggerFactory.Default, AuthorizerClientHelper.GetAsertoOptions(), AuthorizerClientHelper.GetClient());

            // Configure Web API
            WebApiConfig.Configure(app);
        }

        
    }   
}
