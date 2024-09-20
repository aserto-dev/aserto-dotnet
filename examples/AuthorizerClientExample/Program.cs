using Aserto.Clients.Authorizer;
using Aserto.Authorizer.V2.Api;
using Aserto.Clients.Options;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Aserto.Authorizer.V2;
using Google.Protobuf.WellKnownTypes;
using System.Runtime.Remoting.Contexts;
using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace AuthorizerClientExample
{
    internal class Program
    {
        static void Main(string[] args)
        {

            AsertoAuthorizerOptions authzOpts = new AsertoAuthorizerOptions();
            authzOpts.AuthorizerApiKey = ConfigurationManager.AppSettings["Authorizer.API.Key"];            
            authzOpts.TenantID = ConfigurationManager.AppSettings["Authorizer.TenantID"];
            authzOpts.ServiceUrl = ConfigurationManager.AppSettings["Authorizer.ServiceURL"];
            authzOpts.Insecure = Convert.ToBoolean(ConfigurationManager.AppSettings["Authorizer.Insecure"]);
                        
            var authorizerOptions = Options.Create(authzOpts);

            var client = new AuthorizerAPIClient(authorizerOptions, new NullLoggerFactory());

            // Example Is Request for policy-todo instance
            var request = new Aserto.Authorizer.V2.IsRequest();

            request.PolicyContext = new PolicyContext();
            request.PolicyContext.Path = "todoApp.GET.todos";
            request.PolicyContext.Decisions.Add("allowed");
            
            request.IdentityContext = new IdentityContext();
            request.IdentityContext.Type = IdentityType.None;
            request.PolicyInstance = new PolicyInstance();
            request.PolicyInstance.Name = "todo";
            request.PolicyInstance.InstanceLabel = "todo";

            var result = client.IsAsync(request);

            Task.WaitAll(result);

            Console.WriteLine(result);

            var result2 = client.ListPoliciesAsync(new ListPoliciesRequest() { PolicyInstance = new PolicyInstance(){
                Name="todo",
                InstanceLabel="todo"
            }
        });
            
            Task.WaitAll(result2);
            Console.WriteLine(result2);
        }
    }
}
