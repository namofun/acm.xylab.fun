using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polygon.Storages;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tenant.Services;

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
        [Authorize(Policy = "TenantAdmin")]
        public async Task<IActionResult> Teach(
            [FromServices] IContestRepository2 contests,
            [FromServices] IProblemStore problems,
            [FromServices] IStudentStore students,
            [FromServices] IAffiliationStore affiliations)
        {
            var uid = int.Parse(User.GetUserId());
            var tenantId = User.IsInRole("Administrator")
                ? affiliations.GetQueryable().Select(a => a.Id)
                : User.FindAll("tenant_admin").Select(a => int.Parse(a.Value));

            ViewData["Contests"] = await contests.ListAsync(User, limit: 5);
            ViewData["Problems"] = await problems.ListAsync(1, 5, User);
            ViewData["Classes"] = await students.ListClassesAsync(tenantId, 1, 5, c => c.UserId == null || c.UserId == uid);
            ViewData["ActiveAction"] = "Teacher";
            return View();
        }


        [HttpGet]
        public IActionResult Events()
        {
            return Json(new
            {
                success = 1,
                result = new[]
                {
                    new { id = "293", title = "This is warning class event with very long title to check how it fits to evet in day view", url = "http://www.example.com/", @class = "event-warning", start = "1362938400000", end = "1363197686300" },
                    new { id = "256", title = "Event that ends on timeline", url = "http://www.example.com/", @class = "event-warning", start = "1363155300000", end = "1363227600000" },
                    new { id = "276", title = "Short day event", url = "http://www.example.com/", @class = "event-success", start = "1363245600000", end = "1363252200000" },
                    new { id = "294", title = "This is information class ", url = "http://www.example.com/", @class = "event-info", start = "1363111200000", end = "1363284086400" },
                    new { id = "297", title = "This is success event", url = "http://www.example.com/", @class = "event-success", start = "1363234500000", end = "1363284062400" },
                    new { id = "54", title = "This is simple event", url = "http://www.example.com/", @class = "", start = "1363712400000", end = "1363716086400" },
                    new { id = "532", title = "This is inverse event", url = "http://www.example.com/", @class = "event-inverse", start = "1364407200000", end = "1364493686400" },
                    new { id = "548", title = "This is special event", url = "http://www.example.com/", @class = "event-special", start = "1363197600000", end = "1363629686400" },
                    new { id = "295", title = "Event 3", url = "http://www.example.com/", @class = "event-important", start = "1364320800000", end = "1364407286400" }
                }
            });
        }
    }
}
