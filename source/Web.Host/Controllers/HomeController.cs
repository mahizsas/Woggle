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

            return RedirectToAction("Home", "DashBoard", new { username = User.Identity.GetUserName() });
        }
    }

    [Authorize]
    [RoutePrefix("")]
    public class DashBoardController : Controller
    {
        [Route("{username}")]
        public ActionResult Home()
        {
            return View();
        }
    }
}
