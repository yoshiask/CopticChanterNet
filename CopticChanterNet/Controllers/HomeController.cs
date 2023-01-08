using CopticChanterNet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CopticChanterNet.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult DocSet()
        {
            var setId = Request.RouteValues["id"]?.ToString();
            if (string.IsNullOrEmpty(setId))
                throw new System.ArgumentException("A document set ID must be specified.");

            DocSetViewModel vm = new(setId!, _env);

            return View(vm);
        }

        public IActionResult TasbehaOrgGenerator()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel(HttpContext)
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            });
        }
    }
}