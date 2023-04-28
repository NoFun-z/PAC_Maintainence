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

namespace NiagaraCollegeProject.Controllers
{
    [Authorize]

    public class MemberActionItemsController : Controller
    {
        private readonly PAC_Context _context;

        public MemberActionItemsController(PAC_Context context)
        {
            _context = context;
        }

        // GET: MemberActionItems
        public async Task<IActionResult> Index(int? MemberID, int? page, int? pageSizeID)
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //Get the URL with the last filter, sort and page parameters from THE Meetings Index View
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Members");

            if (!MemberID.HasValue)
            {
                //Go back to the proper return URL
                return Redirect(ViewData["returnURL"].ToString());
            }

            var pAC_Context = from m in _context.MemberActionItems
                .Include(a => a.Member).ThenInclude(a => a.PAC)
                .Include(a => a.Member).ThenInclude(a => a.ActionItems)
                .Include(a => a.Member).ThenInclude(a => a.Salutation)
                .Include(a => a.Member).ThenInclude(a => a.MemberDocuments)
                .Include(a => a.Member).ThenInclude(a => a.MemberPhoto)
                .Include(d => d.ActionItem).ThenInclude(d => d.ActionDocuments)
                .Include(d => d.ActionItem).ThenInclude(d => d.Meeting)
                .Where(d => d.ActionItem.IsArchived == false)
                              where m.Member.ID == MemberID.GetValueOrDefault()
                              select m;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "MemberActionItems");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<MemberActionItems>.CreateAsync(pAC_Context.AsNoTracking(), page ?? 1, pageSize);

            //Now get the MASTER record, the member, so it can be displayed at the top of the screen
            Member member = _context.Members
                .Include(m => m.Subscriptions)
                .Include(m => m.ActionItems)
                .Include(m => m.PAC)
                .Include(m => m.AcademicDivision)
                .Include(m => m.Salutation)
                .Include(d => d.MemberDocuments)
                .Where(d => d.ID == MemberID.GetValueOrDefault())
                .AsNoTracking()
                .FirstOrDefault();

            ViewBag.Member = member;

            return View(pagedData);
        }

        // GET: MemberActionItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.MemberActionItems == null)
            {
                return NotFound();
            }

            var memberActionItems = await _context.MemberActionItems
                .Include(m => m.ActionItem)
                .Include(m => m.Member)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (memberActionItems == null)
            {
                return NotFound();
            }

            return View(memberActionItems);
        }

        // GET: MemberActionItems/Create
        public IActionResult Create()
        {
            ViewDataReturnURL();

            ViewData["ActionItemID"] = new SelectList(_context.ActionItems, "ID", "AgendaName");
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "ID");
            return View();
        }

        // POST: MemberActionItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,ActionItemID,MemberID")] MemberActionItems memberActionItems)
        {
            ViewDataReturnURL();

            if (ModelState.IsValid)
            {
                _context.Add(memberActionItems);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ActionItemID"] = new SelectList(_context.ActionItems, "ID", "AgendaName", memberActionItems.ActionItemID);
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "ID", memberActionItems.MemberID);
            return View(memberActionItems);
        }

        // GET: MemberActionItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.MemberActionItems == null)
            {
                return NotFound();
            }

            var memberActionItems = await _context.MemberActionItems.FindAsync(id);
            if (memberActionItems == null)
            {
                return NotFound();
            }
            ViewData["ActionItemID"] = new SelectList(_context.ActionItems, "ID", "AgendaName", memberActionItems.ActionItemID);
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "ID", memberActionItems.MemberID);
            return View(memberActionItems);
        }

        // POST: MemberActionItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,ActionItemID,MemberID")] MemberActionItems memberActionItems)
        {
            ViewDataReturnURL();

            if (id != memberActionItems.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(memberActionItems);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberActionItemsExists(memberActionItems.ID))
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
            ViewData["ActionItemID"] = new SelectList(_context.ActionItems, "ID", "AgendaName", memberActionItems.ActionItemID);
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "ID", memberActionItems.MemberID);
            return View(memberActionItems);
        }

        // GET: MemberActionItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.MemberActionItems == null)
            {
                return NotFound();
            }

            var memberActionItems = await _context.MemberActionItems
                .Include(m => m.ActionItem)
                .Include(m => m.Member)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (memberActionItems == null)
            {
                return NotFound();
            }

            return View(memberActionItems);
        }

        // POST: MemberActionItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewDataReturnURL();

            if (_context.MemberActionItems == null)
            {
                return Problem("Entity set 'PAC_Context.MemberActionItems'  is null.");
            }
            var memberActionItems = await _context.MemberActionItems.FindAsync(id);
            if (memberActionItems != null)
            {
                _context.MemberActionItems.Remove(memberActionItems);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }
        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }

        private bool MemberActionItemsExists(int id)
        {
            return _context.MemberActionItems.Any(e => e.ID == id);
        }
    }
}
