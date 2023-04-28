using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;
using NiagaraCollegeProject.ViewModels;
using NiagaraCollegeProject.Utilities;
using static iTextSharp.text.pdf.AcroFields;
using System.Collections;

namespace NiagaraCollegeProject.Controllers
{
    [Authorize]
    public class MeetingsController : Controller
    {
        private readonly PAC_Context _context;
        private readonly IMyEmailSender _emailSender;

        public MeetingsController(PAC_Context context,
            IMyEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: Meetings
        public async Task<IActionResult> Index(string sortDirectionCheck, string sortFieldID, int? page, int? pageSizeID, 
            DateTime? startDate, DateTime? endDate, int? AcademicDivisionID, int? PACID, string successMessage, string actionButton,
            string sortDirection = "asc", string sortField = "Meeting")
        {
            if (!string.IsNullOrEmpty(successMessage))
            {
                ViewBag.SuccessMessage = successMessage;
            }

            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);
            ViewData["Filtering"] = "btn-outline-primary";
            PopulateDropDownLists();


            var PAC_Context = _context.Meetings
                .Include(m => m.MeetingDocuments)
                .Include(m => m.AcademicDivision)
                .Include(m => m.PAC)
                .Include(m => m.MeetingMinute)
                .Include(m => m.Member)
                .Include(m => m.Attendees).ThenInclude(m => m.Member)
                .Where(m => m.IsArchived == false)
                .AsNoTracking();


            string[] sortOptions = new[] {"MEETING DATE/START TIME ↕", "PAC ↕", "ATTENDEES ↕" };


            if (AcademicDivisionID.HasValue)
            {
                PAC_Context = PAC_Context.Where(c => c.AcademicDivisionID == AcademicDivisionID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (PACID.HasValue)
            {
                PAC_Context = PAC_Context.Where(c => c.PACID == PACID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (startDate.HasValue)
            {
                DateTime start = startDate.Value.Date;
                PAC_Context = PAC_Context.Where(m => m.MeetingStartTimeDate >= start);
                TempData["StartDate"] = startDate.Value.ToString("yyyy-MM-dd");
                ViewData["Filtering"] = "btn-danger";
            }
            if (endDate.HasValue)
            {
                DateTime end = endDate.Value.Date.AddDays(1); // add one day to include the end date
                PAC_Context = PAC_Context.Where(m => m.MeetingStartTimeDate < end);
                TempData["EndDate"] = endDate.Value.ToString("yyyy-MM-dd");
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
                    PAC_Context = PAC_Context
                        .OrderBy(p => p.MeetingStartTimeDate);
                }
                else
                {
                    PAC_Context = PAC_Context
                        .OrderByDescending(p => p.MeetingStartTimeDate);
                }
            }
            else if (sortField == "PAC ↕")
            {
                if (sortDirection == "asc")
                {
                    PAC_Context = PAC_Context
                        .OrderBy(p => p.PAC.PACName);
                }
                else
                {
                    PAC_Context = PAC_Context
                        .OrderByDescending(p => p.PAC.PACName);
                }
            }
            else if (sortField == "ATTENDEES ↕")
            {
                if (sortDirection == "asc")
                {
                    PAC_Context = PAC_Context
                        .OrderByDescending(p => p.Attendees.Count());
                }
                else
                {
                    PAC_Context = PAC_Context
                        .OrderBy(p => p.Attendees.Count());
                }

            }
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            //SelectList for Sorting Options
            ViewBag.sortFieldID = new SelectList(sortOptions, sortField.ToString());

            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName");
            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName");
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "Meetings");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Meeting>.CreateAsync(PAC_Context.AsNoTracking(), page ?? 1, pageSize);

            if (User.IsInRole("Admin") || User.IsInRole("Supervisor"))
            {
                pagedData = await PaginatedList<Meeting>.CreateAsync(PAC_Context.AsNoTracking(), page ?? 1, pageSize);
            }
            if (User.IsInRole("Staff"))
            {
                int? filteredPac = _context.Members.Where(m => m.Email == User.Identity.Name).FirstOrDefault().PACID;
                var filteredMeetings = PAC_Context.Where(m => m.PACID == filteredPac);
                var FilteredMeetings = filteredMeetings.Where(m => m.Attendees.Any(a => a.Member.Email == User.Identity.Name));
                pagedData = await PaginatedList<Meeting>.CreateAsync(FilteredMeetings.AsNoTracking(), page ?? 1, pageSize);
            }

            return View(pagedData);
        }

        // GET: Meetings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.Meeting == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meeting
                .Include(d => d.MeetingDocuments)
                .Include(m => m.AcademicDivision)
                .Include(m => m.PAC)
                .Include(d => d.MeetingMinute)
                .Include(d => d.Member)
                .Include(d => d.Attendees)
                .ThenInclude(m => m.Member)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (meeting == null)
            {
                return NotFound();
            }


            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName");
            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName");
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName");

            return View(meeting);
        }

        // GET: Meetings/Create
        [Authorize(Roles = "Admin, Supervisor")]
        public IActionResult Create()
        {
            ViewDataReturnURL();

            int pacid = 2;

            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName");
            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName");
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName");
            PopulateCreateAttendeesData(pacid);
            return View();
        }
        public JsonResult GetPacList(int Id)
        {
            var result = _context.PAC
                   .Where(r => r.AcademicDivisionID == Id).OrderBy(a => a.PACName).ToList();
            return Json(new SelectList(result, "ID", "PACName"));
        }
        // POST: Meetings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Supervisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,MeetingTopicName,PACID,MemberID,AcademicDivisionID,MeetingStartTimeDate," +
            "MeetingMinutes,MeetingNotes")] Meeting meeting, List<IFormFile> theFiles, string[] selectedOptions)
        {
            ViewDataReturnURL();

            try
            {
                UpdateMeetingAttendees(selectedOptions, meeting);
                if (ModelState.IsValid)
                {
                    _context.Add(meeting);
                    await AddDocumentsAsync(meeting, theFiles);
                    await _context.SaveChangesAsync();
                    //Check to see if any documents have been added during the create process, then send notifications
                    if (_context.MeetingDocuments.Where(m => m.MeetingID == meeting.ID).Count() > 0)
                    {
                        await Notification(meeting.ID);
                    }
                    //Send on to add attendees
                    TempData["SuccessMessage"] = "Meeting created successfully!";
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem continues see your system admin.");
            }
            PopulateAssignedAttendeesData(meeting.PACID);
            return View(meeting);
        }

        // GET: Meetings/Edit/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Edit(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.Meeting == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meeting
                .Include(d => d.MeetingDocuments)
                .Include(m => m.AcademicDivision)
                .Include(m => m.PAC)
                .Include(d => d.MeetingMinute)
                .Include(d => d.Attendees)
                .ThenInclude(m => m.Member)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (meeting == null)
            {
                return NotFound();
            }

            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName");
            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName");
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName");
            PopulateAssignedAttendeesData(meeting.PACID);
            return View(meeting);
        }

        // POST: Meetings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Supervisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, List<IFormFile> theFiles, string[] selectedOptions)
        {
            ViewDataReturnURL();

            var meetingToUpdate = await _context.Meeting
               .Include(d => d.MeetingDocuments)
               .Include(m => m.AcademicDivision)
               .Include(m => m.PAC)
               .Include(d => d.MeetingMinute)
               .Include(d => d.Attendees)
               .ThenInclude(m => m.Member)
               .FirstOrDefaultAsync(m => m.ID == id);

            //Check that you got it or exit with a not found error
            if (meetingToUpdate == null)
            {
                return NotFound(new { message = "Something went wrong looking for that meeting, please try again" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Something went wrong please try again." });
            }
            UpdateMeetingAttendees(selectedOptions, meetingToUpdate);


            if (await TryUpdateModelAsync<Meeting>(meetingToUpdate, "",
                d => d.MeetingTopicName, d => d.MeetingStartTimeDate, d => d.MeetingNotes, d => d.MemberID,
                d => d.AcademicDivisionID, d => d.PACID, d => d.MeetingMinute))
                try
                {
                    int currentMinutes = _context.MeetingDocuments.Where(m => m.MeetingID == meetingToUpdate.ID).Count();

                    await AddDocumentsAsync(meetingToUpdate, theFiles);
                    await _context.SaveChangesAsync();
                    //Check to see if any documents have been added during the create process, then send notifications
                    if (_context.MeetingDocuments.Where(m => m.MeetingID == meetingToUpdate.ID).Count() > currentMinutes)
                    {
                        await NotificationE(meetingToUpdate.ID);
                    }
                    //Send on to add attendees
                    TempData["SuccessMessage"] = "Meeting edited successfully!";
                    return RedirectToAction("Index");

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MeetingExists(meetingToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (ArgumentNullException)
                {
                    ModelState.AddModelError("", "Some fields are missing please make sure all required data is supplied.");
                    return View(meetingToUpdate);
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes please try again, if this continues contact your admin.");
                    return View(meetingToUpdate);
                }


            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName");
            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName");
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName");
            PopulateAssignedAttendeesData(meetingToUpdate.PACID);
            return View(meetingToUpdate);
        }


        public async Task<IActionResult> Archive(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.Meeting == null)
            {
                return NotFound();
            }

            var meetings = await _context.Meeting
               .Include(d => d.MeetingDocuments)
               .Include(m => m.AcademicDivision)
               .Include(m => m.PAC)
               .Include(d => d.MeetingMinute)
               .Include(d => d.Attendees)
               .ThenInclude(m => m.Member)
               .AsNoTracking()
               .FirstOrDefaultAsync(m => m.ID == id);

            if (meetings == null)
            {
                return NotFound();
            }

            return View(meetings);
        }

        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            ViewDataReturnURL();

            var meetings = await _context.Meeting
               .Include(d => d.MeetingDocuments)
               .Include(m => m.AcademicDivision)
               .Include(m => m.PAC)
               .Include(d => d.MeetingMinute)
               .Include(d => d.Attendees)
               .ThenInclude(m => m.Member)
               .AsNoTracking()
               .FirstOrDefaultAsync(m => m.ID == id);

            if (meetings == null)
            {
                return NotFound();
            }

            if (meetings != null)
            {
                meetings.IsArchived = true;

                _context.Entry(meetings).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Meeting archived successfully!";
            }

            //var members = membe

            return Redirect($"/Meetings/Index/{id}");
        }

        // GET: Meetings/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.Meeting == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meeting
                .FirstOrDefaultAsync(m => m.ID == id);
            if (meeting == null)
            {
                return NotFound();
            }

            return View(meeting);
        }

        // POST: Meetings/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            ViewDataReturnURL();

            if (_context.Meeting == null)
            {
                return Problem("Entity set 'PAC_Context.Meeting'  is null.");
            }
            var meeting = await _context.Meeting.FindAsync(id);
            if (meeting != null)
            {
                _context.Meeting.Remove(meeting);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Meeting deleted successfully!";
            return RedirectToAction("Index");
        }

        public async Task<FileContentResult> Download(int id)
        {
            var theFile = await _context.UploadedFile
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .FirstOrDefaultAsync();
            return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
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
                    Subject = "New Meeting Minutes Uploaded for: " + meeting.MeetingTopicName,
                    Content = MeetingMinutesEmailTemplate.MeetingMinTemp(meeting.MeetingTopicName)
                };
                await _emailSender.SendToManyAsync(msg);

            }

            TempData["notificationSent"] = $"{folks.Count()}" + " Member(s) have been notified via email about the meeting minutes uploaded.";
            return View();

        }

        public async Task<IActionResult> NotificationE(int id)
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
                    Subject = "The Meeting Minutes have been modified for: " + meeting.MeetingTopicName,
                    Content = MeetingMinutesEmailTemplateEdit.MeetingMinTemp(meeting.MeetingTopicName)
                };
                await _emailSender.SendToManyAsync(msg);

            }

            TempData["notificationSent"] = $"{folks.Count()}" + " Member(s) have been notified via email about the meeting minutes modification.";
            return View();

        }

        private async Task AddDocumentsAsync(Meeting meeting, List<IFormFile> theFiles)
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
                        MeetingDocuments d = new MeetingDocuments();
                        using (var memoryStream = new MemoryStream())
                        {
                            await f.CopyToAsync(memoryStream);
                            d.FileContent.Content = memoryStream.ToArray();
                        }
                        d.MimeType = mimeType;
                        d.FileName = fileName;
                        meeting.MeetingDocuments.Add(d);
                    };
                }
            }
        }

        private bool MeetingExists(int? id)
        {
            return _context.Meeting.Any(e => e.ID == id);
        }

        public JsonResult PopulateCreateAttendeesData(int? pacID)
        {
            var allOptions = _context.Members.Where(m => m.PACID == pacID);
            //if (pacID == null)
            //{
            //    allOptions = _context.Members.Where(m => m.PACID == meeting.PACID);
            //}
            //else
            //{
            //    allOptions = _context.Members.Where(m => m.PACID == pacID);
            //}
            var currentOptionIDs = new HashSet<int>(_context.Meeting.Where(m => m.PACID == pacID).SelectMany(m => m.Attendees).Select(a => a.MemberID));
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();
            foreach (var m in allOptions)
            {
                    available.Add(new ListOptionVM
                    {
                        ID = m.ID,
                        DisplayText = m.FullName + " Role: " + m.MemberRole
                    });
            }
            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            var selOpts = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            var availOpts = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");

            return Json(new { selOpts, availOpts });
        }

        public JsonResult PopulateAssignedAttendeesData(int? pacID)
        {
            var allOptions = _context.Members.Where(m => m.PACID == pacID);
            var currentOptionIDs = new HashSet<int>(_context.Meeting.Where(m => m.PACID == pacID).SelectMany(m => m.Attendees).Select(a => a.MemberID));
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();
            foreach (var m in allOptions)
            {
                if (currentOptionIDs.Contains(m.ID))
                {
                    selected.Add(new ListOptionVM
                    {
                        ID = m.ID,
                        DisplayText = m.FullName + " Role: " + m.MemberRole
                    });
                }
                else
                {
                    available.Add(new ListOptionVM
                    {
                        ID = m.ID,
                        DisplayText = m.FullName + " Role: " + m.MemberRole
                    });
                }
            }
            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            var selOpts = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            var availOpts = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");

            return Json(new { selOpts, availOpts });
        }

        public async Task<IActionResult> GenerateReport(int? id)
        {
            var meeting = await _context.Meeting
                .Include(m => m.Attendees).ThenInclude(m => m.Member)
                .Include(m => m.AcademicDivision)
                .Include(m => m.PAC)
                .FirstOrDefaultAsync(m => m.ID == id);


            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                var img = iTextSharp.text.Image.GetInstance("wwwroot/images/nc-picture.jpg");
                img.ScaleAbsolute(50f, 50f);
                img.Alignment = iTextSharp.text.Image.TEXTWRAP | iTextSharp.text.Image.ALIGN_LEFT;

                Paragraph AcademicDivision = new Paragraph("Media, Trades & Technology Division School of Technology", new Font(Font.FontFamily.HELVETICA, 18));
                AcademicDivision.SpacingAfter = 15f;
                AcademicDivision.Alignment = Element.ALIGN_CENTER;


                Paragraph StartTime = new Paragraph($"Date: {meeting.MeetingStartTimeDate}", new Font(Font.FontFamily.HELVETICA, 12));
                StartTime.SpacingAfter = 15f;
                StartTime.Alignment = Element.ALIGN_CENTER;

                Paragraph attendeeHeader = new Paragraph("In Attendees", new Font(Font.FontFamily.HELVETICA, 12));
                attendeeHeader.SpacingAfter = 15f;
                attendeeHeader.Alignment = Element.ALIGN_CENTER;


                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 100;

                PdfPCell attendeeName = new PdfPCell(new Phrase("Attendee", new Font(Font.FontFamily.HELVETICA, 10)));
                attendeeName.BackgroundColor = BaseColor.LIGHT_GRAY;
                attendeeName.Border = Rectangle.BOTTOM_BORDER | Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
                attendeeName.BorderWidthRight = 1f;
                attendeeName.BorderWidthTop = 1f;
                attendeeName.BorderWidthLeft = 1f;
                attendeeName.BorderWidthBottom = 1f;
                attendeeName.HorizontalAlignment = Element.ALIGN_CENTER;
                attendeeName.VerticalAlignment = Element.ALIGN_CENTER;
                PdfPCell attendeeContact = new PdfPCell(new Phrase("Contact Info", new Font(Font.FontFamily.HELVETICA, 10)));
                attendeeContact.BackgroundColor = BaseColor.LIGHT_GRAY;
                attendeeContact.Border = Rectangle.BOTTOM_BORDER | Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
                attendeeContact.BorderWidthRight = 1f;
                attendeeContact.BorderWidthTop = 1f;
                attendeeContact.BorderWidthLeft = 1f;
                attendeeContact.BorderWidthBottom = 1f;
                attendeeContact.HorizontalAlignment = Element.ALIGN_CENTER;
                attendeeContact.VerticalAlignment = Element.ALIGN_CENTER;
                table.AddCell(attendeeName);
                table.AddCell(attendeeContact);

                Paragraph agendaHeader = new Paragraph("Agenda", new Font(Font.FontFamily.HELVETICA, 12));
                agendaHeader.SpacingAfter = 15f;
                agendaHeader.Alignment = Element.ALIGN_CENTER;

                PdfPTable agendaTable = new PdfPTable(2);
                agendaTable.WidthPercentage = 100;

                PdfPCell agendaItem = new PdfPCell(new Phrase("Agenda Item", new Font(Font.FontFamily.HELVETICA, 10)));
                agendaItem.BackgroundColor = BaseColor.LIGHT_GRAY;
                agendaItem.Border = Rectangle.BOTTOM_BORDER | Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
                agendaItem.BorderWidthRight = 1f;
                agendaItem.BorderWidthTop = 1f;
                agendaItem.BorderWidthLeft = 1f;
                agendaItem.BorderWidthBottom = 1f;
                agendaItem.HorizontalAlignment = Element.ALIGN_CENTER;
                agendaItem.VerticalAlignment = Element.ALIGN_CENTER;
                PdfPCell agendaOwner = new PdfPCell(new Phrase("Assigned To", new Font(Font.FontFamily.HELVETICA, 10)));
                agendaOwner.BackgroundColor = BaseColor.LIGHT_GRAY;
                agendaOwner.Border = Rectangle.BOTTOM_BORDER | Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
                agendaOwner.BorderWidthRight = 1f;
                agendaOwner.BorderWidthTop = 1f;
                agendaOwner.BorderWidthLeft = 1f;
                agendaOwner.BorderWidthBottom = 1f;
                agendaOwner.HorizontalAlignment = Element.ALIGN_CENTER;
                agendaOwner.VerticalAlignment = Element.ALIGN_CENTER;
                agendaTable.AddCell(agendaItem);
                agendaTable.AddCell(agendaOwner);


                Paragraph actionHeader = new Paragraph("TakeAways", new Font(Font.FontFamily.HELVETICA, 12));
                actionHeader.SpacingAfter = 15f;
                actionHeader.Alignment = Element.ALIGN_CENTER;


                PdfPTable actionTable = new PdfPTable(2);
                actionTable.WidthPercentage = 100;

                PdfPCell actionItem = new PdfPCell(new Phrase("Action Item", new Font(Font.FontFamily.HELVETICA, 10)));
                actionItem.BackgroundColor = BaseColor.LIGHT_GRAY;
                actionItem.Border = Rectangle.BOTTOM_BORDER | Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
                actionItem.BorderWidthRight = 1f;
                actionItem.BorderWidthTop = 1f;
                actionItem.BorderWidthLeft = 1f;
                actionItem.BorderWidthBottom = 1f;
                actionItem.HorizontalAlignment = Element.ALIGN_CENTER;
                actionItem.VerticalAlignment = Element.ALIGN_CENTER;
                PdfPCell actionOwner = new PdfPCell(new Phrase("Assigned To", new Font(Font.FontFamily.HELVETICA, 10)));
                actionOwner.BackgroundColor = BaseColor.LIGHT_GRAY;
                actionOwner.Border = Rectangle.BOTTOM_BORDER | Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
                actionOwner.BorderWidthRight = 1f;
                actionOwner.BorderWidthTop = 1f;
                actionOwner.BorderWidthLeft = 1f;
                actionOwner.BorderWidthBottom = 1f;
                actionOwner.HorizontalAlignment = Element.ALIGN_CENTER;
                actionOwner.VerticalAlignment = Element.ALIGN_CENTER;
                actionTable.AddCell(actionItem);
                actionTable.AddCell(actionOwner);

                var attendees = meeting.Attendees.ToList();
                for (int i = 0; i < meeting.Attendees.Count; i++)
                {
                    string email = "";
                    string phone = "";
                    if (attendees[i].Member.PreferredContact == Member.Contact.Work)
                    {
                        email = attendees[i].Member.CompanyEmail;
                        phone = attendees[i].Member.CompanyPhoneNumber;
                    }
                    else if (attendees[i].Member.PreferredContact == Member.Contact.Personal)
                    {
                        email = attendees[i].Member.Email;
                        phone = attendees[i].Member.PhoneNumber;
                    }
                    PdfPCell name_cell = new PdfPCell(new Phrase(attendees[i].Member.FullName));
                    name_cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(name_cell);
                    PdfPCell contact_cell = new PdfPCell(new Phrase($"{phone},{email}"));
                    contact_cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(contact_cell);
                }

                var actionContext = _context.ActionItems
                .Where(m => m.MeetingID == meeting.ID).ToList();

                for (int i = 0; i < actionContext.Count; i++)
                {
                    var member = _context.Members.Find(actionContext[i].MemberID);
                    PdfPCell actionItems = new PdfPCell(new Phrase(actionContext[i].AgendaName));
                    actionItems.HorizontalAlignment = Element.ALIGN_CENTER;
                    actionTable.AddCell(actionItems);
                    PdfPCell actionOwners = new PdfPCell(new Phrase(member.FullName));
                    actionOwners.HorizontalAlignment = Element.ALIGN_CENTER;
                    actionTable.AddCell(actionOwners);
                }

                Paragraph closingHeader = new Paragraph("Upcoming meeting notifications will sent via your preferred contact Email", new Font(Font.FontFamily.HELVETICA, 20));
                closingHeader.SpacingBefore = 25f;
                closingHeader.SpacingAfter = 15f;
                closingHeader.Alignment = Element.ALIGN_CENTER;


                document.Add(img);
                document.Add(AcademicDivision);
                document.Add(StartTime);
                document.Add(attendeeHeader);
                document.Add(table);
                document.Add(agendaHeader);
                document.Add(agendaTable);
                document.Add(actionHeader);
                document.Add(actionTable);
                document.Add(closingHeader);

                document.Close();
                writer.Close();
                var constant = ms.ToArray();
                return File(constant, "application/vnd", "MeetingMinuteReport.pdf");
            }
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }
        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
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
        private void UpdateMeetingAttendees(string[] selectedOptions, Meeting meetingToUpdate)
        {
            if (selectedOptions == null)
            {
                meetingToUpdate.Attendees = new List<MeetingAttendees>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var currentOptionsHS = new HashSet<int>(meetingToUpdate.Attendees.Select(b => b.MemberID));
            foreach (var m in _context.Members)
            {
                if (selectedOptionsHS.Contains(m.ID.ToString()))
                {
                    if (!currentOptionsHS.Contains(m.ID))
                    {
                        meetingToUpdate.Attendees.Add(new MeetingAttendees
                        {
                            MemberID = m.ID,
                            MeetingID = meetingToUpdate.ID
                        });
                    }
                }
                else
                {
                    if (currentOptionsHS.Contains(m.ID))
                    {
                        MeetingAttendees memberToRemove = meetingToUpdate.Attendees.FirstOrDefault(d => d.MemberID == m.ID);
                        _context.Remove(memberToRemove);
                    }
                }
            }
        }

    }
}