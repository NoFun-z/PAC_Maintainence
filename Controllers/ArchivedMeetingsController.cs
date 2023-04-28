using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;
using NiagaraCollegeProject.Utilities;

namespace NiagaraCollegeProject.Controllers
{
    [Authorize(Roles = "Admin, Supervisor")]

    public class ArchivedMeetingsController : Controller
    {
        private readonly PAC_Context _context;
        private readonly ApplicationDbContext _identityContext;
        private readonly IMyEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public ArchivedMeetingsController(PAC_Context context,
            ApplicationDbContext identityContext, IMyEmailSender emailSender,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _identityContext = identityContext;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string sortDirectionCheck, string sortFieldID, int? page, int? pageSizeID,
            string SearchString, int? AcademicDivisionID, int? PACID, string successMessage, string actionButton,
            string sortDirection = "asc", string sortField = "Meeting")
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);
            ViewData["Filtering"] = "btn-outline-primary";
            PopulateDropDownLists();

            var meeting = _context.Meetings
                .Include(d => d.MeetingDocuments)
                .Include(m => m.AcademicDivision)
                .Include(m => m.PAC)
                .Include(d => d.MeetingMinute)
                .Include(d => d.Attendees).ThenInclude(m => m.Member)
                .Include(d => d.Member)
                .Where(d => d.IsArchived == true)
                .AsNoTracking();

            string[] sortOptions = new[] {"MEETING DATE/START TIME ↕", "ATTENDEES ↕" };


            if (AcademicDivisionID.HasValue)
            {
                meeting = meeting.Where(c => c.AcademicDivisionID == AcademicDivisionID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (PACID.HasValue)
            {
                meeting = meeting.Where(c => c.PACID == PACID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                meeting = meeting.Where(c => c.MeetingTopicName.ToUpper().Contains(SearchString.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }

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
                else //Sort by the controls in the filter area
                {
                    sortDirection = String.IsNullOrEmpty(sortDirectionCheck) ? "asc" : "desc";
                    sortField = sortFieldID;
                }
            }

            if (sortField == "MEETING DATE/START TIME ↕")
            {
                if (sortDirection == "asc")
                {
                    meeting = meeting
                        .OrderBy(p => p.MeetingStartTimeDate);
                }
                else
                {
                    meeting = meeting
                        .OrderByDescending(p => p.MeetingStartTimeDate);
                }
            }
            else if (sortField == "ATTENDEES ↕")
            {
                if (sortDirection == "asc")
                {
                    meeting = meeting
                        .OrderByDescending(p => p.Attendees.Count());
                }
                else
                {
                    meeting = meeting
                        .OrderBy(p => p.Attendees.Count());
                }

            }

            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            //SelectList for Sorting Options
            ViewBag.sortFieldID = new SelectList(sortOptions, sortField.ToString());

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "Meetings");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Meeting>.CreateAsync(meeting.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        public async Task<ActionResult> Restore(int? id)
        {
            ViewDataReturnURL();

            var meeting = await _context.Meetings
                .Include(d => d.MeetingDocuments)
                .Include(m => m.AcademicDivision)
                .Include(m => m.PAC)
                .Include(d => d.MeetingMinute)
                .Include(d => d.Member)
                .Include(d => d.Attendees).ThenInclude(m => m.Member)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (id == null || _context.Meetings == null)
            {
                return NotFound();
            }

            meeting.IsArchived = false; // set the IsArchived flag to false to restore the record

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Meeting restored successfully!";
            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.Meetings == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meetings
                .Include(d => d.MeetingDocuments)
                .Include(m => m.AcademicDivision).ThenInclude(m => m.PACs)
                .Include(m => m.PAC)
                .Include(d => d.MeetingMinute)
                .Include(d => d.Member)
                .Include(d => d.Attendees).ThenInclude(m => m.Member)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (meeting == null)
            {
                return NotFound();
            }

            return View(meeting);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewDataReturnURL();

            var meeting = await _context.Meetings
                .Include(d => d.MeetingDocuments)
                .Include(m => m.AcademicDivision).ThenInclude(m => m.PACs)
                .Include(m => m.PAC)
                .Include(d => d.MeetingMinute)
                .Include(d => d.Member)
                .Include(d => d.Attendees).ThenInclude(m => m.Member)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (meeting == null)
            {
                return Problem("Entity set 'PAC_Context.Meetings'  is null.");
            }
            if (meeting != null)
            {
                _context.Meetings.Remove(meeting);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Meeting deleted successfully!";
            return Redirect(ViewData["returnURL"].ToString());
        }

        private SelectList AcademicDivisionSelectList(int? selectedID)
        {
            return new SelectList(_context.AcademicDivisions
                .OrderBy(c => c.DivisionName), "ID", "DivisionName", selectedID);
        }

        private SelectList PACSelectList(int? selectedID)
        {
            return new SelectList(_context.PAC
                .OrderBy(a => a.PACName), "ID", "PACName", selectedID);
        }
        private void PopulateDropDownLists(AcademicDivision division = null, PAC pac = null)
        {
            ViewData["AcademicDivisionID"] = AcademicDivisionSelectList(division?.ID);
            ViewData["PACID"] = PACSelectList(pac?.ID);
        }

        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }
    }
}
