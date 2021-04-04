using Microsoft.AspNetCore.Mvc;
using System;
using System.Reflection;

namespace SatelliteSite.OnPremiseModule.Controllers
{
    [Area("Xylab")]
    [SupportStatusCodePage]
    [Route("/[action]")]
    public class HomeController : ViewControllerBase
    {
        public static string ProgramVersion { get; }
            = typeof(HomeController).Assembly
                .GetCustomAttribute<GitVersionAttribute>()?
                .Version?.Substring(0, 7) ?? "unknown";


        [HttpGet]
        [HttpGet("/")]
        public IActionResult Index()
        {
            ViewData["ActiveAction"] = "HomePage";
            return View();
        }


        [HttpGet]
        public IActionResult Teach()
        {
            ViewData["ActiveAction"] = "Teacher";
            return View();
        }
    }
}
