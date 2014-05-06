using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Data.Infrastructure;
using Data.Models;
using Microsoft.Owin.Security;
using Moq;
using Newtonsoft.Json;
using Owin;
using Should;
using Web.Host;
using Web.Host.Controllers.Api;
using Xunit;

namespace Tests.Acceptance.Examples
{
    public class ApiGeneralTestExample
    {
        [Fact]
        public void ShouldReturnNotFoundWhenNoRouteControllerMatch()
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute("DefaultApi", "{controller}/{id}", new { id = RouteParameter.Optional });

            var server = new HttpServer(config);
            var client = new HttpClient(server);

            var task = client.GetAsync("http://test.com//issues");
            task.Wait();

            var response = task.Result;

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}