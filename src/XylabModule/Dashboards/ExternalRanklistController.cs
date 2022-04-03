using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.OjUpdateModule.Entities;
using SatelliteSite.OjUpdateModule.Models;
using SatelliteSite.OjUpdateModule.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.XylabModule.Dashboards
{
    [Area("Dashboard")]
    [Authorize(Roles = "Administrator,TeamLeader")]
    [Route("[area]/[controller]")]
    [AuditPoint(AuditlogType.User)]
    public class ExternalRanklistController : ViewControllerBase
    {
        const int ItemsPerPage = 50;

        private IRecordStorage Store { get; }

        public ExternalRanklistController(IRecordStorage store)
        {
            Store = store;
        }


        [HttpGet]
        public async Task<IActionResult> List()
        {
            return View(await Store.ListAsync());
        }


        [HttpGet("[action]/{category}")]
        public async Task<IActionResult> Cleanup(int category)
        {
            ViewBag.Category = category;
            var record = (RecordType)category;
            var model = await Store.GetAllAsync(record);
            
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
            var toDelete = model.ToDelete ?? Array.Empty<string>();
            
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
            [FromForm] CreateRecordModel[] Batch)
        {
            var results = Batch
                .Where(item
                    => !string.IsNullOrWhiteSpace(item.Account)
                    && !string.IsNullOrWhiteSpace(item.NickName))
                .ToList();

            await Store.CreateAsync(results);
            StatusMessage = $"Successfully added {results.Count} items!";
            return RedirectToAction(nameof(List));
        }


        [HttpGet("{id}/[action]")]
        public async Task<IActionResult> Delete(string id)
        {
            var item = await Store.FindAsync(id);
            if (item == null) return NotFound();
            return AskPost(
                title: $"Delete record {id}",
                message: $"Are you sure to remove {item.NickName} - {item.Category}, {item.Grade}?",
                type: BootstrapColor.danger);
        }


        [HttpPost("{id}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, bool post = true)
        {
            var item = await Store.FindAsync(id);
            if (item == null) return NotFound();
            await Store.DeleteAsync(item.Category, new[] { id });
            StatusMessage = "Successfully deleted.";
            return RedirectToAction(nameof(List));
        }


        [HttpGet("{id}/[action]")]
        public async Task<IActionResult> Edit(string id)
        {
            var item = await Store.FindAsync(id);
            if (item == null) return NotFound();
            return Window(item);
        }


        [HttpPost("{id}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CreateRecordModel model)
        {
            var item = await Store.FindAsync(id);
            if (item == null) return NotFound();

            await Store.UpdateAsync(item, model);
            StatusMessage = "Successfully updated.";
            return RedirectToAction(nameof(List));
        }
    }
}
