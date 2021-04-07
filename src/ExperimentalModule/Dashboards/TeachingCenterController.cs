using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polygon.Storages;
using System;
using System.Threading.Tasks;
using Tenant.Services;

namespace SatelliteSite.ExperimentalModule.Dashboards
{
    [Area("Dashboard")]
    [Authorize(Policy = "TenantAdmin")]
    [Route("/[area]/[controller]")]
    public class TeachingCenterController : StudentModule.Dashboards.TenantControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Index(
            [FromServices] IContestRepository2 contests,
            [FromServices] IProblemStore problems,
            [FromServices] IStudentStore students)
        {
            var uid = int.Parse(User.GetUserId());

            ViewData["Contests"] = await contests.ListAsync(User, limit: 5);
            ViewData["Problems"] = await problems.ListAsync(1, 5, User);
            ViewData["Classes"] = await students.ListClassesAsync(Affiliation, 1, 5, uid);
            ViewData["ActiveAction"] = "Teacher";
            return View();
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Organization(
            [FromServices] IStudentStore store)
        {
            ViewBag.Administrators = await store.GetAdministratorsAsync(Affiliation);
            ViewBag.UserRoles = await store.GetAdministratorRolesAsync(Affiliation);
            return View(Affiliation);
        }
    }
}
