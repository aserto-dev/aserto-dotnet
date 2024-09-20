using System.Web;
using System.Web.Mvc;
using MvCApp.Support;
using Aserto.Middleware.Extensions;

namespace MvCApp
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AsertoMVCFilter(AuthorizerClientHelper.GetAsertoOptions(), AuthorizerClientHelper.GetClient()));
        }
    }
}
