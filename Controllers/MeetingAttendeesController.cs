using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;
using NiagaraCollegeProject.Utilities;
using NiagaraCollegeProject.ViewModels;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace NiagaraCollegeProject.Controllers
{
    [Authorize]

    public class MeetingAttendeesController : Controller
    {
        private readonly PAC_Context _context;

        public MeetingAttendeesController(PAC_Context context)
        {
            _context = context;
        }

        // GET: MeetingAttendees
        public async Task<IActionResult> Index(int? MeetingID, int? MemberID, bool? Completed, DateTime? startDate, DateTime? endDate,
            string activeTab, int? page, int? pageSizeID, string actionButton, string actionButtonD, string actionButtonA,
            string sortDirection = "asc", string sortField = "Members ↕", string sortDirectionD = "asc", string sortFieldD = "Meeting",
            string sortDirectionA = "asc", string sortFieldA = "Member")
        {

            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //Get the URL with the last filter, sort and page parameters from THE Meetings Index View
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Meetings");

            if (!MeetingID.HasValue)
            {
                //Go back to the proper return URL for the meetingattendees controller
                return Redirect(ViewData["returnURL"].ToString());
            }

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Members ↕" };


            var pAC_Context = from m in _context.MeetingAttendees
                .Include(m => m.Meeting).ThenInclude(m => m.Member)
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

            ////////////////////////////////////
            ////////////////////////////////////
            //Now get the MASTER record, the meeting, so it can be displayed at the top of the screen
            Meeting meeting = _context.Meetings
                .Include(d => d.MeetingDocuments)
                .Include(m => m.Member)
                .Include(d => d.AcademicDivision).ThenInclude(d => d.PACs)
                .Include(d => d.PAC)
                .Include(d => d.MeetingMinute)
                .Include(d => d.Attendees).ThenInclude(d => d.Member)
                .Where(d => d.ID == MeetingID.GetValueOrDefault())
                .AsNoTracking()
                .FirstOrDefault();

            ViewBag.Meeting = meeting;

            ////////////////////////////////////
            ////////////////////////////////////
            //Now get the MASTER record, the meeting minutes, so it can be displayed at the bottom of the attendees table
            var meetingdocuments = _context.MeetingDocuments
                .Include(d => d.Meeting).ThenInclude(d => d.Attendees)
                .Where(d => d.MeetingID == MeetingID.GetValueOrDefault() && d.IsArchived == false)
                .AsNoTracking();

            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["FilteringD"] = "btn-outline-dark"; //Assume not filtering
            //Then in each "test" for filtering, add ViewData["Filtering"] = " show" if true;

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptionsD = new[] { "FILE NAME ↕", "UPLOADED DATE ↕" };

            //Add as many filters as needed
            if (startDate.HasValue)
            {
                DateTime start = startDate.Value.Date;
                meetingdocuments = meetingdocuments.Where(m => m.UploadedDate >= start);
                TempData["StartDate"] = startDate.Value.ToString("yyyy-MM-dd");
                ViewData["FilteringD"] = "btn-danger";
            }
            if (endDate.HasValue)
            {
                DateTime end = endDate.Value.Date.AddDays(1); // add one day to include the end date
                meetingdocuments = meetingdocuments.Where(m => m.UploadedDate < end);
                TempData["EndDate"] = endDate.Value.ToString("yyyy-MM-dd");
                ViewData["FilteringD"] = "btn-danger";
            }

            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButtonD)) //Form Submitted!
            {
                if (sortOptionsD.Contains(actionButtonD))//Change of sort is requested
                {
                    if (actionButtonD == sortFieldD) //Reverse order on same field
                    {
                        sortDirectionD = sortDirectionD == "asc" ? "desc" : "asc";
                    }
                    sortFieldD = actionButtonD;//Sort by the button clicked
                }
            }
            //Now we know which field and direction to sort by
            if (sortFieldD == "FILE NAME ↕")
            {
                if (sortDirectionD == "asc")
                {
                    meetingdocuments = meetingdocuments
                        .OrderBy(p => p.FileName);
                }
                else
                {
                    meetingdocuments = meetingdocuments
                        .OrderByDescending(p => p.FileName);
                }
            }
            else if (sortFieldD == "UPLOADED DATE ↕")
            {
                if (sortDirectionD == "asc")
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
            ViewData["sortFieldD"] = sortFieldD;
            ViewData["sortDirectionD"] = sortDirectionD;

            //Handle separate Paging - out of my league for now :)
            //pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "MeetingDocuments");
            //ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            //pagedData = await PaginatedList<MeetingDocuments>.CreateAsync(meetingdocuments.AsNoTracking(), page ?? 1, pageSize);

            ViewBag.MeetingDocuments = meetingdocuments;

            ////////////////////////////////////
            ////////////////////////////////////
            //GET MASTER RECORD FOR ACTION ITEMS
            var meetingactionitems = _context.ActionItems
                .Include(a => a.Meeting)
                .Include(a => a.Member)
                .Include(d => d.ActionDocuments)
                .Where(d => d.MeetingID == MeetingID.GetValueOrDefault() && d.IsArchived == false)
                .AsNoTracking();

            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["FilteringA"] = "btn-outline-dark"; //Assume not filtering
            //Then in each "test" for filtering, add ViewData["Filtering"] = " show" if true;

            //Add as many filters as needed
            if (MemberID.HasValue)
            {
                meetingactionitems = meetingactionitems.Where(p => p.MemberID == MemberID);
                ViewData["FilteringA"] = "btn-danger";
            }
            if (Completed.HasValue)
            {
                meetingactionitems = meetingactionitems.Where(p => p.Completed == Completed);
                ViewData["FilteringA"] = "btn-danger";
            }


            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptionsA = new[] { "MEMBER ↕", "DUE DATE ↕" };


            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButtonA)) //Form Submitted!
            {
                page = 1;//Reset page to start
                if (sortOptionsA.Contains(actionButtonA))//Change of sort is requested
                {
                    if (actionButtonA == sortFieldA) //Reverse order on same field
                    {
                        sortDirectionA = sortDirectionA == "asc" ? "desc" : "asc";
                    }
                    sortFieldA = actionButtonA;//Sort by the button clicked
                }
            }

            //Now we know which field and direction to sort by
            if (sortFieldA == "MEMBER ↕")
            {
                if (sortDirectionA == "asc")
                {
                    meetingactionitems = meetingactionitems
                        .OrderBy(p => p.Member.LastName)
                        .ThenBy(p => p.Member.FirstName);
                }
                else
                {
                    meetingactionitems = meetingactionitems
                        .OrderByDescending(p => p.Member.LastName)
                        .ThenByDescending(p => p.Member.FirstName);
                }
            }
            else if (sortFieldA == "DUE DATE ↕")
            {
                if (sortDirectionA == "asc")
                {
                    meetingactionitems = meetingactionitems
                        .OrderBy(p => p.TaskDueDate);
                }
                else
                {
                    meetingactionitems = meetingactionitems
                        .OrderByDescending(p => p.TaskDueDate);
                }
            }

            //Set sort for next time
            ViewData["sortFieldA"] = sortFieldA;
            ViewData["sortDirectionA"] = sortDirectionA;

            ActionItem actionitems = null;
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName", actionitems?.MemberID);
            List<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem { Text = "Completed", Value = "True" },
                new SelectListItem { Text = "Incomplete", Value = "False" }
            };

            ViewData["Completed"] = new SelectList(items, "Value", "Text", actionitems?.Completed);

            ViewBag.Meetingactionitems = meetingactionitems;

            ViewBag.ActiveTab = activeTab ?? "table1";

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
                .Include(m => m.Member)
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

            int? meetingPac = _context.Meetings.Where(m => m.ID == MeetingID).FirstOrDefault().PACID;

            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "ID");
            ViewData["MemberID"] = new SelectList(_context.Members.Where(m => m.IsArchived == false && m.PACID == meetingPac), "ID", "FullName");
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
                    if (_context.Meetings.Any(m => m.ID == meetingAttendees.MeetingID && m.Attendees
                    .Any(a => a.Member.ID == meetingAttendees.MemberID)))
                    {
                        ModelState.AddModelError("", "The attendee has already existed in the meeting. Please try to add another.");
                    }
                    else
                    {
                        _context.Add(meetingAttendees);
                        await _context.SaveChangesAsync();
                        TempData["successMessage"] = "Attendee added successfully!";
                        return Redirect(ViewData["returnURL"].ToString());
                    }
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
                    TempData["successMessage"] = "Attendees edited successfully!";
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
            TempData["successMessage"] = "Attendee removed successfully!";
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
