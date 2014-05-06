using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Web.Http;
using Data.Infrastructure;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security;

namespace Web.Host.Controllers.Api
{
    public class AuthenticateRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class AuthenticateResponse
    {
        public string access_token { get; set; }
        public string Expires { get; set; }

        public HttpStatusCode Status { get; set; }
    }

    [Authorize]
    public class AccountController : ApiController
    {
        private readonly ISecureDataFormat<AuthenticationTicket> _accessTokenFormat;
        public IApplicationUserManager UserManager { get; set; }

        public AccountController()
        {
            GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IApplicationUserManager));
            _accessTokenFormat = Startup.OAuthOptions.AccessTokenFormat;
        }

        public AccountController(IApplicationUserManager userManager, ISecureDataFormat<AuthenticationTicket> accessTokenFormtFormat = null)
        {
            UserManager = userManager;
            _accessTokenFormat = accessTokenFormtFormat ?? Startup.OAuthOptions.AccessTokenFormat;
        }

        [HttpPost]
        [Route("api/auth/request_token")]
        [AllowAnonymous]
        public AuthenticateResponse Authenticate([FromBody]AuthenticateRequest request)
        {
            var user = request.Username;
            var password = request.Password;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
                return new AuthenticateResponse { Status = HttpStatusCode.BadRequest };
            var findAsync = UserManager.FindAsync(user, password);
            findAsync.Wait();

            var userIdentity = findAsync.Result;
            if (userIdentity != null)
            {
                var identity = new ClaimsIdentity(Startup.OAuthOptions.AuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.Name, user));
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userIdentity.Id));

                AuthenticationTicket ticket = new AuthenticationTicket(identity, new AuthenticationProperties());

                var currentUtc = new SystemClock().UtcNow;
                ticket.Properties.IssuedUtc = currentUtc;
                DateTimeOffset expiresUtc = currentUtc.Add(TimeSpan.FromMinutes(30));
                ticket.Properties.ExpiresUtc = expiresUtc;

                string accessToken = _accessTokenFormat.Protect(ticket);
                return new AuthenticateResponse()
                {
                    access_token = accessToken,
                    Expires = expiresUtc.ToString()
                };
            }
            return new AuthenticateResponse { Status = HttpStatusCode.Unauthorized };
        }

        [Authorize]
        [HttpGet]
        [ActionName("ValidateToken")]
        public String ValidateToken()
        {
            var user = this.User.Identity;
            if (user != null)
                return string.Format("{0} - {1}", user.GetUserId(), user.GetUserName());
            else
                return "Unable to resolve user id";

        }
    }
}