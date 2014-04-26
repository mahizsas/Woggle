using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace Web.Host.Controllers
{

    [RoutePrefix("user")]
    public class UserController : Controller
    {
        [Authorize]
        [Route("{userName}")]
        public ActionResult Dashboard(string userName)
        {
            var token =
                "#access_token=o0i7gwhciCaQzTYcDrvVwDq_vrlYkfesYS2XqrhSuuuwLCwfQw_W3-aoLwso3ibZDMJbvR6lSxLyatoFhw9DpSpDDngISD79nqTiHC4LZnEwkcJ4GfnLJjGHr_3UtH1JRC2GayPaUfhob8OQTOQsLNjURnjZCb4gZTs4OMMrvc_BnY8sd4QIFn5QfOXN_MSPSeT-Mbo1qg1lK92OAN_5HiUZ7Wfwsj6LiTXhOZgIIFw8MU-k8-jPIt-T_nx-xItJbbJsP2O9Y1kXcwI2R9VyWEgZtS5j55-Q-FSJAeZRgC2SlvWK92e0jeQS-NJqE_KtdmuQJsYJE4wTQfd5UgyVMrR6dDrh1H7ZwbvWhZAj5-zSYDINTbeJo9t1a-n_-aKTsaGLN_a3fTZEDV6ViIzpAMwreMIKgDwD_yj3uonnLi6vykUpfbI_VvbKmJlrG-a_6XI9ZbR084is_pCCn8qw0IDauwZsGEzgZEc5A6BRs95mKCWYQZ3BBhIjTM3GtfsN&token_type=bearer&expires_in=1209600";

            if (User.Identity.GetUserName() != userName) // if the logged in user is not the requested bounce to the public profile
                return RedirectToAction("Profile", new { userName });

            return View();
        }

        [Route("public/{userName}")]
        public ActionResult Profile(string userName)
        {
            return View();
        }
    }
}