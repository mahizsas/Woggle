using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Data.Infrastructure;
using Data.Models;
using Microsoft.Owin.Security;
using Moq;
using Newtonsoft.Json;
using Should;
using Web.Host;
using Web.Host.Controllers.Api;
using Xunit;

namespace Tests.Unit.Auth.API
{
    public class ApiAccountControllerTests
    {
        [Fact]
        public async void ShouldHandlePostRequestForTokenWithValidCredentials()
        {
            HttpConfiguration config = new HttpConfiguration();
            ContainerBuilder builder = new ContainerBuilder();

            Mock<IApplicationUserManager> mockAuthManager = new Mock<IApplicationUserManager>();
            Mock<ISecureDataFormat<AuthenticationTicket>> mockDataSecurity =
                new Mock<ISecureDataFormat<AuthenticationTicket>>();
            mockAuthManager.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new ApplicationUser { UserName = "Fred1234", Email = "andrew@mail.com" }));
            mockDataSecurity.Setup(x => x.Protect(It.IsAny<AuthenticationTicket>())).Returns(
                "SomeRandomBigStrigForValidationnPurposes");

            builder.RegisterInstance(mockAuthManager.Object);
            builder.RegisterInstance(mockDataSecurity.Object);

            WebApiConfiguration.Configure(config, builder);

            HttpServer server = new HttpServer(config);
            HttpClient client = new HttpClient(server);

            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post,
                new Uri("http://test.com/api/auth/request_token"))
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                                    {
                                        {"username", "Fred"},
                                        {"password", "password1234"}
                                    })
            };
            HttpResponseMessage response = await client.SendAsync(message);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<AuthenticateResponse>(content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            result.ShouldNotBeNull();
            result.access_token.ShouldEqual("SomeRandomBigStrigForValidationnPurposes");
            mockAuthManager.Verify(x=>x.FindAsync(It.IsAny<string>(), It.IsAny<string>()));
            mockDataSecurity.Verify(x=>x.Protect(It.IsAny<AuthenticationTicket>()));
        }
        
        [Fact]
        public async void ShouldHandlePostRequestForTokenWithEmpytyCredentials()
        {
            HttpConfiguration config = new HttpConfiguration();
            ContainerBuilder builder = new ContainerBuilder();

            Mock<IApplicationUserManager> mockAuthManager = new Mock<IApplicationUserManager>();
            Mock<ISecureDataFormat<AuthenticationTicket>> mockDataSecurity =
                new Mock<ISecureDataFormat<AuthenticationTicket>>();
            mockAuthManager.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult((ApplicationUser)null));
            mockDataSecurity.Setup(x => x.Protect(It.IsAny<AuthenticationTicket>())).Returns(
                "SomeRandomBigStrigForValidationnPurposes");

            builder.RegisterInstance(mockAuthManager.Object);
            builder.RegisterInstance(mockDataSecurity.Object);

            WebApiConfiguration.Configure(config, builder);

            HttpServer server = new HttpServer(config);
            HttpClient client = new HttpClient(server);

            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post,
                new Uri("http://test.com/api/auth/request_token"))
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                                    {
                                        {"username", ""},
                                        {"password", ""}
                                    })
            };
            HttpResponseMessage response = await client.SendAsync(message);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            mockAuthManager.Verify(x => x.FindAsync(It.IsAny<string>(), It.IsAny<string>()));
            mockDataSecurity.Verify(x=>x.Protect(It.IsAny<AuthenticationTicket>()), Times.Never);
        }  
        
        [Fact]
        public async void ShouldHandlePostRequestForTokenWithUnknownCredentials()
        {
            HttpConfiguration config = new HttpConfiguration();
            ContainerBuilder builder = new ContainerBuilder();

            Mock<IApplicationUserManager> mockAuthManager = new Mock<IApplicationUserManager>();
            Mock<ISecureDataFormat<AuthenticationTicket>> mockDataSecurity =
                new Mock<ISecureDataFormat<AuthenticationTicket>>();
            mockAuthManager.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult((ApplicationUser)null));
            mockDataSecurity.Setup(x => x.Protect(It.IsAny<AuthenticationTicket>())).Returns(
                "SomeRandomBigStrigForValidationnPurposes");

            builder.RegisterInstance(mockAuthManager.Object);
            builder.RegisterInstance(mockDataSecurity.Object);

            WebApiConfiguration.Configure(config, builder);

            HttpServer server = new HttpServer(config);
            HttpClient client = new HttpClient(server);

            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post,
                new Uri("http://test.com/api/auth/request_token"))
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                                    {
                                        {"username", "ImNotAUser"},
                                        {"password", "ImNotAPassword"}
                                    })
            };
            HttpResponseMessage response = await client.SendAsync(message);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            mockDataSecurity.Verify(x=>x.Protect(It.IsAny<AuthenticationTicket>()), Times.Never);
        }
    }
}
