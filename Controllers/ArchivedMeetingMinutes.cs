using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.ViewModels;
using NiagaraCollegeProject.Models;
using Microsoft.AspNetCore.Identity;
using NiagaraCollegeProject.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace NiagaraCollegeProject.Controllers
{
    [Authorize(Roles = "Admin, Supervisor")]

    public class ArchivedMeetingMinutesController : Controller
    {
        private readonly PAC_Context _context;
        private readonly ApplicationDbContext _identityContext;
        private readonly IMyEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public ArchivedMeetingMinutesController(PAC_Context context,
            ApplicationDbContext identityContext, IMyEmailSender emailSender,
            UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _identityContext = identityContext;
            _emailSender = emailSender;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;

        }

        public async Task<IActionResult> Index(int? page, int? pageSizeID, int? MeetingID, DateTime? startDate, DateTime? endDate,
            string actionButton, string sortDirection = "asc", string sortField = "Meeting")
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            PopulateDropDownLists();

            var meetingdocuments = _context.MeetingDocuments
                .Include(m => m.Meeting).ThenInclude(m => m.Attendees)
                .Where(d => d.IsArchived == true)
                .AsNoTracking();

            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = "btn-outline-dark"; //Assume not filtering
            //Then in each "test" for filtering, add ViewData["Filtering"] = " show" if true;

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "MEETING ↕", "UPLOADED DATE ↕" };

            //Add as many filters as needed
            if (MeetingID.HasValue)
            {
                meetingdocuments = meetingdocuments.Where(p => p.MeetingID == MeetingID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (startDate.HasValue)
            {
                DateTime start = startDate.Value.Date;
                meetingdocuments = meetingdocuments.Where(m => m.UploadedDate >= start);
                TempData["StartDate"] = startDate.Value.ToString("yyyy-MM-dd");
                ViewData["Filtering"] = "btn-danger";
            }
            if (endDate.HasValue)
            {
                DateTime end = endDate.Value.Date.AddDays(1); // add one day to include the end date
                meetingdocuments = meetingdocuments.Where(m => m.UploadedDate < end);
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
            if (sortField == "MEETING ↕")
            {
                if (sortDirection == "asc")
                {
                    meetingdocuments = meetingdocuments
                        .OrderBy(p => p.Meeting.MeetingTopicName);
                }
                else
                {
                    meetingdocuments = meetingdocuments
                        .OrderByDescending(p => p.Meeting.MeetingTopicName);
                }
            }
            else if (sortField == "UPLOADED DATE ↕")
            {
                if (sortDirection == "asc")
                {
                    meetingdocuments = meetingdocuments
                        .OrderBy(p => p.UploadedDate);
                }
                else
                {
                    meetingdocuments = meetingdocuments
                        .OrderByDescending(p => p.UploadedDate);
                }
            }

            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "MeetingDocuments");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<MeetingDocuments>.CreateAsync(meetingdocuments.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        public async Task<ActionResult> Restore(int? id)
        {
            ViewDataReturnURL();

            var meetingdocuments = await _context.MeetingDocuments
                .Include(m => m.Meeting).ThenInclude(m => m.Attendees)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (id == null || _context.Members == null)
            {
                return NotFound();
            }

            meetingdocuments.IsArchived = false; // set the IsArchived flag to false to restore the record

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Meeting Minute restored successfully!";
            string baseUrll = _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase + "/";
            return Redirect($"{baseUrll}MeetingAttendees?MeetingID={meetingdocuments.MeetingID}");
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.Members == null)
            {
                return NotFound();
            }

            var meetingdocuments = await _context.MeetingDocuments
                .Include(m => m.Meeting).ThenInclude(m => m.Attendees)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (meetingdocuments == null)
            {
                return NotFound();
            }

            return View(meetingdocuments);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewDataReturnURL();

            var meetingdocuments = await _context.MeetingDocuments
                .Include(m => m.Meeting).ThenInclude(m => m.Attendees)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (meetingdocuments == null)
            {
                return Problem("Entity set 'PAC_Context.MeetingDocuments'  is null.");
            }
            if (meetingdocuments != null)
            {
                _context.MeetingDocuments.Remove(meetingdocuments);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Meeting Minute deleted successfully!";
            return Redirect($"/Meetings/Details/{id}");
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
    }
}
