using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using Data.Infrastructure;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security;

namespace Web.Host.Controllers.Api
{
    public class AuthenticateRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AuthenticateResponse
    {
        public string AccessToken { get; set; }
        public string Expires { get; set; }
    }

    public class AccountController : ApiController
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [Route("authenticate")]
        [HttpPost]
        [ActionName("Authenticate")]
        [AllowAnonymous]
        public object Authenticate([FromBody]AuthenticateRequest request)
        {
            var user = request.Username;
            var password = request.Password;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
                return Unauthorized();
            var userIdentity = UserManager.FindAsync(user, password).Result;
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

                string AccessToken = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket);
                return Ok(new AuthenticateResponse()
                {
                    AccessToken = AccessToken,
                    Expires = expiresUtc.ToString()
                });
            }
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
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

        [Authorize]
        [HttpGet]
        [ActionName("GetPrivateData")]
        public object GetPrivateData()
        {
            return new { Message = "Secret information" };
        }
    }
}