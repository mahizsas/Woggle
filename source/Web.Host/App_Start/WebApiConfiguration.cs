using System.Linq;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Web.Host.Controllers.Api;

namespace Web.Host
{
    public class WebApiConfiguration
    {
        private static ContainerBuilder _builder;
        private static IContainer _container;

        public static void Configure(HttpConfiguration config, ContainerBuilder builder = null)
        {
            _builder = builder ?? new ContainerBuilder();

            // Web API routes
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute("DefaultApi", "{controller}/{id}", new { id = RouteParameter.Optional });
            // We want to use OWIN middleware auth
            //config.SuppressDefaultHostAuthentication(); // Causes errors in ut

            ConfigureFormatters(config);
            ConfigureAutofac(config, _builder);

            ////var userManager = _container.Resolve<IApplicationUserManager>();
            ////config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
            ////config.Filters.Add(new BasicAuthenticationFilter("web.local", userManager.ValidateBasiccredentials));

            // Ensure the config is initialised or we end up in bother.
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
            // Register and build the container
            _builder = containerBuilder;
            _builder.RegisterApiControllers(typeof(AccountController).Assembly);
            _builder.RegisterHttpRequestMessage(config);
            _container = _builder.Build();

            // Set up resolvers
            var resolver = new AutofacWebApiDependencyResolver(_container);
            config.DependencyResolver = resolver;
            GlobalConfiguration.Configuration.DependencyResolver = resolver;
        }
    }
}