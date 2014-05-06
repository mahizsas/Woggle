using System.Data.Entity;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Data.EntityFramework;
using Data.Infrastructure;
using Data.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Web.Host
{
    public class MvcApplication : HttpApplication
    {
        private readonly ContainerBuilder _builder = new ContainerBuilder();

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RegisterDependancies(_builder);

            WebApiConfiguration.Configure(GlobalConfiguration.Configuration, _builder);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        public void RegisterDependancies(ContainerBuilder builder)
        {
            builder.RegisterInstance(new WoggleDbContext()).As<DbContext>();
            builder.RegisterInstance(new WoggleDbContext()).As<WoggleDbContext>();
            builder.RegisterType<UserStore<ApplicationUser>>().As<IUserStore<ApplicationUser>>().PropertiesAutowired();
            builder.RegisterType<ApplicationUserManager>().As<IApplicationUserManager>().PropertiesAutowired();
        }
    }
}
