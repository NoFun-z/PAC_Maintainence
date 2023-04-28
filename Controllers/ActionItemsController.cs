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
using SkiaSharp;

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
        public async Task<IActionResult> Index(int? page, int? pageSizeID, int? MemberID, DateTime? startDate, DateTime? endDate,
            bool? Completed, string actionButton, string sortDirection = "asc", string sortField = "Member")
        {

            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            PopulateDropDownLists();

            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = "btn-outline-dark"; //Assume not filtering
            //Then in each "test" for filtering, add ViewData["Filtering"] = " show" if true;

            var pAC_Context = _context.ActionItems
                .Include(a => a.Meeting)
                .Include(a => a.Member)
                .Include(a => a.ActionDocuments)
                .Where(a => a.IsArchived == false)
                .AsNoTracking();

            //Add as many filters as needed
            if (MemberID.HasValue)
            {
                pAC_Context = pAC_Context.Where(p => p.MemberID == MemberID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (startDate.HasValue)
            {
                DateTime start = startDate.Value.Date;
                pAC_Context = pAC_Context.Where(m => m.TaskDueDate >= start);
                TempData["StartDate"] = startDate.Value.ToString("yyyy-MM-dd");
                ViewData["Filtering"] = "btn-danger";
            }
            if (endDate.HasValue)
            {
                DateTime end = endDate.Value.Date.AddDays(1); // add one day to include the end date
                pAC_Context = pAC_Context.Where(m => m.TaskDueDate < end);
                TempData["EndDate"] = endDate.Value.ToString("yyyy-MM-dd");
                ViewData["Filtering"] = "btn-danger";
            }
            if (Completed.HasValue)
            {
                pAC_Context = pAC_Context.Where(p => p.Completed == Completed);
                ViewData["Filtering"] = "btn-danger";
            }


            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "MEMBER ↕", "DUE DATE ↕" };


            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
            {
                page = 1;//Reset page to start
                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }

            //Now we know which field and direction to sort by
            if (sortField == "MEMBER ↕")
            {
                if (sortDirection == "asc")
                {
                    pAC_Context = pAC_Context
                        .OrderBy(p => p.Member.LastName)
                        .ThenBy(p => p.Member.FirstName);
                }
                else
                {
                    pAC_Context = pAC_Context
                        .OrderByDescending(p => p.Member.LastName)
                        .ThenByDescending(p => p.Member.FirstName);
                }
            }
            else if (sortField == "DUE DATE ↕")
            {
                if (sortDirection == "asc")
                {
                    pAC_Context = pAC_Context
                        .OrderBy(p => p.TaskDueDate);
                }
                else
                {
                    pAC_Context = pAC_Context
                        .OrderByDescending(p => p.TaskDueDate);
                }
            }

            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Filter Member Tasks
            var FilteredTask = pAC_Context.Where(t => t.Member.Email == User.Identity.Name).AsNoTracking();

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "ActionItems");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<ActionItem>.CreateAsync(pAC_Context.AsNoTracking(), page ?? 1, pageSize);

            if (User.IsInRole("Admin") || User.IsInRole("Supervisor"))
            {
                pagedData = await PaginatedList<ActionItem>.CreateAsync(pAC_Context.AsNoTracking(), page ?? 1, pageSize);
            }
            if (User.IsInRole("Staff"))
            {
                pagedData = await PaginatedList<ActionItem>.CreateAsync(FilteredTask.AsNoTracking(), page ?? 1, pageSize);
            }

            return View(pagedData);
        }

        // GET: ActionItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.ActionItems == null)
            {
                return NotFound();
            }

            var actionitem = await _context.ActionItems
                .Include(a => a.Meeting)
                .Include(a => a.Member)
                .Include(a => a.ActionDocuments)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (actionitem == null)
            {
                return NotFound();
            }

            ViewData["MeetingID"] = new SelectList(_context.Meeting, "ID", "MeetingTopicName");
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName");

            return View(actionitem);
        }

        // GET: ActionItems/Create
        [Authorize(Roles = "Admin, Supervisor")]
        public IActionResult Create()
        {
            ViewDataReturnURL();

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
            ViewDataReturnURL();

            if (ModelState.IsValid)
            {
                _context.Add(actionItem);
                await AddDocumentsAsync(actionItem, theFiles);
                UpdateMemberActionItems(actionItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Action Item assigned successfully!";
                return Redirect(ViewData["returnURL"].ToString());
            }
            ViewData["MeetingID"] = new SelectList(_context.Meeting, "ID", "MeetingTopicName", actionItem.MeetingID);
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName", actionItem.MemberID);
            return View(actionItem);
        }

        // GET: ActionItems/Add
        [Authorize(Roles = "Admin, Supervisor")]
        public IActionResult Add(int? meetingID)
        {
            ViewDataReturnURL();

            if (!meetingID.HasValue)
            {
                return Redirect(ViewData["returnURL"].ToString());
            }

            ActionItem ai = new ActionItem()
            {
                TaskDueDate = DateTime.Now,
                MeetingID = meetingID.GetValueOrDefault()
            };

            ViewData["MeetingID"] = new SelectList(_context.Meeting, "ID", "MeetingTopicName", meetingID);
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName");
            return View(ai);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("ID,AgendaName,Description, TaskDueDate, Comments, Completed,MeetingID,MemberID")] ActionItem actionItem, List<IFormFile> theFiles)
        {
            ViewDataReturnURL();

            try
            {
                if (ModelState.IsValid)
                {
                    if (actionItem.MeetingID == null)
                    {
                        ModelState.AddModelError("MeetingID", "Please select a valid Meeting.");
                        ViewData["MeetingID"] = new SelectList(_context.Meeting, "ID", "MeetingTopicName", actionItem.MeetingID);
                        ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName", actionItem.MemberID);
                        return View(actionItem);
                    }

                    _context.Add(actionItem);
                    await AddDocumentsAsync(actionItem, theFiles);
                    UpdateMemberActionItems(actionItem);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Action Item assigned successfully!";
                    return Redirect($"/MeetingAttendees/Index/{actionItem.MeetingID}");
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                    "persists see your system administrator.");
            }

            ViewData["MeetingID"] = new SelectList(_context.Meeting, "ID", "MeetingTopicName", actionItem.MeetingID);
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName", actionItem.MemberID);
            return View(actionItem);
        }


        // GET: ActionItems/Edit/5
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        public async Task<IActionResult> Edit(int? id, int? MeetingID, string AgendaName)
        {
            ViewDataReturnURL();

            if (id == null || _context.ActionItems == null)
            {
                return NotFound();
            }

            // var actionItem = await _context.ActionItems.FindAsync(id);
            var actionItem = await _context.ActionItems
                .Include(d => d.Meeting)
                .Include(d => d.Member)
                .Include(d => d.ActionDocuments)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (actionItem == null)
            {
                return NotFound();
            }

            actionItem.MeetingID = MeetingID;
            ViewData["AgendaName"] = AgendaName;

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
            ViewDataReturnURL();

            if (id != actionItem.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actionItem);
                    await AddDocumentsAsync(actionItem, theFiles);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Action Item edited successfully!";
                    return Redirect(ViewData["returnURL"].ToString());
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
            }
            ViewData["MeetingID"] = new SelectList(_context.Meeting, "ID", "MeetingTopicName", actionItem.MeetingID);
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName", actionItem.MemberID);
            return View(actionItem);
        }

        public async Task<IActionResult> Archive(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.ActionItems == null)
            {
                return NotFound();
            }

            var actionItem = await _context.ActionItems
                .Include(d => d.Meeting)
                .Include(d => d.Member)
                .Include(d => d.ActionDocuments)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (actionItem == null)
            {
                return NotFound();
            }

            return View(actionItem);
        }

        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            ViewDataReturnURL();

            var actionItem = await _context.ActionItems
                .Include(d => d.Meeting)
                .Include(d => d.Member)
                .Include(d => d.ActionDocuments)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (actionItem == null)
            {
                return NotFound();
            }

            if (actionItem != null)
            {
                actionItem.IsArchived = true;

                _context.Entry(actionItem).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Action Item archived successfully!";
            }

            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: ActionItems/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewDataReturnURL();

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
            ViewDataReturnURL();

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
            TempData["SuccessMessage"] = "Action Item deleted successfully!";
            return Redirect(ViewData["returnURL"].ToString());
        }

        private void PopulateDropDownLists(ActionItem actionitems = null)
        {
            var aQuery = from a in _context.ActionItems
                         select a;
            ViewData["MeetingID"] = new SelectList(_context.Meeting, "ID", "MeetingTopicName", actionitems?.MeetingID);
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName", actionitems?.MemberID);
            List<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem { Text = "Completed", Value = "True" },
                new SelectListItem { Text = "Incomplete", Value = "False" }
            };

            ViewData["Completed"] = new SelectList(items, "Value", "Text", actionitems?.Completed);
        }

        public async Task<FileContentResult> Download(int id)
        {
            var theFile = await _context.UploadedFile
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .FirstOrDefaultAsync();
            return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }
        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
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

        //update MEMBERACTIONITEMS
        private void UpdateMemberActionItems(ActionItem itemToUpdate)
        {

            itemToUpdate.MemberActionItems.Add(new MemberActionItems
            {
                MemberID = itemToUpdate.MemberID,
                ActionItemID = itemToUpdate.ID
            });
        }

        //private void UpdateMemberActionItems2(ActionItem itemToUpdate)
        //{
        //    var memaction = _context.MemberActionItems.FirstOrDefault(m => m.MemberID == itemToUpdate.MemberID);

        //    if (itemToUpdate != null)
        //    {
        //        // Modify the retrieved data
        //        memaction.Name = newName;
        //    }
        //}

        private bool ActionItemExists(int id)
        {
            return _context.ActionItems.Any(e => e.ID == id);
        }
    }
}
