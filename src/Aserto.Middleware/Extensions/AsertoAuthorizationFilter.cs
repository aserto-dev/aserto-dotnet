using Aserto.Authorizer.V2.Api;
using Aserto.Clients.Authorizer;
using Aserto.Middleware.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Aserto.Middleware.Extensions
{
    public class AsertoAuthorizationFilter : AuthorizationFilterAttribute
    {
        public AsertoOptions Options { get; set; }
        public IAuthorizerAPIClient Client { get; set; }

        public IEnumerable<string> DefaultClaimTypes { get; set; } = new List<string>() { ClaimTypes.Email, ClaimTypes.Name, ClaimTypes.NameIdentifier };

        public AsertoAuthorizationFilter(AsertoOptions opts, IAuthorizerAPIClient client)
        {
            this.Client = client;
            this.Options = opts;
        }

        public override void OnAuthorization(HttpActionContext context)
        {
            if (!this.Options.Enabled)
            {
                base.OnAuthorization(context);
            }

            var request = new Authorizer.V2.IsRequest();

            request.PolicyContext = new PolicyContext();
            request.PolicyContext.Path = this.Options.PolicyPathMapper(this.Options.PolicyRoot, context.Request);
            request.PolicyContext.Decisions.Add(this.Options.Decision);

            request.ResourceContext = this.Options.ResourceMapper(this.Options.PolicyRoot, context.Request);

            var userDetails = (ClaimsPrincipal)context.RequestContext.Principal;

            request.IdentityContext = this.Options.IdentityMapper(userDetails, DefaultClaimTypes);

            request.PolicyInstance = new PolicyInstance();
            request.PolicyInstance.Name = this.Options.PolicyName;
            request.PolicyInstance.InstanceLabel = this.Options.PolicyInstanceLabel;


            var allowed = Task.Run(async () => await this.Client.IsAsync(request)).GetAwaiter().GetResult();
            if (!allowed)
            {
                context.Response = new System.Net.Http.HttpResponseMessage();
                context.Response.StatusCode = System.Net.HttpStatusCode.Unauthorized;                
                context.Response.Content = new StringContent(HttpStatusCode.Forbidden.ToString());
            }
            base.OnAuthorization(context);
        }
    }
}
