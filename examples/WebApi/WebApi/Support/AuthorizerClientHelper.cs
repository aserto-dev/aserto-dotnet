using Aserto.Clients.Authorizer;
using Aserto.Clients.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Extensions.Options;
using System.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Aserto.Middleware.Options;
using Microsoft.Owin;
using System.Diagnostics.Eventing.Reader;

namespace WebApi.Support
{
    public static class AuthorizerClientHelper
    {
        public static IAuthorizerAPIClient GetClient()
        {
            AsertoAuthorizerOptions authzOpts = new AsertoAuthorizerOptions();
            authzOpts.AuthorizerApiKey = ConfigurationManager.AppSettings["Authorizer.API.Key"];
            authzOpts.TenantID = ConfigurationManager.AppSettings["Authorizer.TenantID"];
            authzOpts.ServiceUrl = ConfigurationManager.AppSettings["Authorizer.ServiceURL"];
            authzOpts.Insecure = Convert.ToBoolean(ConfigurationManager.AppSettings["Authorizer.Insecure"]);
            var authorizerOptions = Options.Create(authzOpts);

            return new AuthorizerAPIClient(authorizerOptions, new NullLoggerFactory());            
        }

        public static AsertoOptions GetAsertoOptions()
        {
            AsertoOptions options = new AsertoOptions();
            options.PolicyName = "policy-todo";
            options.PolicyInstanceLabel = "policy-todo";
            options.PolicyRoot = "todoApp";
            options.PolicyPathMapper = (root, request) =>
            {
                if (request == null)
                {
                    return "todoApp.GET.todos"; // has default allowed method set to true in policy.
                }
                if (request is OwinRequest)
                {
                    {
                        if (request.Path.ToString().Contains("public"))
                        {
                            return "todoApp.GET.todos"; // has default allowed method set to true in policy.
                        }
                        return "todoApp.POST.todos"; // checks that provided identity (extracted from jwt sub) is a memeber of the editor or admin group
                    }
                }
                else
                {
                    if (request.RequestUri.LocalPath.ToString().Contains("public"))
                    {
                        return "todoApp.GET.todos"; // has default allowed method set to true in policy.
                    }

                    return "todoApp.POST.todos"; // checks that provided identity (extracted from jwt sub) is a memeber of the editor or admin group
                }                
            };

            return options;
        }
    }
}