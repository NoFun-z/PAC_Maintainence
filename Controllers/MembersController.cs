using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;
using NiagaraCollegeProject.Utilities;
using NuGet.Protocol.Plugins;

namespace NiagaraCollegeProject.Controllers
{
    [Authorize(Roles = "Admin, Supervisor, Staff")]
    public class MembersController : Controller
    {
        private readonly PAC_Context _context;

        public MembersController(PAC_Context context)
        {
            _context = context;
        }

        // GET: Members
        public async Task<IActionResult> Index(string sortDirectionCheck, string sortFieldID, int? page, int? pageSizeID, string SearchString, int? AcademicDivisionID,
            int? PACID, string actionButton, string sortDirection = "asc", string sortField = "Member")
        {

            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = "btn-outline-primary"; //Assume not filtering
            //Then in each "test" for filtering, add ViewData["Filtering"] = " show" if true;

            PopulateDropDownLists();

            var PAC_Context = _context.Members
                .Include(m => m.Subscriptions)
                .Include(m => m.ActionItems)
                .Include(m => m.PAC)
                .Include(m => m.AcademicDivision)
                .Include(m => m.Salutation)
                .Include(d => d.MemberDocuments)
                .Where(m => m.Email != User.Identity.Name && m.IsArchived == false /*&& m.MemberStatus == true*/)
                .AsNoTracking();

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "FIRST NAME ↕", "LAST NAME ↕", "PAC ↕", "EMAIL ↕"};

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
            if (!String.IsNullOrEmpty(SearchString))
            {
                PAC_Context = PAC_Context.Where(c => c.LastName.ToUpper().Contains(SearchString.ToUpper())
                || c.FirstName.ToUpper().Contains(SearchString.ToUpper()));
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

            //Now we know which field and direction to sort by
            if (sortField == "LAST NAME ↕")
            {
                if (sortDirection == "asc")
                {
                    PAC_Context = PAC_Context
                        .OrderBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);
                }
                else
                {
                    PAC_Context = PAC_Context
                        .OrderByDescending(p => p.LastName)
                        .ThenByDescending(p => p.FirstName);
                }
            }
            else if (sortField == "FIRST NAME ↕")
            {
                if (sortDirection == "asc")
                {
                    PAC_Context = PAC_Context
                        .OrderBy(p => p.FirstName)
                        .ThenBy(p => p.LastName);
                }
                else
                {
                    PAC_Context = PAC_Context
                        .OrderByDescending(p => p.FirstName)
                        .ThenByDescending(p => p.LastName);
                }
            }
            else if (sortField == "PAC ↕")
            {
                if (sortDirection == "asc")
                {
                    PAC_Context = PAC_Context
                        .OrderByDescending(p => p.PAC.PACName);
                }
                else
                {
                    PAC_Context = PAC_Context
                        .OrderBy(p => p.PAC.PACName);
                }
            }
            else if (sortField == "EMAIL ↕")
            {
                if (sortDirection == "asc")
                {
                    PAC_Context = PAC_Context
                        .OrderBy(p => p.Email);
                }
                else
                {
                    PAC_Context = PAC_Context
                        .OrderByDescending(p => p.Email);
                }
            }

            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            //SelectList for Sorting Options
            ViewBag.sortFieldID = new SelectList(sortOptions, sortField.ToString());

            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName");
            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName");

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "Members");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Member>.CreateAsync(PAC_Context.AsNoTracking(), page ?? 1, pageSize);

            if (User.IsInRole("Admin") || User.IsInRole("Supervisor"))
            {
                pagedData = await PaginatedList<Member>.CreateAsync(PAC_Context.AsNoTracking(), page ?? 1, pageSize);
            }
            if (User.IsInRole("Staff"))
            {
                int? filteredPac = _context.Members.Where(m => m.Email == User.Identity.Name).FirstOrDefault().PACID;
                var filteredMembers = PAC_Context.Where(m => m.PACID == filteredPac);
                pagedData = await PaginatedList<Member>.CreateAsync(filteredMembers.AsNoTracking(), page ?? 1, pageSize);
            }

            return View(pagedData);
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            if (id == null || _context.Members == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.Subscriptions)
                .Include(m => m.ActionItems)
                .Include(m => m.PAC)
                .Include(m => m.AcademicDivision)
                .Include(m => m.Salutation)
                .Include(d => d.MemberPhoto)
                .Include(d => d.MemberDocuments)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // GET: Members/Create
        public IActionResult Create()
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName");
            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName");
            ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle");
            return View();
        }

        // POST: Members/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstName,LastName,PhoneNumber,Email,EducationSummary," +
            "OccupationalSummary,StreetAddress," +
            "Province,City,PostalCode," +
            "NCGraduate,SignUpDate,ReNewDate_,RenewalDueBy," +
            "CompanyName,CompanyStreetAddress,CompanyProvince,CompanyCity," +
            "CompanyPostalCode,CompanyPhoneNumber,CompanyEmail,PreferredContact," +
            "CompanyPositionTitle,AcademicDivisionID,PACID,SalutationID, Subscriptions,MemberRole")] Member member, IFormFile thePicture, List<IFormFile> theFiles)
        {
            if (ModelState.IsValid)
            {
                await AddPicture(member, thePicture);
                _context.Add(member);
                await AddDocumentsAsync(member, theFiles);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName", member.AcademicDivisionID);
            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName", member.PACID);
            ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", member.SalutationID);
            return View(member);
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            if (id == null || _context.Members == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.Subscriptions)
                .Include(m => m.ActionItems)
                .Include(m => m.PAC)
                .Include(m => m.AcademicDivision)
                .Include(m => m.Salutation)
                .Include(d => d.MemberPhoto)
                .Include(d => d.MemberDocuments)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (member == null)
            {
                return NotFound();
            }
            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName", member.AcademicDivisionID);
            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName", member.PACID);
            ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", member.SalutationID);
            return View(member);
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string chkRemoveImage, IFormFile thePicture, List<IFormFile> theFiles)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            var memberToUpdate = await _context.Members
                .Include(m => m.Subscriptions)
                .Include(m => m.ActionItems)
                .Include(m => m.PAC)
                .Include(m => m.AcademicDivision)
                .Include(m => m.Salutation)
                .Include(d => d.MemberDocuments)
                .Include(d => d.MemberPhoto)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (memberToUpdate == null)
            {
                return NotFound(new { message = "Something went wrong looking for that member, please try again." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Something went wrong please try again." });
            }

            if (await TryUpdateModelAsync<Member>(memberToUpdate, "",
                m => m.FirstName, m => m.LastName, m => m.PhoneNumber, m => m.Email, m => m.EducationSummary,
                m => m.OccupationalSummary, m => m.StreetAddress, m => m.PostalCode, m => m.City, m => m.Province, m => m.IsArchived, m => m.NCGraduate
                , m => m.CompanyName, m => m.CompanyStreetAddress, m => m.CompanyProvince, m => m.CompanyPostalCode, m => m.CompanyPhoneNumber, m => m.CompanyEmail, m => m.PreferredContact, m => m.CompanyPositionTitle, m => m.MemberDocuments, m => m.ActionItems
                , m => m.AcademicDivisionID, m => m.PACID, m => m.SalutationID, m => m.Subscriptions,m=>m.MemberRole))
                try
                {
                   
                    if (EmailValidator.IsValidEmail(memberToUpdate.Email) == false)
                    {
                        ModelState.AddModelError(nameof(memberToUpdate.Email), "Please enter a valid email. Ex. test1234@hotmail.com");
                        ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                        return View(memberToUpdate);
                    }
                    if (EmailValidator.IsValidEmail(memberToUpdate.CompanyEmail) == false)
                    {
                        ModelState.AddModelError(nameof(memberToUpdate.CompanyEmail), "Please enter a valid company email. Ex. test1234@hotmail.com");
                        ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                        return View(memberToUpdate);
                    }                 
                    if (chkRemoveImage != null)
                    {
                        memberToUpdate.MemberPhoto = null;
                    }
                    else
                    {
                        await AddPicture(memberToUpdate, thePicture);
                    }
                    await AddDocumentsAsync(memberToUpdate, theFiles);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Member edited successfully!";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(memberToUpdate.ID))
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
                    ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                    return View(memberToUpdate);
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes please try again, if this continues contact your admin.");
                    ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                    return View(memberToUpdate);
                }

            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName", memberToUpdate.AcademicDivisionID);
            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName", memberToUpdate.PACID);
            ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
            return RedirectToAction("Index", new { successMessage = "Member edited successfully!" });
        }

        public JsonResult GetPacList(int Id)
        {
            var result = _context.PAC
                .Where(r => r.AcademicDivisionID == Id).OrderBy(a => a.PACName).ToList();
            return Json(new SelectList(result, "ID", "PACName"));
        }

        public JsonResult GetPacList2()
        {
            var result = _context.PAC.OrderBy(a => a.PACName).ToList();
            return Json(new SelectList(result, "ID", "PACName"));
        }


        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            if (id == null || _context.Members == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.Subscriptions)
                .Include(m => m.ActionItems)
                .Include(m => m.PAC)
                .Include(m => m.AcademicDivision)
                .Include(m => m.Salutation)
                .Include(d => d.MemberDocuments)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            if (_context.Members == null)
            {
                return Problem("Entity set 'PAC_Context.Members'  is null.");
            }
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                _context.Members.Remove(member);
            }

            await _context.SaveChangesAsync();
            //return RedirectToAction(nameof(Index));
            ViewBag.SuccessMessage = "The account was successfully deleted";
            return Redirect(ViewData["returnURL"].ToString());
        }

        //private SelectList DivisionSelectList(int selectedID)
        //{
        //    return new SelectList(_context.AcademicDivisions
        //        .OrderBy(m => m.DivisionName), "ID", "DivisionName", selectedID);
        //}

        //private SelectList PACSelectList2(int DivisionID ,int? selectedID)
        //{
        //    var query = from p in _context.PAC
        //                select p;
        //    if (DivisionID > -1)
        //    {
        //        query = query.Where(p => p.AcademicDivisionID == DivisionID);
        //    }

        //    return new SelectList(query.OrderBy(p => p.PACName), "ID", "PACName", selectedID);
        //}

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }
        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }

        //[HttpGet]
        //public JsonResult GetPACs(int DivisionID)
        //{
        //    return Json(PACSelectList2(DivisionID, null));
        //}

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

        private async Task AddPicture(Member member, IFormFile thePicture)
        {
            //Get the picture and save it with the Patient (2 sizes)
            if (thePicture != null)
            {
                string mimeType = thePicture.ContentType;
                long fileLength = thePicture.Length;
                if (!(mimeType == "" || fileLength == 0))//Looks like we have a file!!!
                {
                    if (mimeType.Contains("image"))
                    {
                        using var memoryStream = new MemoryStream();
                        await thePicture.CopyToAsync(memoryStream);
                        var pictureArray = memoryStream.ToArray();//Gives us the Byte[]

                        //Check if we are replacing or creating new
                        if (member.MemberPhoto != null)
                        {
                            //We already have pictures so just replace the Byte[]
                            member.MemberPhoto.Content = ResizeImage.shrinkImageWebp(pictureArray, 500, 600);
                        }
                        else //No pictures saved so start new
                        {
                            member.MemberPhoto = new MemberPhoto
                            {
                                Content = ResizeImage.shrinkImageWebp(pictureArray, 500, 600),
                                MimeType = "image/webp"
                            };
                        }
                    }
                }
            }
        }

        public async Task<FileContentResult> Download(int id)
        {
            var theFile = await _context.UploadedFile
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .FirstOrDefaultAsync();
            return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
        }

        private async Task AddDocumentsAsync(Member member, List<IFormFile> theFiles)
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
                        MemberDocuments d = new MemberDocuments();
                        using (var memoryStream = new MemoryStream())
                        {
                            await f.CopyToAsync(memoryStream);
                            d.FileContent.Content = memoryStream.ToArray();
                        }
                        d.MimeType = mimeType;
                        d.FileName = fileName;
                        member.MemberDocuments.Add(d);
                    };
                }
            }
        }
        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.ID == id);
        }

        public IActionResult DownloadFile(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Files", fileName);
            var fileExists = System.IO.File.Exists(filePath);

            if (!fileExists)
            {
                return NotFound();
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            return File(fileStream, "application/octet-stream", fileName);
        }
    }
}
