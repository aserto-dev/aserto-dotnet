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
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;

namespace Aserto.Middleware.Extensions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class Aserto : AuthorizeAttribute
    {
        public AsertoOptions Options { get; set; }
        public IAuthorizerAPIClient Client { get; set; } 

        public IEnumerable<string> DefaultClaimTypes { get; set; } = new List<string>() { ClaimTypes.Email, ClaimTypes.Name, ClaimTypes.NameIdentifier };

        public Aserto()
        {   
            
        }

        protected override bool IsAuthorized(HttpActionContext context)
        {
            var scope = context.Request.GetDependencyScope();
            this.Client = scope.GetService(typeof(IAuthorizerAPIClient)) as IAuthorizerAPIClient; 

            this.Options = scope.GetService(typeof(AsertoOptions)) as AsertoOptions;

            if (!this.Options.Enabled)
            {
                return true;
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


            return Task.Run(async () => await this.Client.IsAsync(request)).GetAwaiter().GetResult();
        }
    }
}
