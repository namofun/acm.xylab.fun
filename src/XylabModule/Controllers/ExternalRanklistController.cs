﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.XylabModule.Services;
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
            [FromServices] IRecordStorage store)
        {
            if (!OjUpdateService.OjList.TryGetValue(name, out var oj))
                return NotFound();
            var title = name + " Ranklist";
            if (year.HasValue) title += " " + year;
            ViewData["Title"] = title;

            var ojac = await store.ListAsync(oj.Driver.Category, year);
            ojac.Sort();
            return View(new RanklistViewModel
            {
                OjName = name,
                LastUpdate = oj.LastUpdate ?? DateTimeOffset.UnixEpoch,
                IsUpdating = oj.IsUpdating,
                RankTemplate = oj.Driver.RankTemplate,
                AccountTemplate = oj.Driver.AccountTemplate,
                CountColumn = oj.Driver.ColumnName,
                OjAccounts = ojac,
            });
        }


        [HttpPost("/ranklist/{oj}/[action]")]
        [ValidateAntiForgeryToken]
        [AuditPoint(AuditlogType.Scoreboard)]
        public async Task<IActionResult> Refresh(string oj)
        {
            if (OjUpdateService.OjList.TryGetValue(oj ?? string.Empty, out var ojs) && !ojs.IsUpdating)
            {
                await HttpContext.AuditAsync("requested refresh", "external", oj);
                ojs.RequestUpdate();
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
