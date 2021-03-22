using Microsoft.AspNetCore.Mvc;
using SatelliteSite.NewsModule.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SatelliteSite.XylabModule.Controllers
{
    [Area("Xylab")]
    [SupportStatusCodePage]
    [Route("/[action]")]
    public class HomeController : ViewControllerBase
    {
        public static string ProgramVersion { get; }
            = typeof(GitVersionAttribute).Assembly
                .GetCustomAttribute<GitVersionAttribute>()?
                .Version?.Substring(0, 7) ?? "unknown";

        public static IReadOnlyList<string> PhotoList { get; }
            = new[] { "2018qingdao", "2018xian", "2018final" };


        [HttpGet]
        [HttpGet("/")]
        public async Task<IActionResult> Index([FromServices] INewsStore store)
        {
            ViewData["Photo"] = PhotoList[DateTimeOffset.Now.Millisecond % PhotoList.Count];
            return View(await store.ListActiveAsync(10));
        }


        [HttpGet]
        public IActionResult About()
        {
            return View();
        }
    }
}
