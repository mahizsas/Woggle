using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Data.Infrastructure;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Web.Host.Controllers.Api
{
    public class AuthUserSession
    {
        public string UserName { get; set; }
        
        public string DisplayName { get; set; }

        public string EmailAddress { get; set; }
    }

    public class UserController : ApiController
    {
        private ApplicationUserManager _userManager;

        public UserController()
        {
        }

        public UserController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        
        [Route("api/user")]
        public HttpResponseMessage Current()
        {
            var userDb = UserManager.FindById(User.Identity.GetUserId());

            var user = new AuthUserSession
                                {
                                    DisplayName = userDb.DisplayName,
                                    EmailAddress = userDb.Email
                                };

            return Request.CreateResponse(HttpStatusCode.OK, user);
        }

    }
}