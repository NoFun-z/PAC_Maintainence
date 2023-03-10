using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;

namespace NiagaraCollegeProject.Controllers
{
    public class AgendaItemsController : Controller
    {
        private readonly PAC_Context _context;

        public AgendaItemsController(PAC_Context context)
        {
            _context = context;
        }

        // GET: AgendaItems
        public async Task<IActionResult> Index()
        {
            var pAC_Context = _context.AgendaItem.Include(a => a.Meeting).Include(a => a.Member);
            return View(await pAC_Context.ToListAsync());
        }

        // GET: AgendaItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.AgendaItem == null)
            {
                return NotFound();
            }

            var agendaItem = await _context.AgendaItem
                .Include(a => a.Meeting)
                .Include(a => a.Member)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (agendaItem == null)
            {
                return NotFound();
            }

            return View(agendaItem);
        }

        // GET: AgendaItems/Create
        public IActionResult Create()
        {
            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "MeetingTopicName");
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName");
            return View();
        }

        // POST: AgendaItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,AgendaName,UploadDate,MemberID,MeetingID")] AgendaItem agendaItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(agendaItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "MeetingTopicName", agendaItem.MeetingID);
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName", agendaItem.MemberID);
            return View(agendaItem);
        }

        // GET: AgendaItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.AgendaItem == null)
            {
                return NotFound();
            }

            var agendaItem = await _context.AgendaItem.FindAsync(id);
            if (agendaItem == null)
            {
                return NotFound();
            }
            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "MeetingTopicName", agendaItem.MeetingID);
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName", agendaItem.MemberID);
            return View(agendaItem);
        }

        // POST: AgendaItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,AgendaName,UploadDate,MemberID,MeetingID")] AgendaItem agendaItem)
        {
            if (id != agendaItem.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(agendaItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AgendaItemExists(agendaItem.ID))
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
            ViewData["MeetingID"] = new SelectList(_context.Meetings, "ID", "MeetingTopicName", agendaItem.MeetingID);
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "FullName", agendaItem.MemberID);
            return View(agendaItem);
        }

        // GET: AgendaItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.AgendaItem == null)
            {
                return NotFound();
            }

            var agendaItem = await _context.AgendaItem
                .Include(a => a.Meeting)
                .Include(a => a.Member)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (agendaItem == null)
            {
                return NotFound();
            }

            return View(agendaItem);
        }

        // POST: AgendaItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AgendaItem == null)
            {
                return Problem("Entity set 'PAC_Context.AgendaItem'  is null.");
            }
            var agendaItem = await _context.AgendaItem.FindAsync(id);
            if (agendaItem != null)
            {
                _context.AgendaItem.Remove(agendaItem);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AgendaItemExists(int id)
        {
          return _context.AgendaItem.Any(e => e.ID == id);
        }
    }
}
