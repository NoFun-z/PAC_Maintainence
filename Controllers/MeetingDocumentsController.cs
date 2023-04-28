using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;
using NiagaraCollegeProject.Utilities;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace NiagaraCollegeProject.Controllers
{
    [Authorize]

    public class MeetingDocumentsController : Controller
    {
        private readonly PAC_Context _context;
        private readonly IMyEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public MeetingDocumentsController(PAC_Context context,
            IMyEmailSender emailSender,IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;

        }
        // GET: MeetingDocuments
        public async Task<IActionResult> Index(int? page, int? pageSizeID, int? MeetingID, DateTime? startDate, DateTime? endDate,
            string actionButton, string sortDirection = "asc", string sortField = "Meeting")
        {

            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //Get the URL with the last filter, sort and page parameters from THE Meetings Index View
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "MeetingDocuments");

            PopulateDropDownLists();

            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = "btn-outline-dark"; //Assume not filtering
            //Then in each "test" for filtering, add ViewData["Filtering"] = " show" if true;

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Meeting ↕", "UploadedDate ↕" };

            var pAC_Context = _context.MeetingDocuments
                .Include(m => m.Meeting).ThenInclude(m => m.Attendees)
                .Where(m => m.IsArchived == false)
                .AsNoTracking();

            //Add as many filters as needed
            if (MeetingID.HasValue)
            {
                pAC_Context = pAC_Context.Where(p => p.MeetingID == MeetingID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (startDate.HasValue)
            {
                DateTime start = startDate.Value.Date;
                pAC_Context = pAC_Context.Where(m => m.UploadedDate >= start);
                TempData["StartDate"] = startDate.Value.ToString("yyyy-MM-dd");
                ViewData["Filtering"] = "btn-danger";
            }
            if (endDate.HasValue)
            {
                DateTime end = endDate.Value.Date.AddDays(1); // add one day to include the end date
                pAC_Context = pAC_Context.Where(m => m.UploadedDate < end);
                TempData["EndDate"] = endDate.Value.ToString("yyyy-MM-dd");
                ViewData["Filtering"] = "btn-danger";
            }

            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
            {
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
            if (sortField == "Meeting ↕")
            {
                if (sortDirection == "asc")
                {
                    pAC_Context = pAC_Context
                        .OrderBy(p => p.Meeting.MeetingTopicName);
                }
                else
                {
                    pAC_Context = pAC_Context
                        .OrderByDescending(p => p.Meeting.MeetingTopicName);
                }
            }
            else if (sortField == "UploadedDate ↕")
            {
                if (sortDirection == "asc")
                {
                    pAC_Context = pAC_Context
                        .OrderBy(p => p.UploadedDate);
                }
                else
                {
                    pAC_Context = pAC_Context
                        .OrderByDescending(p => p.UploadedDate);
                }
            }

            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "MeetingDocuments");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<MeetingDocuments>.CreateAsync(pAC_Context.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: MeetingDocuments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.MeetingDocuments == null)
            {
                return NotFound();
            }

            var meetingDocuments = await _context.MeetingDocuments
                .Include(m => m.Meeting)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (meetingDocuments == null)
            {
                return NotFound();
            }

            return View(meetingDocuments);
        }

        // GET: MeetingDocuments/Create
        public IActionResult Create()
        {
            ViewDataReturnURL();

            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "MeetingTopicName");
            return View();
        }

        // POST: MeetingDocuments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MeetingID,ID,FileName,Description,MimeType")] MeetingDocuments meetingDocuments)
        {
            ViewDataReturnURL();

            if (ModelState.IsValid)
            {
                _context.Add(meetingDocuments);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Meeting Minute created successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "MeetingTopicName", meetingDocuments.MeetingID);
            return View(meetingDocuments);
        }

        // GET: MeetingDocuments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.MeetingDocuments == null)
            {
                return NotFound();
            }

            var meetingDocuments = await _context.MeetingDocuments.FindAsync(id);
            if (meetingDocuments == null)
            {
                return NotFound();
            }
            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "MeetingTopicName", meetingDocuments.MeetingID);
            return View(meetingDocuments);
        }

        // POST: MeetingDocuments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MeetingID,ID,FileName,Description,MimeType")] MeetingDocuments meetingDocuments)
        {
            ViewDataReturnURL();

            if (id != meetingDocuments.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(meetingDocuments);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Meeting Minute edited successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MeetingDocumentsExists(meetingDocuments.ID))
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
            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "MeetingTopicName", meetingDocuments.MeetingID);
            return View(meetingDocuments);
        }

        public async Task<IActionResult> Archive(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.MeetingDocuments == null)
            {
                return NotFound();
            }

            var meetingdocuments = await _context.MeetingDocuments
                .Include(m => m.Meeting).ThenInclude(m => m.Attendees)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (meetingdocuments == null)
            {
                return NotFound();
            }

            return View(meetingdocuments);
        }

        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            ViewDataReturnURL();

            var meetingdocuments = await _context.MeetingDocuments
                .Include(m => m.Meeting).ThenInclude(m => m.Attendees)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (meetingdocuments == null)
            {
                return NotFound();
            }

            if (meetingdocuments != null)
            {
                meetingdocuments.IsArchived = true;

                _context.Entry(meetingdocuments).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Meeting Minute archived successfully!";
            }
           
            string baseUrll = _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase + "/";
            return Redirect($"{baseUrll}MeetingAttendees?MeetingID={meetingdocuments.MeetingID}");
        }

        // GET: MeetingDocuments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.MeetingDocuments == null)
            {
                return NotFound();
            }

            var meetingDocuments = await _context.MeetingDocuments
                .Include(m => m.Meeting)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (meetingDocuments == null)
            {
                return NotFound();
            }

            return View(meetingDocuments);
        }

        // POST: MeetingDocuments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewDataReturnURL();

            if (_context.MeetingDocuments == null)
            {
                return Problem("Entity set 'PAC_Context.MeetingDocuments' is null.");
            }
            var meetingDocuments = await _context.MeetingDocuments.FindAsync(id);
            if (meetingDocuments != null)
            {
                _context.MeetingDocuments.Remove(meetingDocuments);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Meeting Minute deleted successfully!";
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

        private void PopulateDropDownLists(MeetingDocuments meetDocuments = null)
        {
            var dQuery = from d in _context.MeetingDocuments
                         select d;
            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "MeetingTopicName", meetDocuments?.MeetingID);
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }

        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }

        private bool MeetingDocumentsExists(int id)
        {
            return _context.MeetingDocuments.Any(e => e.ID == id);
        }
    }
}