using Aserto.AspNetCore.Middleware.Extensions;
using Aserto.AspNetCore.Middleware.Policies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Aserto.NETCore3.Auth0
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
            string domain = $"https://{Configuration["Auth0:Domain"]}/";

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.Authority = domain;
                options.Audience = Configuration["Auth0:Audience"];
            });

            //Aserto options handling
            services.AddAsertoAuthorization(Configuration.GetSection("Aserto"));
            //end Aserto options handling

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Aserto", policy => policy.Requirements.Add(new AsertoDecisionRequirement()));
            });

            // Only authorizes the endpoints that have the [Authorize("Aserto")] attribute
            services.AddControllers();

            //// Authorizes all the endpoints. The below code is equivalent to decorating all endpoints with [Authorize("Aserto")] attribute
            //services.AddControllers().AddMvcOptions(options =>
            //{
            //    options.Filters.Add(new AuthorizeFilter("Aserto"));
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //// Authorizes all the endpoints. The below code is equivalent to decorating all endpoints with [Authorize("Aserto")] attribute
                //endpoints.MapControllers().RequireAuthorization("Aserto");
            });
        }
    }
}
