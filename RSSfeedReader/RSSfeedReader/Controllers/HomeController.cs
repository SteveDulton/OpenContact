using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using System.Web;
using System.Web.Mvc;



namespace RSSfeedReader.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}