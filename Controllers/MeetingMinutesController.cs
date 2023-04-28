using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;
using NiagaraCollegeProject.ViewModels;
using NiagaraCollegeProject.Utilities;
using Microsoft.AspNetCore.Authorization;

namespace NiagaraCollegeProject.Controllers
{
    [Authorize(Roles = "Admin, Supervisor")]

    public class MeetingMinutesController : Controller
    {
        private readonly PAC_Context _context;
        private readonly IMyEmailSender _emailSender;

        public MeetingMinutesController(PAC_Context context,
            IMyEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: MeetingMinutes
        public async Task<IActionResult> Index(string sortDirectionCheck, string sortFieldID, int? page, int? pageSizeID, string SearchString, int? AcademicDivisionID,
            int? PACID, string actionButton, string sortDirection = "asc", string sortField = "Meeting")
        {
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);
            ViewData["Filtering"] = "btn-outline-primary";
            PopulateDropDownLists();

            var meetingMinutes = _context.MeetingMinutes
                .Include(m => m.MeetingMinuteDocuments)
                .Include(m => m.Meeting)
                .AsNoTracking();
            string[] sortOptions = new[] { "Meeting Comment ↕", "Meeting ↕", "Archive Status ↕" };

            if (AcademicDivisionID.HasValue)
            {
                meetingMinutes = meetingMinutes.Where(c => c.Meeting.AcademicDivisionID == AcademicDivisionID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (PACID.HasValue)
            {
                meetingMinutes = meetingMinutes.Where(c => c.Meeting.PACID == PACID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                meetingMinutes = meetingMinutes.Where(c => c.MeetingComment.ToUpper().Contains(SearchString.ToUpper()));
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

            if (sortField == "Meeting Comment ↕")
            {
                if (sortDirection == "asc")
                {
                    meetingMinutes = meetingMinutes
                        .OrderBy(p => p.MeetingComment);

                }
                else
                {
                    meetingMinutes = meetingMinutes
                        .OrderByDescending(p => p.MeetingComment);

                }
            }
            else if (sortField == "Meeting ↕")
            {
                if (sortDirection == "asc")
                {
                    meetingMinutes = meetingMinutes
                        .OrderBy(p => p.Meeting.MeetingTopicName);
                }
                else
                {
                    meetingMinutes = meetingMinutes
                        .OrderByDescending(p => p.Meeting.MeetingTopicName);

                }
            }
            else if (sortField == "Archive Status ↕")
            {
                if (sortDirection == "asc")
                {
                    meetingMinutes = meetingMinutes
                        .OrderByDescending(p => p.IsArchived);
                }
                else
                {
                    meetingMinutes = meetingMinutes
                        .OrderBy(p => p.IsArchived);
                }
            }

            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            //SelectList for Sorting Options
            ViewBag.sortFieldID = new SelectList(sortOptions, sortField.ToString());

            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName");
            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName");

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "Members");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<MeetingMinute>.CreateAsync(meetingMinutes.AsNoTracking(), page ?? 1, pageSize);

            if (User.IsInRole("Admin") || User.IsInRole("Supervisor"))
            {
                pagedData = await PaginatedList<MeetingMinute>.CreateAsync(meetingMinutes.AsNoTracking(), page ?? 1, pageSize);
            }


            return View(pagedData);
        }

        // GET: MeetingMinutes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewDataReturnURL();
            if (id == null || _context.MeetingMinutes == null)
            {
                return NotFound();
            }

            var meetingMinute = await _context.MeetingMinutes
                .Include(m => m.Meeting)
                .Include(m => m.MeetingMinuteDocuments)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (meetingMinute == null)
            {
                return NotFound();
            }

            return View(meetingMinute);
        }

        // GET: MeetingMinutes/Create
        public IActionResult Create()
        {
            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "MeetingTopicName");
            return View();
        }

        // POST: MeetingMinutes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,MeetingComment,IsArchived,MeetingID")] MeetingMinute meetingMinute, List<IFormFile> meetingMinuteFile)
        {
            if (ModelState.IsValid)
            {
                _context.Add(meetingMinute);
                await AddDocumentsAsync(meetingMinute, meetingMinuteFile);
                await Notification(meetingMinute.MeetingID);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "MeetingTopicName", meetingMinute.MeetingID);
            return View(meetingMinute);
        }

        // GET: MeetingMinutes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.MeetingMinutes == null)
            {
                return NotFound();
            }
            var meetingMinute = await _context.MeetingMinutes
                .Include(m => m.Meeting)
                .Include(m => m.MeetingMinuteDocuments)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (meetingMinute == null)
            {
                return NotFound();
            }
            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "MeetingTopicName", meetingMinute.MeetingID);
            return View(meetingMinute);
        }

        // POST: MeetingMinutes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,MeetingComment,IsArchived,MeetingID")] MeetingMinute meetingMinute, List<IFormFile> meetingMinuteFile)
        {
            ViewDataReturnURL();



            if (id != meetingMinute.ID)
            {
                return NotFound();
            }
            if (meetingMinute == null)
            {
                return NotFound(new { message = "Something went wrong looking for that meeting minute, please try again." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Something went wrong please try again." });
            }


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(meetingMinute);
                    await AddDocumentsAsync(meetingMinute, meetingMinuteFile);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MeetingMinuteExists(meetingMinute.ID))
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
            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "MeetingTopicName", meetingMinute.MeetingID);
            return View(meetingMinute);
        }

        // GET: MeetingMinutes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MeetingMinutes == null)
            {
                return NotFound();
            }

            var meetingMinute = await _context.MeetingMinutes
                .Include(m => m.Meeting)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (meetingMinute == null)
            {
                return NotFound();
            }

            return View(meetingMinute);
        }

        // POST: MeetingMinutes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MeetingMinutes == null)
            {
                return Problem("Entity set 'PAC_Context.MeetingMinutes'  is null.");
            }
            var meetingMinute = await _context.MeetingMinutes.FindAsync(id);
            if (meetingMinute != null)
            {
                _context.MeetingMinutes.Remove(meetingMinute);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public JsonResult GetPacList(int Id)
        {
            var result = _context.PAC
                .Where(r => r.AcademicDivisionID == Id).ToList();
            return Json(new SelectList(result, "ID", "PACName"));
        }
        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }
        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }
        private void PopulateDropDownLists(AcademicDivision division = null, PAC pac = null)
        {
            ViewData["AcademicDivisionID"] = AcademicDivisionSelectList(division?.ID);
            ViewData["PACID"] = PACSelectList(pac?.ID);
        }
        private SelectList AcademicDivisionSelectList(int? selectedID)
        {
            return new SelectList(_context.AcademicDivisions
                .OrderBy(c => c.DivisionName), "ID", "DivisionName", selectedID);
        }
        public async Task<FileContentResult> Download(int id)
        {
            var theFile = await _context.UploadedFile
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .FirstOrDefaultAsync();
            return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
        }

        private async Task AddDocumentsAsync(MeetingMinute meetingMinute, List<IFormFile> meetingMinuteFile)
        {
            foreach (var f in meetingMinuteFile)
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
                        MeetingMinuteDocuments d = new MeetingMinuteDocuments();
                        using (var memoryStream = new MemoryStream())
                        {
                            await f.CopyToAsync(memoryStream);
                            d.FileContent.Content = memoryStream.ToArray();
                        }
                        d.MimeType = mimeType;
                        d.FileName = fileName;
                        meetingMinute.MeetingMinuteDocuments.Add(d);
                    };
                }
            }
        }
        public async Task<IActionResult> Notification(int id)
        {

            var meeting = _context.Meeting.Find(id);


            var members = _context.MeetingAttendees
                .Include(x => x.Member)
                .Where(m => m.MeetingID == id).ToList();



            int folksCount = 0;

            List<EmailAddress> folks = (from p in members
                                        where p.MeetingID == id
                                        select new EmailAddress
                                        {
                                            Name = p.Member.FullName,
                                            Address = p.Member.Email
                                        }).ToList();
            folksCount = folks.Count();
            if (folksCount > 0)
            {
                var msg = new EmailMessage()
                {
                    ToAddresses = folks,
                    Subject = "New Meeting Minutes Uploadded for: " + meeting.MeetingTopicName,
                    Content = MeetingMinutesEmailTemplate.MeetingMinTemp(meeting.MeetingTopicName)
                };
                await _emailSender.SendToManyAsync(msg);

            }

            TempData["notificationSent"] = $"{folks.Count()}" + " Member(s) have been notified via email about the meeting minutes uploaded.";
            return View();

        }

        private SelectList PACSelectList(int? selectedID)
        {
            return new SelectList(_context.PAC
                .OrderBy(a => a.PACName), "ID", "PACName", selectedID);
        }

        private bool MeetingMinuteExists(int id)
        {
            return _context.MeetingMinutes.Any(e => e.ID == id);
        }
    }
}