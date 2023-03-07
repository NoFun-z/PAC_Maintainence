using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
                .AsNoTracking();

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Members ↕", "PAC ↕" };

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
            if (sortField == "Members ↕")
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


            string memCommittee = "";
            memCommittee = PAC_Context.Where(e => e.Email == User.Identity.Name).FirstOrDefault().AcademicDivision.DivisionName;
            var filteredMembers = PAC_Context.Where(e => e.AcademicDivision.DivisionName == memCommittee);

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
            "Province,City,PostalCode,MemberStatus," +
            "NCGraduate,SignUpDate,ReNewDate_,RenewalDueBy," +
            "CompanyName,CompanyStreetAddress,CompanyProvince,CompanyCity," +
            "CompanyPostalCode,CompanyPhoneNumber,CompanyEmail,PreferredContact," +
            "CompanyPositionTitle,AcademicDivisionID,PACID,SalutationID, Subscriptions")] Member member, IFormFile thePicture, List<IFormFile> theFiles)
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
                .FirstOrDefaultAsync(m=> m.ID == id);
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
                return NotFound(new {message = "Something went wrong looking for that member, please try again."});
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Something went wrong please try again." });
            }

            if(await TryUpdateModelAsync<Member>(memberToUpdate, "",
                m => m.FirstName, m => m.LastName, m => m.PhoneNumber, m => m.Email, m => m.EducationSummary,
                m => m.OccupationalSummary, m => m.StreetAddress, m => m.PostalCode, m => m.City, m => m.Province, m => m.MemberStatus, m => m.NCGraduate
                , m => m.CompanyName, m => m.CompanyStreetAddress, m => m.CompanyProvince, m => m.CompanyPostalCode, m => m.CompanyPhoneNumber, m => m.CompanyEmail, m => m.PreferredContact, m => m.CompanyPositionTitle, m => m.MemberDocuments, m => m.ActionItems
                , m => m.AcademicDivisionID, m => m.PACID, m => m.SalutationID, m => m.Subscriptions))
                try
                {
                    var allowedEmailRegex = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*"
                                        + "@"
                                        + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";

                    var allowedPostalRegex = "^[ABCEGHJ-NPRSTVXY][0-9][ABCEGHJ-NPRSTV-Z][0-9][ABCEGHJ-NPRSTV-Z][0-9]$";

                    var allowedPhoneNumberRegex = "^\\d{10}$";

                    if (!Regex.IsMatch(memberToUpdate.Email, allowedEmailRegex))
                    {
                        ModelState.AddModelError(nameof(memberToUpdate.Email), "Please enter a valid email. Ex. test1234@hotmail.com");
                        ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                        return View(memberToUpdate);
                    }
                    if (!Regex.IsMatch(memberToUpdate.PostalCode, allowedPostalRegex))
                    {
                        ModelState.AddModelError(nameof(memberToUpdate.PostalCode), "Please enter a valid postal code. Ex. T3L2K9");
                        ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                        return View(memberToUpdate);
                    }
                    if (!Regex.IsMatch(memberToUpdate.PhoneNumber, allowedPhoneNumberRegex))
                    {
                        ModelState.AddModelError(nameof(memberToUpdate.PhoneNumber), "Please enter a valid phone number. Ex. 9056883529");
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
                        await AddDocumentsAsync(memberToUpdate, theFiles);
                    }

                    await _context.SaveChangesAsync();
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
                catch(ArgumentNullException) 
                {
                    ModelState.AddModelError("", "Some fields are missing please make sure all required data is supplied.");
                    ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                    return View(memberToUpdate);
                }
                catch(RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes please try again, if this continues contact your admin.");
                    ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                    return View(memberToUpdate);
                }
            
            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName", memberToUpdate.AcademicDivisionID);
            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName", memberToUpdate.PACID);
            ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
            return RedirectToAction("Details", new { memberToUpdate.ID });
        }

        public JsonResult GetPacList(int Id)
        {
            var result = _context.PAC
                .Where(r => r.AcademicDivisionID == Id).ToList();
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
    }
}
