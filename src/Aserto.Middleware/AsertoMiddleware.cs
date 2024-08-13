using Aserto.Authorizer.V2;
using Aserto.Authorizer.V2.Api;
using Aserto.Clients.Authorizer;
using Aserto.Middleware.Options;
using Microsoft.Extensions.Options;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Aserto.Middleware
{
    public class AsertoMiddleware : OwinMiddleware
    {
        private readonly OwinMiddleware next;
        private readonly ILogger logger;
        private AsertoOptions options;
        private IAuthorizerAPIClient client;

        public IEnumerable<string> DefaultClaimTypes { get; private set; } = new List<string>() { ClaimTypes.Email, ClaimTypes.Name, ClaimTypes.NameIdentifier };

        public AsertoMiddleware(OwinMiddleware next, ILoggerFactory loggerFactory, AsertoOptions options, IAuthorizerAPIClient client) : base(next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.logger = loggerFactory.Create("AsertoMiddleware");

            this.options = options ?? throw new ArgumentNullException(nameof(options));

            this.client = client;
        }

        public override async Task Invoke(IOwinContext context)
        {

            var request = new Authorizer.V2.IsRequest();

            request.PolicyContext = new PolicyContext();
            request.PolicyContext.Path = this.options.PolicyPathMapper(this.options.PolicyRoot, context.Request);
            request.PolicyContext.Decisions.Add(options.Decision);

            request.ResourceContext = this.options.ResourceMapper(this.options.PolicyRoot, context.Request);

            request.IdentityContext = options.IdentityMapper(context.Authentication.User, DefaultClaimTypes);
            
            request.PolicyInstance = new PolicyInstance();
            request.PolicyInstance.Name = options.PolicyName;
            request.PolicyInstance.InstanceLabel = options.PolicyInstanceLabel;
                        

            var allowed = await this.client.IsAsync(request);
            if (!allowed && this.options.Enabled)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                var errorMessage = Encoding.UTF8.GetBytes(HttpStatusCode.Forbidden.ToString());
                await context.Response.Body.WriteAsync(errorMessage, 0, errorMessage.Length);
            }
            else
            {
                this.logger.WriteInformation($"Decision to allow: {context.Request.Path} was: {allowed}");
                await this.next.Invoke(context);
            }
        }
    }
}
