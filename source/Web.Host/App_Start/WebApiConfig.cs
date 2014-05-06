using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Common.Auth.Basic;
using Data.Infrastructure;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using Web.Host.Controllers.Api;

namespace Web.Host
{
    public static class WebApiConfig
    {
        private static ContainerBuilder _builder;

        private static async Task<ClaimsPrincipal> Validator(string username, string password)
        {
            var manager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = await manager.FindAsync(username, password);

            if (user == null) return null;

            var identiy = await manager.CreateIdentityAsync(user, "Basic");
            return new ClaimsPrincipal(identiy);
        }

        public static void Register(HttpConfiguration config, ContainerBuilder containerBuilder = null)
        {
            _builder = containerBuilder ?? new ContainerBuilder();

            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
            config.Filters.Add(new BasicAuthenticationFilter("web.local", Validator));

            // Web API configuration and services
            ConfigureFormatters(config);
            ConfigureAutofac(config, _builder);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.EnsureInitialized();
        }

        public static void ConfigureFormatters(HttpConfiguration config)
        {
            config.EnableQuerySupport();

            // set up Json formatters.
            JsonSerializerSettings settings = config.Formatters.JsonFormatter.SerializerSettings;
            settings.NullValueHandling = NullValueHandling.Include;
            settings.Formatting = Formatting.Indented;
            settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

           // Remove the XML formatter. This might be a bad think as we may need XML 
            // TODO: Find  a better way to default to JSON
            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);
        }

        public static void ConfigureAutofac(HttpConfiguration config, ContainerBuilder containerBuilder)
        {
            _builder = containerBuilder;
            _builder.RegisterApiControllers(typeof(AccountController).Assembly);

           // _builder.RegisterWebApiFilterProvider(config);
            _builder.RegisterHttpRequestMessage(config);

           _builder.RegisterType<MemoryCacheClient>().As<ICacheClient>().SingleInstance();
           
            var container = _builder.Build();
            var resolver = new AutofacWebApiDependencyResolver(container);
            config.DependencyResolver = resolver;
            GlobalConfiguration.Configuration.DependencyResolver = resolver;
            
            //DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;
            ////config.Services.Add((typeof(ModelValidatorProvider)), new WebApiModelValidatorProvider(container.Resolve<IValidatorFactory>()));
        }

    }
}
