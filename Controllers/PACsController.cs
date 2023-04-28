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
    [Authorize(Roles = "Admin, Supervisor")]
    public class PACsController : Controller
    {
        private readonly PAC_Context _context;

        public PACsController(PAC_Context context)
        {
            _context = context;
        }

        // GET: PACs
        public async Task<IActionResult> Index(int? page, int? pageSizeID, string successMessage, string actionButton, string sortDirection = "asc", string sortField = "PAC ↕")
        {
            if (!string.IsNullOrEmpty(successMessage))
            {
                ViewBag.SuccessMessage = successMessage;
            }

            var pAC_Context = _context.PAC
                .Include(d => d.AcademicDivision)
                .AsNoTracking();

            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "PAC ↕" };


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
            if (sortField == "PAC ↕")
            {
                if (sortDirection == "asc")
                {
                    pAC_Context = pAC_Context
                        .OrderBy(p => p.PACName);
                }
                else
                {
                    pAC_Context = pAC_Context
                        .OrderByDescending(p => p.PACName);
                }
            }

            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "PACs");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<PAC>.CreateAsync(pAC_Context.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: PACs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            if (id == null || _context.PAC == null)
            {
                return NotFound();
            }

            var pAC = await _context.PAC
                .Include(x => x.AcademicDivision)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (pAC == null)
            {
                return NotFound();
            }

            return View(pAC);
        }

        // GET: PACs/Create
        public IActionResult Create()
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName");
            return View();
        }

        // POST: PACs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,PACName,AcademicDivisionID")] PAC pAC)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            if (ModelState.IsValid)
            {
                _context.Add(pAC);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { successMessage = "PAC created successfully!" });
            }

            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName");
            return View(pAC);
        }

        // GET: PACs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            if (id == null || _context.PAC == null)
            {
                return NotFound();
            }

            var pAC = await _context.PAC
                .Include(x => x.AcademicDivision)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (pAC == null)
            {
                return NotFound();
            }
            
            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName");
            return View(pAC);
        }

        // POST: PACs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,PACName,AcademicDivisionID")] PAC pAC)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            if (id != pAC.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pAC);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", new { successMessage = "PAC edited successfully!" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PACExists(pAC.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName");
            return View(pAC);
        }

        public JsonResult GetPacList(int Id)
        {
            var result = _context.PAC
                .Where(r => r.AcademicDivisionID == Id).OrderBy(a => a.PACName).ToList();
            return Json(new SelectList(result, "ID", "PACName"));
        }

        // GET: PACs/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            if (id == null || _context.PAC == null)
            {
                return NotFound();
            }

            var pAC = await _context.PAC
                .Include(x => x.AcademicDivision)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (pAC == null)
            {
                return NotFound();
            }

            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName");
            return View(pAC);
        }

        // POST: PACs/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            if (_context.PAC == null)
            {
                return Problem("Entity set 'PAC_Context.PAC'  is null.");
            }
            var pAC = await _context.PAC.FindAsync(id);
            if (pAC != null)
            {
                _context.PAC.Remove(pAC);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { successMessage = "PAC deleted successfully!" });
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }
        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }

        private bool PACExists(int id)
        {
          return _context.PAC.Any(e => e.ID == id);
        }
    }
}
