using System;
using System.ComponentModel;
using System.Reflection;
using System.Web.Http;
using Aserto.Clients.Authorizer;
using Aserto.Middleware.Options;
using Ninject;
using Owin;
using WebApi.Support;

namespace WebApi
{
    public class WebApiConfig
    {
        private static HttpConfiguration config = new HttpConfiguration();

        public static void Configure(IAppBuilder app)
        {                      

            // Web API routes
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new {id = RouteParameter.Optional});

            // Example configuration using the aserto authorization filter from the aserto middleware extensions 
            //config.Filters.Add(new Aserto.Middleware.Extensions.AsertoAuthorizationFilter(AuthorizerClientHelper.GetAsertoOptions(), AuthorizerClientHelper.GetClient() ));

            app.UseNinject(CreateKernel);

            app.UseWebApi(config);
        }

        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<IAuthorizerAPIClient>().ToMethod(context => { return AuthorizerClientHelper.GetClient(); });            
            kernel.Bind<AsertoOptions>().ToMethod(context => { return AuthorizerClientHelper.GetAsertoOptions(); });

            kernel.Load(Assembly.GetExecutingAssembly());
            config.DependencyResolver = new NinjectResolver(kernel);

            return kernel;
        }
    }
}