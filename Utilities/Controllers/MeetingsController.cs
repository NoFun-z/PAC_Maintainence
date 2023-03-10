using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;
using NiagaraCollegeProject.Utilities;
using NiagaraCollegeProject.ViewModels;
using static iTextSharp.text.pdf.AcroFields;

namespace NiagaraCollegeProject.Controllers
{
    [Authorize]
    public class MeetingsController : Controller
    {
        private readonly PAC_Context _context;

        public MeetingsController(PAC_Context context)
        {
            _context = context;
        }

        // GET: Meetings
        public async Task<IActionResult> Index(int? page, int? pageSizeID)
        {

            var pAC_Context = _context.Meeting
                .Include(d => d.MeetingDocuments)
                .Include(d => d.AgendaItems)
                .Include(m => m.AcademicDivision).ThenInclude(m => m.PACs)
                .Include(d => d.MeetingMinute)
                .Include(d => d.Attendees).ThenInclude(m=>m.Member)
                .AsNoTracking();

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "Meetings");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Meeting>.CreateAsync(pAC_Context.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Meetings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
          
            if (id == null || _context.Meeting == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meeting
                .Include(d => d.MeetingDocuments)
                .Include(d => d.AgendaItems)
                .Include(m => m.AcademicDivision).ThenInclude(m => m.PACs)
                .Include(d => d.MeetingMinute)
                .Include(d => d.Attendees)
                .ThenInclude(m => m.Member)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (meeting == null)
            {
                return NotFound();
            }

            return View(meeting);
        }

        // GET: Meetings/Create
        [Authorize(Roles = "Admin, Supervisor")]
        public IActionResult Create()
        {
            var meeting = new Meeting();

            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName");
            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName");
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName");
            PopulateAssignedAttendeesData(meeting);
            return View();
        }
        public JsonResult GetPacList(int Id)
        {
            var result = _context.PAC
                .Where(r => r.AcademicDivisionID == Id).ToList();
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
            try
            {
                UpdateMeetingAttendees(selectedOptions, meeting);
                if (ModelState.IsValid)
                {
                    _context.Add(meeting);
                    await _context.SaveChangesAsync();
                    //Send on to add attendees
                    return RedirectToAction("Index", "MeetingAttendees", new { MeetingID = meeting.ID });
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
            PopulateAssignedAttendeesData(meeting);
            return View(meeting);
        }

        // GET: Meetings/Edit/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Meeting == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meeting
                .Include(d => d.MeetingDocuments)
                .Include(d => d.AgendaItems)
                .Include(m => m.AcademicDivision).ThenInclude(m => m.PACs)
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
            PopulateAssignedAttendeesData(meeting);
            return View(meeting);
        }

        // POST: Meetings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Supervisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("ID,MeetingTopicName,PACID,MemberID,AcademicDivisionID,MeetingStartTimeDate," +
            "MeetingMinutes,MeetingNotes")]List<IFormFile> theFiles, string[] selectedOptions)
        {

            var meetingToUpdate = await _context.Meeting
               .Include(d => d.MeetingDocuments)
               .Include(d => d.AgendaItems)
               .Include(m => m.AcademicDivision).ThenInclude(m => m.PACs)
               .Include(d => d.MeetingMinute)
               .Include(d => d.Attendees)
               .ThenInclude(m => m.Member)
               .FirstOrDefaultAsync(m => m.ID == id);

            //Check that you got it or exit with a not found error
            if (meetingToUpdate == null)
            {
                return NotFound();
            }

            UpdateMeetingAttendees(selectedOptions, meetingToUpdate);


                try
                {
                    await _context.SaveChangesAsync();
                //Send on to add attendees
                return RedirectToAction("Index", "MeetingAttendees", new { MeetingID = meetingToUpdate.ID });
            }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
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
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem continues see your system admin.");
                }


            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName");
            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName");
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName");
            PopulateAssignedAttendeesData(meetingToUpdate);
            return View(meetingToUpdate);
        }


        // GET: Meetings/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
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

        private void PopulateAssignedAttendeesData(Meeting meeting)
        {

            var allOptions = _context.Members;
            var currentOptionIDs = new HashSet<int>(meeting.Attendees.Select(b => b.MemberID));
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();
            foreach (var m in allOptions)
            {
                if (currentOptionIDs.Contains(m.ID))
                {
                    selected.Add(new ListOptionVM
                    {
                        ID = m.ID,
                        DisplayText = m.FullName
                    });
                }
                else
                {
                    available.Add(new ListOptionVM
                    {
                        ID = m.ID,
                        DisplayText = m.FullName
                    });
                }
            }
            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }

        public async Task<IActionResult> GenerateReport(int? id)
        {
            var meeting = await _context.Meeting
                .Include(m => m.AgendaItems)
                .Include(m => m.Attendees).ThenInclude(m=>m.Member)
                .Include(m=>m.AcademicDivision)
                .Include(m=>m.PAC)
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
                attendeeName.HorizontalAlignment= Element.ALIGN_CENTER;
                attendeeName.VerticalAlignment= Element.ALIGN_CENTER;
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
                    if(attendees[i].Member.PreferredContact == Member.Contact.Work)
                    {
                        email = attendees[i].Member.CompanyEmail;
                        phone = attendees[i].Member.CompanyPhoneNumber;
                    }
                    else if(attendees[i].Member.PreferredContact == Member.Contact.Personal)
                    {
                        email = attendees[i].Member.Email;
                        phone = attendees[i].Member.PhoneNumber;
                    }
                    PdfPCell name_cell = new PdfPCell(new Phrase(attendees[i].Member.FullName));
                    name_cell.HorizontalAlignment= Element.ALIGN_CENTER;
                    table.AddCell(name_cell);
                    PdfPCell contact_cell = new PdfPCell(new Phrase($"{phone},{email}"));
                    contact_cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(contact_cell);
                }
                var agendaItems = meeting.AgendaItems.ToList();

                for (int i = 0; i < meeting.AgendaItems.Count; i++)
                {
                    var member = _context.Members.Find(agendaItems[i].MemberID);
                    PdfPCell agenda_items = new PdfPCell(new Phrase(agendaItems[i].AgendaName));
                    agenda_items.HorizontalAlignment = Element.ALIGN_CENTER;
                    agendaTable.AddCell(agenda_items);
                    PdfPCell agenda_owner = new PdfPCell(new Phrase(member.FullName));
                    agenda_owner.HorizontalAlignment = Element.ALIGN_CENTER;
                    agendaTable.AddCell(agenda_owner);
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
