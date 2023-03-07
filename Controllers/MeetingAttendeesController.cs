using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;
using NiagaraCollegeProject.Utilities;

namespace NiagaraCollegeProject.Controllers
{
    public class MeetingAttendeesController : Controller
    {
        private readonly PAC_Context _context;

        public MeetingAttendeesController(PAC_Context context)
        {
            _context = context;
        }

        // GET: MeetingAttendees
        public async Task<IActionResult> Index(int? MeetingID, int? page, int? pageSizeID, string actionButton, string sortDirection = "asc", string sortField = "Members ↕")
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //Get the URL with the last filter, sort and page parameters from THE Meetings Index View
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Meetings");

            if (!MeetingID.HasValue)
            {
                //Go back to the proper return URL for the Patients controller
                return Redirect(ViewData["returnURL"].ToString());
            }

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Members ↕" };

            var pAC_Context = from m in _context.MeetingAttendees
                .Include(m => m.Meeting)
                .Include(m => m.Member).ThenInclude(m => m.PAC)
                where m.MeetingID == MeetingID.GetValueOrDefault()
                select m;

            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
            {
                page = 1;//Reset back to first page when sorting or filtering

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
            if (sortField == "Members ↕")
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

            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "MeetingAttendeess");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<MeetingAttendees>.CreateAsync(pAC_Context.AsNoTracking(), page ?? 1, pageSize);

            //Now get the MASTER record, the patient, so it can be displayed at the top of the screen
            Meeting meeting = _context.Meetings
                .Include(d => d.MeetingDocuments)
                .Include(d => d.AgendaItems)
                .Include(d => d.AcademicDivision).ThenInclude(d => d.PACs)
                .Include(d => d.MeetingMinute)
                .Include(d => d.Attendees).ThenInclude(d => d.Member)
                .Where(d => d.ID == MeetingID.GetValueOrDefault())
                .AsNoTracking()
                .FirstOrDefault();

            ViewBag.Meeting = meeting;

            return View(pagedData);
        }

        // GET: MeetingAttendees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();


            if (id == null || _context.MeetingAttendees == null)
            {
                return NotFound();
            }

            var meetingAttendees = await _context.MeetingAttendees
                .Include(m => m.Meeting)
                .Include(m => m.Member).ThenInclude(m => m.PAC)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (meetingAttendees == null)
            {
                return NotFound();
            }

            return View(meetingAttendees);
        }

        // GET: MeetingAttendees/Create
        public IActionResult Add(int? MeetingID, string MeetingTopicName)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            if (!MeetingID.HasValue)
            {
                return Redirect(ViewData["returnURL"].ToString());
            }

            ViewData["MeetingTopicName"] = MeetingTopicName;

            MeetingAttendees ma = new MeetingAttendees()
            {
                MeetingID = MeetingID.GetValueOrDefault() 
            };

            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "ID");
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName");
            return View(ma);
        }

        // POST: MeetingAttendees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("ID,MeetingID,MemberID")] MeetingAttendees meetingAttendees, string MeetingTopicName)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(meetingAttendees);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                    "persists see your system administrator.");
            }

            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "ID");
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName");
            ViewData["MeetingTopicName"] = MeetingTopicName;
            return View(meetingAttendees);
        }

        // GET: MeetingAttendees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();


            if (id == null || _context.MeetingAttendees == null)
            {
                return NotFound();
            }

            var meetingAttendees = await _context.MeetingAttendees.FindAsync(id);
            if (meetingAttendees == null)
            {
                return NotFound();
            }
            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "MeetingTopicName", meetingAttendees.MeetingID);
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName", meetingAttendees.MemberID);
            return View(meetingAttendees);
        }

        // POST: MeetingAttendees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,MeetingID,MemberID")] MeetingAttendees meetingAttendees)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();


            if (id != meetingAttendees.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(meetingAttendees);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MeetingAttendeesExists(meetingAttendees.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", new { meetingAttendees.ID });
            }
            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "MeetingTopicName", meetingAttendees.MeetingID);
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName", meetingAttendees.MemberID);
            return View(meetingAttendees);
        }

        // GET: MeetingAttendees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();


            if (id == null || _context.MeetingAttendees == null)
            {
                return NotFound();
            }

            var meetingAttendees = await _context.MeetingAttendees
                .Include(m => m.Meeting)
                .Include(m => m.Member).ThenInclude(m => m.PAC)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (meetingAttendees == null)
            {
                return NotFound();
            }

            return View(meetingAttendees);
        }

        // POST: MeetingAttendees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();


            if (_context.MeetingAttendees == null)
            {
                return Problem("Entity set 'PAC_Context.MeetingAttendees'  is null.");
            }
            var meetingAttendees = await _context.MeetingAttendees.FindAsync(id);
            if (meetingAttendees != null)
            {
                _context.MeetingAttendees.Remove(meetingAttendees);
            }
            
            await _context.SaveChangesAsync();
            return Redirect(ViewData["returnURL"].ToString());
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }
        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }

        private bool MeetingAttendeesExists(int id)
        {
          return _context.MeetingAttendees.Any(e => e.ID == id);
        }
    }
}
