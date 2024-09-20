using Ninject.Activation;
using Ninject.Syntax;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ninject.Parameters;
using Aserto.Clients.Authorizer;
using Aserto.Middleware.Options;


namespace MvCApp.Support
{    public class NinjectResolver : IDependencyResolver
    {
        private IKernel _kernel;

        public NinjectResolver()
        {
            _kernel = new StandardKernel();
            RegisterServices(_kernel);
        }

        public NinjectResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        public object GetService(Type serviceType)
        {
            return _kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _kernel.GetAll(serviceType);
        }

        public IBindingToSyntax<T> Bind<T>()
        {
            return _kernel.Bind<T>();
        }

        public static void RegisterServices(IKernel kernel)
        {
            //Add your bindings here. 
            //This is static as you can use it for WebApi by passing it the IKernel
            kernel.Bind<IAuthorizerAPIClient>().ToMethod(context => { return AuthorizerClientHelper.GetClient(); });
            kernel.Bind<AsertoOptions>().ToMethod(context => { return AuthorizerClientHelper.GetAsertoOptions(); });

        }
    }
}