using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity;

namespace Web.Host.Controllers
{
    //    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
                return View();

            return RedirectToAction("Dashboard", "User", new { username = User.Identity.GetUserName() });
        }
    }
}
