using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Owin;
using System.Configuration;
using WebApi.Support;
using Microsoft.IdentityModel.Tokens;
using Aserto.Owin.Middleware;
using Aserto.Owin.Middleware.Options;
using Aserto.Clients.Authorizer;
using System.Reflection;
using Aserto.Clients.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;

[assembly: OwinStartup(typeof(WebApi.Startup))]

namespace WebApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var domain = $"https://{ConfigurationManager.AppSettings["Auth0Domain"]}/";
            var apiIdentifier = ConfigurationManager.AppSettings["Auth0ApiIdentifier"];
            AsertoAuthorizerOptions authzOpts = new AsertoAuthorizerOptions();
            var authorizerOptions = Options.Create(authzOpts);
            var logggerFactory = new NullLoggerFactory();
            var client = new AuthorizerAPIClient(authorizerOptions, logggerFactory);

            app.Use<AsertoMiddleware>(logggerFactory,client);

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

            // Configure Web API
            WebApiConfig.Configure(app);
        }
    }   
}
