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
            authzOpts.AuthorizerApiKey = ConfigurationManager.AppSettings["Authorizer.API.Key"];
            authzOpts.TenantID = ConfigurationManager.AppSettings["Authorizer.TenantID"];
            authzOpts.ServiceUrl = ConfigurationManager.AppSettings["Authorizer.ServiceURL"];
            authzOpts.Insecure = Convert.ToBoolean(ConfigurationManager.AppSettings["Authorizer.Insecure"]); 
            var authorizerOptions = Options.Create(authzOpts);

            var client = new AuthorizerAPIClient(authorizerOptions, new NullLoggerFactory());

            AsertoOptions options = new AsertoOptions();
            options.PolicyName = "policy-todo";
            options.PolicyInstanceLabel = "policy-todo";
            options.PolicyRoot = "todoApp";
            options.PolicyPathMapper = (root, request) =>
            {
                if (request.Path.ToString().Contains("public"))
                {
                    return "todoApp.GET.todos";
                }
                return "todoApp.POST.todos";
            };

            app.Use<AsertoMiddleware>(LoggerFactory.Default, (object)options, client);

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
