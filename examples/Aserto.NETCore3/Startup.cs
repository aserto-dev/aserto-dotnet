using Aserto.AspNetCore.Middleware.Extensions;
using Aserto.AspNetCore.Middleware.Options;
using Aserto.Authorizer.V2.API;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aserto.NETCore3
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var checkResourceRules = new Dictionary<string, Func<string, HttpRequest, Struct>>();

            checkResourceRules.Add("admin", (policyRoot, httpRequest) =>
            {
                Struct result = new Struct();
                result.Fields.Add("object_id", Value.ForString("admin"));
                result.Fields.Add("object_type", Value.ForString("group"));
                result.Fields.Add("relation", Value.ForString("member"));
                return result;
            });

            var options = new CheckOptions();
            // Bind base aserto option configurations
            Configuration.GetSection("Aserto").Bind(options.BaseOptions);
            options.BaseOptions.PolicyPathMapper = (policyRoot, httpRequest) =>
            {
                return "rebac.check"; // hardcoded policy path for aserto policy-rebac example
            };

            // hardcoded identity mapper example using a citadel aserto user
            options.BaseOptions.IdentityMapper = (policyRoot, httpRequest) =>
            {
                var result = new IdentityContext()
                {
                    Type = IdentityType.Sub,
                    Identity = "rick@the-citadel.com",
                };
                return result;
            };
            options.ResourceMappingRules = checkResourceRules;

            //Aserto options handling
            services.AddAsertoCheckAuthorization(options,
            authorizerConfig =>
            {
                Configuration.GetSection("Aserto").Bind(authorizerConfig);
            });
           
            //end Aserto options handling

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            //Add the Aserto middleware
            app.UseAsertoCheckAuthorization();
            //end add Aserto middleware

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

    }
}
