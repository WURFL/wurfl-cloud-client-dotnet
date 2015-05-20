using System;
using System.Web.Mvc;
using CloudDemo.Services.Home;

namespace CloudDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly WurflService _service;

        public HomeController()
        {
            _service = new WurflService();
        }

        [HttpGet]
        [ActionName("byrequest")]
        public ActionResult ByRequestFromGet()
        {
            return View();
        }

        [HttpPost]
        [ActionName("byrequest")]
        public ActionResult ByRequestFromPost()
        {
            var model = _service.GetDataByRequest(HttpContext);    
            return View(model);
        }

        [HttpGet]
        [ActionName("byagent")]
        public ActionResult ByAgentFromGet()
        {
            return View();
        }

        [HttpPost]
        [ActionName("byagent")]
        public ActionResult ByAgentFromPost(String userAgent)
        {
            var model = _service.GetDataByAgent(HttpContext, userAgent);
            return View(model);
        }
    }
}
