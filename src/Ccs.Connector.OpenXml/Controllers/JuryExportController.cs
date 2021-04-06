using Ccs.Models;
using Ccs.Scoreboard;
using Ccs.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule.Controllers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Connector.OpenXml.Controllers
{
    [Area("Contest")]
    [Authorize(Policy = "ContestIsJury")]
    [Route("[area]/{cid:c(3)}/jury/export")]
    public class JuryExportController : JuryControllerBase<IContestContext>
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Scoreboard(
            [FromQuery] int[] affiliations = null,
            [FromQuery] int[] categories = null)
        {
            if (Contest.RankingStrategy == CcsDefaults.RuleCodeforces) return StatusCode(501);
            if (affiliations != null && affiliations.Length == 0) affiliations = null;
            if (categories != null && categories.Length == 0) categories = null;

            var workbook = new XLWorkbook(XLEventTracking.Disabled);

            if (!Contest.StartTime.HasValue) return BadRequest();
            var scb = await Context.GetScoreboardAsync();
            var affs = await Context.ListAffiliationsAsync();
            var orgs = await Context.ListCategoriesAsync();
            var probs = await Context.ListProblemsAsync();

            var board = new FullBoardViewModel
            {
                UpdateTime = scb.RefreshTime,
                Problems = probs,
                IsPublic = false,
                Categories = orgs,
                ContestId = Contest.Id,
                RankingStrategy = Contest.RankingStrategy,
                Affiliations = affs,
                RankCache = scb.Data.Values
                    .WhereIf(affiliations != null, r => affiliations.Contains(r.AffiliationId))
                    .WhereIf(categories != null, r => categories.Contains(r.CategoryId)),
            };

            foreach (var sortOrder in board)
            {
                var teams = sortOrder.ToList();
                var cats = string.Join(", ", teams.Select(t => t.Category).Distinct().Where(a => !string.IsNullOrWhiteSpace(a)));
                var worksheet = workbook.Worksheets.Add(cats);

                worksheet.Range(1, 1, 1, 4 + probs.Count).Merge().Value(Contest.Name).Bold().Align();
                worksheet.Range(2, 1, 2, 4 + probs.Count).Merge().Value($"导出时间：{DateTimeOffset.Now}").Align(XLAlignmentHorizontalValues.Right);

                worksheet.Row(1).Height *= 3;
                worksheet.Row(1).Style.Font.FontSize *= 1.5;
                worksheet.Row(2).Height *= 1.5;
                worksheet.Row(3).Height *= 1.5;
                worksheet.Column(2).Width *= 2;

                worksheet.Cell(3, 1).Value("RANK").Bold().Align();
                worksheet.Cell(3, 2).Value("TEAM").Bold().Align();
                worksheet.Range(3, 3, 3, 4).Merge().Value("SCORE").Bold().Align();
                for (int i = 0; i < probs.Count; i++) worksheet.Cell(3, 5 + i).Value(probs[i].ShortName).Bold().Align();

                int rowid = 4;
                foreach (var team in teams)
                {
                    var row = worksheet.Row(rowid++);
                    row.Height *= 1.5;

                    row.Cell(1).Value(team.Rank).Align();
                    row.Cell(2).Value(team.TeamName).Bold().Bgcolor(XLColor.FromHtml(team.CategoryColor)).Align(XLAlignmentHorizontalValues.Right);
                    row.Cell(3).Value(team.Points).Bold().Align();
                    row.Cell(4).Value(team.Penalty).Align();

                    for (int i = 0; i < team.Problems.Length; i++)
                    {
                        if (Contest.RankingStrategy == CcsDefaults.RuleXCPC)
                        {
                            if (team.Problems[i] == null) continue;
                            var prob = team.Problems[i];
                            if (!prob.Score.HasValue && prob.JudgedCount == 0) continue;

                            row.Cell(5 + i).Align()
                                .Value(!prob.Score.HasValue
                                    ? $"(-{prob.JudgedCount})"
                                    : $"{prob.Score} (+{prob.JudgedCount})");
                        }
                        else if (Contest.RankingStrategy == CcsDefaults.RuleIOI)
                        {
                            if (!(team.Problems[i]?.Score is int sc)) continue;
                            row.Cell(5 + i).Align().Value(sc);
                        }
                    }
                }
            }

            // sentinel worksheet
            workbook.AddWorksheet("Sheet0");

            var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            memoryStream.Position = 0;

            return File(
                fileStream: memoryStream,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: $"c{Contest.Id}-scoreboard.xlsx");
        }
    }
}
