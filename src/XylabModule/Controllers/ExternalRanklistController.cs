using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Xylab.BricksService.OjUpdate;

namespace SatelliteSite.XylabModule.Controllers
{
    [Area("Xylab")]
    [Authorize(Policy = "ExternalRanklistReader")]
    public class ExternalRanklistController : ViewControllerBase
    {
        [HttpGet("/ranklist/{name}/{year?}")]
        public async Task<IActionResult> List(
            [FromRoute] string name,
            [FromRoute] int? year,
            [FromServices] IRecordStorage store,
            [FromServices] IUpdateProvider provider)
        {
            if (!provider.TryGetOrchestrator(name, out var oj))
                return NotFound();
            var title = name + " Ranklist";
            if (year.HasValue) title += " " + year;
            ViewData["Title"] = title;

            var stat = await oj.GetStatus();
            var ojac = await store.ListAsync(oj.Driver.Category, year);
            ojac.Sort();
            return View(new RanklistViewModel
            {
                OjName = name,
                LastUpdate = stat.LastUpdate ?? DateTimeOffset.UnixEpoch,
                IsUpdating = stat.IsUpdating,
                RankTemplate = oj.Driver.RankTemplate,
                AccountTemplate = oj.Driver.AccountTemplate,
                CountColumn = oj.Driver.ColumnName,
                OjAccounts = ojac,
            });
        }


        [HttpPost("/ranklist/{oj}/[action]")]
        [ValidateAntiForgeryToken]
        [AuditPoint(AuditlogType.Scoreboard)]
        public async Task<IActionResult> Refresh(
            [FromRoute] string oj,
            [FromServices] IUpdateProvider provider)
        {
            if (provider.TryGetOrchestrator(oj ?? string.Empty, out var ojs)
                && !(await ojs.GetStatus()).IsUpdating)
            {
                await HttpContext.AuditAsync("requested refresh", "external", oj);
                await ojs.RequestUpdate();
                StatusMessage = "Ranklist will be refreshed in minutes... Please refresh this page a minute later.";
                return RedirectToAction(nameof(List), new { name = oj });
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
