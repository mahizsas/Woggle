using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Autofac;
using Data.Infrastructure;
using Data.Models;
using Microsoft.Owin.Security;
using Moq;
using Web.Host;
using Xunit;

namespace Tests.Acceptance.Auth
{
    public class AccountFeature
    {
        public HttpResponseMessage Response;
        public HttpRequestMessage Request { get; private set; }
        public HttpClient Client;

        public AccountFeature()
        {
            Request = new HttpRequestMessage();
            var config = new HttpConfiguration();
            var mockAuthManager = new Mock<IApplicationUserManager>();
            mockAuthManager.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new ApplicationUser());

            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(mockAuthManager.Object);

            WebApiConfiguration.Configure(config, builder);

            var server = new HttpServer(config);
            Client = new HttpClient(server);
        }

        [Fact]
        public void FactMethodName()
        {
            Request.Method = HttpMethod.Post;
            Request.RequestUri = new Uri("http://localhost/api/auth/request_token");
            
            var result = Client.SendAsync(Request).Result;
            var content = result.Content.ReadAsStringAsync().Result;


        }
    }
}