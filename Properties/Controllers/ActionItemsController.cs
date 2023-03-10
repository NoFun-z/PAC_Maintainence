using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;
using NiagaraCollegeProject.Utilities;

namespace NiagaraCollegeProject.Controllers
{
    [Authorize]
    public class ActionItemsController : Controller
    {
        private readonly PAC_Context _context;

        public ActionItemsController(PAC_Context context)
        {
            _context = context;
        }

        // GET: ActionItems
        public async Task<IActionResult> Index(int? page, int? pageSizeID)
        {
            var pAC_Context = _context.ActionItems
                .Include(a => a.Meeting)
                .Include(a => a.Member)
                .Include(d => d.ActionDocuments)
                .AsNoTracking();

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "ActionItems");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<ActionItem>.CreateAsync(pAC_Context.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: ActionItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ActionItems == null)
            {
                return NotFound();
            }

            var actionitem = await _context.ActionItems
                .FirstOrDefaultAsync(m => m.ID == id);
            if (actionitem == null)
            {
                return NotFound();
            }

            return View(actionitem);
        }

        // GET: ActionItems/Create
        [Authorize(Roles = "Admin, Supervisor")]
        public IActionResult Create()
        {
            ViewData["MeetingID"] = new SelectList(_context.Meeting, "ID", "MeetingTopicName");
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName");
            return View();
        }

        // POST: ActionItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Supervisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,AgendaName,Description, TaskDueDate, Comments, Completed,MeetingID,MemberID")] ActionItem actionItem, List<IFormFile> theFiles)
        {
            if (ModelState.IsValid)
            {
                _context.Add(actionItem);
                await AddDocumentsAsync(actionItem, theFiles);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MeetingID"] = new SelectList(_context.Meeting, "ID", "MeetingTopicName", actionItem.MeetingID);
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName", actionItem.MemberID);
            return View(actionItem);
        }

        // GET: ActionItems/Edit/5
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ActionItems == null)
            {
                return NotFound();
            }

           // var actionItem = await _context.ActionItems.FindAsync(id);
            var actionItem = await _context.ActionItems
                .Include(d => d.ActionDocuments)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (actionItem == null)
            {
                return NotFound();
            }
            ViewData["MeetingID"] = new SelectList(_context.Meeting, "ID", "MeetingTopicName", actionItem.MeetingID);
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName", actionItem.MemberID);
            return View(actionItem);
        }

        // POST: ActionItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,AgendaName,Description,Comments,TaskDueDate,Completed,MeetingID,MemberID")] ActionItem actionItem, List<IFormFile> theFiles)
        {
            if (id != actionItem.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actionItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActionItemExists(actionItem.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MeetingID"] = new SelectList(_context.Meeting, "ID", "MeetingTopicName", actionItem.MeetingID);
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName", actionItem.MemberID);
            return View(actionItem);
        }

        // GET: ActionItems/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ActionItems == null)
            {
                return NotFound();
            }

            var actionItem = await _context.ActionItems
                .Include(a => a.Meeting)
                .Include(a => a.Member)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (actionItem == null)
            {
                return NotFound();
            }

            return View(actionItem);
        }

        // POST: ActionItems/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ActionItems == null)
            {
                return Problem("Entity set 'PAC_Context.ActionItems'  is null.");
            }
            var actionItem = await _context.ActionItems.FindAsync(id);
            if (actionItem != null)
            {
                _context.ActionItems.Remove(actionItem);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<FileContentResult> Download(int id)
        {
            var theFile = await _context.UploadedFile
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .FirstOrDefaultAsync();
            return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
        }
        private async Task AddDocumentsAsync(ActionItem action, List<IFormFile> theFiles)
        {
            foreach (var f in theFiles)
            {
                if (f != null)
                {
                    string mimeType = f.ContentType;
                    string fileName = Path.GetFileName(f.FileName);
                    long fileLength = f.Length;
                    //Note: you could filter for mime types if you only want to allow
                    //certain types of files.  I am allowing everything.
                    if (!(fileName == "" || fileLength == 0))//Looks like we have a file!!!
                    {
                        ActionDocuments d = new ActionDocuments();
                        using (var memoryStream = new MemoryStream())
                        {
                            await f.CopyToAsync(memoryStream);
                            d.FileContent.Content = memoryStream.ToArray();
                        }
                        d.MimeType = mimeType;
                        d.FileName = fileName;
                        action.ActionDocuments.Add(d);
                    };
                }
            }
        }
        private bool ActionItemExists(int id)
        {
          return _context.ActionItems.Any(e => e.ID == id);
        }
    }
}
