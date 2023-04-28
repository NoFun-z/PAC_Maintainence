using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;
using NiagaraCollegeProject.ViewModels;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using NiagaraCollegeProject.Utilities;

namespace NiagaraCollegeProject.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly PAC_Context _context;
        private readonly ApplicationDbContext _identityContext;
        private readonly UserManager<IdentityUser> _userManager;

        public ReportsController(PAC_Context context,
            ApplicationDbContext identityContext, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _identityContext = identityContext;
            _userManager = userManager;
        }

        // GET: Reports
        public async Task<IActionResult> Index(int? page, int? pageSizeID, string SearchString, string MemberEmail)
        {
            var pAC_Context = _context.Report
                .Include(d => d.ReportDocuments)
                .AsNoTracking();

            var members = _context.Members
                .Select(e => new MemberAdminVM
                {
                    ID = e.ID,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    PhoneNumber = e.PhoneNumber,
                    Email = e.Email,
                    EducationSummary = e.EducationSummary,
                    OccupationalSummary = e.OccupationalSummary,
                    StreetAddress = e.StreetAddress,
                    Province = (MemberVM.Provinces)e.Province,
                    City = e.City,
                    PostalCode = e.PostalCode,
                    NCGraduate = e.NCGraduate,
                    SignUpDate = e.SignUpDate,
                    RenewalDueBy = e.RenewalDueBy,
                    CompanyName = e.CompanyName,
                    CompanyStreetAddress = e.CompanyStreetAddress,
                    CompanyProvince = (MemberVM.Provinces)e.CompanyProvince,
                    CompanyCity = e.CompanyCity,
                    CompanyPostalCode = e.CompanyPostalCode,
                    CompanyPhoneNumber = e.CompanyPhoneNumber,
                    CompanyEmail = e.CompanyEmail,
                    PreferredContact = (MemberVM.Contact)e.PreferredContact,
                    CompanyPositionTitle = e.CompanyPositionTitle,
                }).ToList();

            foreach (var e in members)
            {
                var user = await _userManager.FindByEmailAsync(e.Email);
                if (user != null)
                {
                    e.UserRoles = (List<string>)await _userManager.GetRolesAsync(user);
                }
            };

            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = "btn-outline-secondary"; //Assume not filtering
            //Then in each "test" for filtering, add ViewData["Filtering"] = " show" if true;

            PopulateDropDownLists(members);

            //Add as many filters as needed
            if (!String.IsNullOrEmpty(MemberEmail))
            {
                pAC_Context = pAC_Context.Where(p => p.CreatedBy == MemberEmail);
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                pAC_Context = pAC_Context.Where(p => p.Summary.ToUpper().Contains(SearchString.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "Reports");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Report>.CreateAsync(pAC_Context.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        //public JsonResult GetMemList()
        //{
        //    var result = _context.Members.ToList();
        //    return Json(new SelectList(result, "FullName", "Email"));
        //}

        // GET: Reports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Report == null)
            {
                return NotFound();
            }

            var report = await _context.Report
                .Include(d => d.ReportDocuments)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // GET: Reports/Create
        [Authorize(Roles = "Admin, Supervisor")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Reports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Supervisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Summary")] Report report, List<IFormFile> theFiles)
        {
            if (ModelState.IsValid)
            {
                _context.Add(report);
                await AddDocumentsAsync(report, theFiles);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(report);
        }

        // GET: Reports/Edit/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Report == null)
            {
                return NotFound();
            }

            // var report = await _context.Report.FindAsync(id);
            var report = await _context.Report
                .Include(d => d.ReportDocuments)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (report == null)
            {
                return NotFound();
            }
            return View(report);
        }

        // POST: Reports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Supervisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Summary")] Report report, List<IFormFile> theFiles)
        {
            if (id != report.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(report);
                    await AddDocumentsAsync(report, theFiles);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReportExists(report.ID))
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
            return View(report);
        }

        // GET: Reports/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Report == null)
            {
                return NotFound();
            }

            var report = await _context.Report
                .FirstOrDefaultAsync(m => m.ID == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: Reports/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, List<IFormFile> theFiles)
        {
            if (_context.Report == null)
            {
                return Problem("Entity set 'PAC_Context.Report'  is null.");
            }
            var report = await _context.Report.FindAsync(id);
            if (report != null)
            {
                _context.Report.Remove(report);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private static SelectList MemberSelectList(string memEmail, List<MemberAdminVM> members)
        {
            List<string> roleNames = new() { "Admin", "Supervisor" };

            var filteredMember = members.Where(x => x.UserRoles.Any(y => roleNames.Contains(y)));

            return new SelectList(filteredMember.OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
                , "Email", "FullName", memEmail);
        }

        private void PopulateDropDownLists(List<MemberAdminVM> member, Member members = null)
        {
            ViewData["MemberEmail"] = MemberSelectList(members?.Email, member);
        }

        private bool ReportExists(int id)
        {
            return _context.Report.Any(e => e.ID == id);
        }

        public async Task<FileContentResult> Download(int id)
        {
            var theFile = await _context.UploadedFile
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .FirstOrDefaultAsync();
            return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
        }

        private async Task AddDocumentsAsync(Report report, List<IFormFile> theFiles)
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
                        ReportDocuments d = new ReportDocuments();
                        using (var memoryStream = new MemoryStream())
                        {
                            await f.CopyToAsync(memoryStream);
                            d.FileContent.Content = memoryStream.ToArray();
                        }
                        d.MimeType = mimeType;
                        d.FileName = fileName;
                        report.ReportDocuments.Add(d);
                    };
                }
            }
        }
    }
}
