using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;
using NiagaraCollegeProject.Utilities;

namespace NiagaraCollegeProject.Controllers
{
    [Authorize(Roles = "Admin, Supervisor,Staff")]

    public class ArchivedActionItemsController : Controller
    {
        private readonly PAC_Context _context;
        private readonly ApplicationDbContext _identityContext;
        private readonly IMyEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public ArchivedActionItemsController(PAC_Context context,
            ApplicationDbContext identityContext, IMyEmailSender emailSender,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _identityContext = identityContext;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? page, int? pageSizeID, int? MemberID, DateTime? startDate, DateTime? endDate,
            bool? Completed, string actionButton, string sortDirection = "asc", string sortField = "Member")
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            PopulateDropDownLists();

            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = "btn-outline-dark"; //Assume not filtering
            //Then in each "test" for filtering, add ViewData["Filtering"] = " show" if true;

            var actionitems = _context.ActionItems
                .Include(a => a.Meeting)
                .Include(a => a.Member)
                .Include(d => d.ActionDocuments)
                .Where(d => d.IsArchived == true)
                .AsNoTracking();

            //Add as many filters as needed
            if (MemberID.HasValue)
            {
                actionitems = actionitems.Where(p => p.MemberID == MemberID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (startDate.HasValue)
            {
                DateTime start = startDate.Value.Date;
                actionitems = actionitems.Where(m => m.TaskDueDate >= start);
                TempData["StartDate"] = startDate.Value.ToString("yyyy-MM-dd");
                ViewData["Filtering"] = "btn-danger";
            }
            if (endDate.HasValue)
            {
                DateTime end = endDate.Value.Date.AddDays(1); // add one day to include the end date
                actionitems = actionitems.Where(m => m.TaskDueDate < end);
                TempData["EndDate"] = endDate.Value.ToString("yyyy-MM-dd");
                ViewData["Filtering"] = "btn-danger";
            }
            if (Completed.HasValue)
            {
                actionitems = actionitems.Where(p => p.Completed == Completed);
                ViewData["Filtering"] = "btn-danger";
            }


            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "MEMBER ↕", "DUE DATE ↕" };


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
            }

            //Now we know which field and direction to sort by
            if (sortField == "MEMBER ↕")
            {
                if (sortDirection == "asc")
                {
                    actionitems = actionitems
                        .OrderBy(p => p.Member.LastName)
                        .ThenBy(p => p.Member.FirstName);
                }
                else
                {
                    actionitems = actionitems
                        .OrderByDescending(p => p.Member.LastName)
                        .ThenByDescending(p => p.Member.FirstName);
                }
            }
            else if (sortField == "DUE DATE ↕")
            {
                if (sortDirection == "asc")
                {
                    actionitems = actionitems
                        .OrderBy(p => p.TaskDueDate);
                }
                else
                {
                    actionitems = actionitems
                        .OrderByDescending(p => p.TaskDueDate);
                }
            }

            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "ActionItems");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<ActionItem>.CreateAsync(actionitems.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        public async Task<ActionResult> Restore(int? id)
        {
            ViewDataReturnURL();

            var actionitems = await _context.ActionItems
                .Include(a => a.Meeting)
                .Include(a => a.Member)
                .Include(d => d.ActionDocuments)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (id == null || _context.Members == null)
            {
                return NotFound();
            }

            actionitems.IsArchived = false; // set the IsArchived flag to false to restore the record

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Action Item restored successfully!";
            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.ActionItems == null)
            {
                return NotFound();
            }

            var actionitems = await _context.ActionItems
                .Include(a => a.Meeting)
                .Include(a => a.Member)
                .Include(d => d.ActionDocuments)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (actionitems == null)
            {
                return NotFound();
            }

            return View(actionitems);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewDataReturnURL();
            PopulateDropDownLists();

            var actionitems = await _context.ActionItems
                .Include(a => a.Meeting)
                .Include(a => a.Member)
                .Include(d => d.ActionDocuments)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (actionitems == null)
            {
                return Problem("Entity set 'PAC_Context.ActionItems'  is null.");
            }
            if (actionitems != null)
            {
                _context.ActionItems.Remove(actionitems);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Action Item deleted successfully!";
            return Redirect(ViewData["returnURL"].ToString());
        }

        private void PopulateDropDownLists(ActionItem actionitems = null)
        {
            var aQuery = from a in _context.ActionItems
                         select a;
            ViewData["MeetingID"] = new SelectList(_context.Meeting, "ID", "MeetingTopicName", actionitems?.MeetingID);
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName", actionitems?.MemberID);
            List<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem { Text = "True", Value = "True" },
                new SelectListItem { Text = "False", Value = "False" }
            };

            ViewData["Completed"] = new SelectList(items, "Value", "Text", actionitems?.Completed);
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }
        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }
    }
}
