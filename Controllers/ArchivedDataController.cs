using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.ViewModels;
using NiagaraCollegeProject.Models;
using Microsoft.AspNetCore.Identity;
using NiagaraCollegeProject.Utilities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace NiagaraCollegeProject.Controllers
{
	[Authorize(Roles = "Admin, Supervisor")]
	public class ArchivedDataController : Controller
    {
        private readonly PAC_Context _context;
        private readonly ApplicationDbContext _identityContext;
        private readonly IMyEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public ArchivedDataController(PAC_Context context,
            ApplicationDbContext identityContext, IMyEmailSender emailSender,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _identityContext = identityContext;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string sortDirectionCheck, string sortFieldID, int? page, int? pageSizeID, string SearchString, int? AcademicDivisionID,
            int? PACID, string actionButton, string sortDirection = "asc", string sortField = "Member")
        {
            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = "btn-outline-primary"; //Assume not filtering
            //Then in each "test" for filtering, add ViewData["Filtering"] = " show" if true;

            PopulateDropDownLists();

            var member = _context.Members
                .Include(m => m.PAC)
                .Include(m => m.AcademicDivision)
                .Where(d => d.IsArchived == true)
                .AsNoTracking();

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "FIRST NAME ↕", "LAST NAME ↕", "PAC ↕", "EMAIL ↕" };

            if (AcademicDivisionID.HasValue)
            {
                member = member.Where(c => c.AcademicDivisionID == AcademicDivisionID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (PACID.HasValue)
            {
                member = member.Where(c => c.PACID == PACID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                member = member.Where(c => c.LastName.ToUpper().Contains(SearchString.ToUpper())
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
                    member = member
                        .OrderBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);
                }
                else
                {
                    member = member
                        .OrderByDescending(p => p.LastName)
                        .ThenByDescending(p => p.FirstName);
                }
            }
            else if (sortField == "FIRST NAME ↕")
            {
                if (sortDirection == "asc")
                {
                    member = member
                        .OrderBy(p => p.FirstName)
                        .ThenBy(p => p.LastName);
                }
                else
                {
                    member = member
                        .OrderByDescending(p => p.FirstName)
                        .ThenByDescending(p => p.LastName);
                }
            }
            else if (sortField == "PAC ↕")
            {
                if (sortDirection == "asc")
                {
                    member = member
                        .OrderByDescending(p => p.PAC.PACName);
                }
                else
                {
                    member = member
                        .OrderBy(p => p.PAC.PACName);
                }
            }
            else if (sortField == "EMAIL ↕")
            {
                if (sortDirection == "asc")
                {
                    member = member
                        .OrderBy(p => p.Email);
                }
                else
                {
                    member = member
                        .OrderByDescending(p => p.Email);
                }
            }

            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            //SelectList for Sorting Options
            ViewBag.sortFieldID = new SelectList(sortOptions, sortField.ToString());

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "Members");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Member>.CreateAsync(member.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
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

        public async Task<ActionResult> Restore(int? id)
        {
            var member = await _context.Members
                .Include(m => m.PAC)
                .Include(m => m.AcademicDivision)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (id == null || _context.Members == null)
            {
                return NotFound();
            }
            member.IsArchived = false; // set the IsArchived flag to false to restore the record
            member.NotRenewing = false;

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Account restored successfully!";
            return RedirectToAction("Index", "Members");
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
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
            if (_context.Members == null)
            {
                return Problem("Entity set 'PAC_Context.Members' is null.");
            }
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                _context.Members.Remove(member);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Account deleted successfully!";
            return RedirectToAction("Index", "Members");
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
    }

}
