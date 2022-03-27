using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.OjUpdateModule.Entities;
using SatelliteSite.OjUpdateModule.Models;
using SatelliteSite.OjUpdateModule.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SatelliteSite.OjUpdateModule.Dashboards
{
    [Area("Dashboard")]
    [Authorize(Roles = "Administrator,TeamLeader")]
    [Route("[area]/[controller]")]
    [AuditPoint(AuditlogType.User)]
    public class ExternalRanklistController : ViewControllerBase
    {
        const int ItemsPerPage = 50;

        private ISolveRecordStore Store { get; }

        public ExternalRanklistController(ISolveRecordStore store)
        {
            Store = store;
        }


        [HttpGet]
        public async Task<IActionResult> List(int page = 1)
        {
            if (page <= 0) return BadRequest();
            var model = await Store.ListAsync(page, ItemsPerPage);
            return View(model);
        }


        [HttpGet("[action]/{category}")]
        public async Task<IActionResult> Cleanup(int category)
        {
            ViewBag.Category = category;
            var record = (RecordType)category;
            var model = await Store.ListAsync(record);
            
            if (model.Count == 0)
            {
                StatusMessage = "This category is empty.";
                return RedirectToAction(nameof(List));
            }

            return View(model);
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cleanup(CleanupModel model)
        {
            var category = (RecordType)model.Category;
            var toDelete = model.ToDelete ?? Array.Empty<int>();
            
            if (toDelete.Length == 0)
            {
                StatusMessage = "Error no items selected.";
            }
            else
            {
                var count = await Store.DeleteAsync(category, toDelete);
                StatusMessage = $"Successfully deleted {count} items.";
            }

            return RedirectToAction(nameof(List));
        }


        [HttpGet("[action]")]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> Create(
            [FromForm] SolveRecord[] Batch)
        {
            var results = new List<SolveRecord>();
            foreach (var item in Batch)
            {
                if (string.IsNullOrWhiteSpace(item.Account) ||
                    string.IsNullOrWhiteSpace(item.NickName))
                    continue;
                item.Id = 0;
                item.Result = null;
                results.Add(item);
            }

            await Store.CreateAsync(results);
            StatusMessage = $"Successfully added {results.Count} items!";
            return RedirectToAction(nameof(List));
        }


        [HttpGet("{id}/[action]")]
        public async Task<IActionResult> Delete(int id, int page)
        {
            var item = await Store.FindAsync(id);
            if (item == null) return NotFound();
            return AskPost(
                title: $"Delete record {id}",
                message: $"Are you sure to remove {item.NickName} - {item.Category}, {item.Grade}?",
                routeValues: new { id, page },
                type: BootstrapColor.danger);
        }


        [HttpPost("{id}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int page, bool post = true)
        {
            var item = await Store.FindAsync(id);
            if (item == null) return NotFound();
            await Store.DeleteAsync(item);
            StatusMessage = "Successfully deleted.";
            return RedirectToAction(nameof(List), new { page });
        }


        [HttpGet("{id}/[action]")]
        public async Task<IActionResult> Edit(int id, int page)
        {
            var item = await Store.FindAsync(id);
            if (item == null) return NotFound();
            ViewBag.Page = page;
            return Window(item);
        }


        [HttpPost("{id}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int page, SolveRecord model)
        {
            var item = await Store.FindAsync(id);
            if (item == null) return NotFound();

            item.Account = model.Account;
            item.Category = model.Category;
            item.Grade = model.Grade;
            item.NickName = model.NickName;

            await Store.UpdateAsync(item, resultOnly: false);
            StatusMessage = "Successfully updated.";
            return RedirectToAction(nameof(List), new { page });
        }
    }
}
