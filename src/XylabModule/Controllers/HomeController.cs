using Microsoft.AspNetCore.Mvc;
using SatelliteSite.NewsModule.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xylab.Contesting;
using Xylab.Contesting.Services;

namespace SatelliteSite.XylabModule.Controllers
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

        public static IReadOnlyList<string> PhotoList { get; }
            = new[] { "2018qingdao", "2018xian", "2018final" };


        [HttpGet]
        [HttpGet("/")]
        public async Task<IActionResult> Index([FromServices] INewsStore store)
        {
            ViewData["ActiveAction"] = "HomePage";
            ViewData["Photo"] = PhotoList[DateTimeOffset.Now.Millisecond % PhotoList.Count];
            return View(await store.ListActiveAsync(10));
        }


        [HttpGet]
        public IActionResult About()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Problemsets([FromServices] IContestRepository2 store, int page = 1)
        {
            ViewData["ActiveAction"] = "Problemset";

            return View(
                await store.ListAsync(User, CcsDefaults.KindProblemset, page));
        }


        [HttpGet]
        public async Task<IActionResult> Contests([FromServices] IContestRepository2 store, int page = 1)
        {
            ViewData["ActiveAction"] = "ListContest";

            return View(
                await store.ListAsync(User, CcsDefaults.KindDom, page));
        }


        [HttpGet]
        public async Task<IActionResult> Gyms([FromServices] IContestRepository2 store, int page = 1)
        {
            ViewData["ActiveAction"] = "ListGym";

            return View(
                await store.ListAsync(User, CcsDefaults.KindGym, page));
        }


        [HttpGet("/api/smoketest")]
        public IActionResult SmokeTest()
        {
            return Json(new
            {
                version = ProgramVersion,
                time = DateTimeOffset.Now,
                instance = Environment.MachineName,
            });
        }
    }
}
