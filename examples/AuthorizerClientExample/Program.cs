using Aserto.Clients.Authorizer;
using Aserto.Authorizer.V2.Api;
using Aserto.Clients.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aserto.Authorizer.V2;
using Google.Protobuf.WellKnownTypes;
using System.Configuration;
using Grpc.Net.Client;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AuthorizerClientExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                           .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                           .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = builder.Build();

            AsertoAuthorizerOptions authzOpts = new AsertoAuthorizerOptions();
            authzOpts.AuthorizerApiKey = config.GetSection("Authorizer:APIKey").Value;
            authzOpts.ServiceUrl = config.GetSection("Authorizer:ServiceUrl").Value;
            authzOpts.TenantID = config.GetSection("Authorizer:TenantID").Value;
            authzOpts.Insecure = Convert.ToBoolean(config.GetSection("Authorizer:Insecure").Value);
                        
            var authorizerOptions = Options.Create(authzOpts);

            //var handler = new WinHttpHandler();
            
            //handler.ServerCertificateValidationCallback =
            //    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            //var channel = GrpcChannel.ForAddress("https://localhost:8282",
            //  new GrpcChannelOptions { HttpHandler = handler });

            //var client = new Aserto.Authorizer.V2.Authorizer.AuthorizerClient(channel);
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

            var result =  client.IsAsync(request);

            Task.WaitAll(result);

            Console.WriteLine(result.Result);

            var result2 = client.ListPoliciesAsync(new ListPoliciesRequest() { PolicyInstance = new PolicyInstance(){
                Name="todo",
                InstanceLabel="todo"
            }
        });
            
            Task.WaitAll(result2);
            Console.WriteLine("List modules:");
            foreach (var policy in result2.Result)
            {
                Console.WriteLine(policy.Raw);
            }
        }
    }
}
