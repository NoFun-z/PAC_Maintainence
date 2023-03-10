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
    public class MeetingMinutesController : Controller
    {
        private readonly PAC_Context _context;

        public MeetingMinutesController(PAC_Context context)
        {
            _context = context;
        }

        // GET: MeetingMinutes
        public async Task<IActionResult> Index()
        {
            var pAC_Context = _context.MeetingMinutes.Include(m => m.Meeting);
            return View(await pAC_Context.ToListAsync());
        }

        // GET: MeetingMinutes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MeetingMinutes == null)
            {
                return NotFound();
            }

            var meetingMinute = await _context.MeetingMinutes
                .Include(m => m.Meeting)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (meetingMinute == null)
            {
                return NotFound();
            }

            return View(meetingMinute);
        }

        // GET: MeetingMinutes/Create
        public IActionResult Create()
        {
            ViewData["MeetingID"] = new SelectList(_context.Meeting, "ID", "MeetingTopicName");
            return View();
        }

        // POST: MeetingMinutes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,MeetingDiscussion,Duration,MeetingID")] MeetingMinute meetingMinute)
        {
            if (ModelState.IsValid)
            {
                _context.Add(meetingMinute);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MeetingID"] = new SelectList(_context.Meeting, "ID", "MeetingTopicName", meetingMinute.MeetingID);
            return View(meetingMinute);
        }

        // GET: MeetingMinutes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MeetingMinutes == null)
            {
                return NotFound();
            }

            var meetingMinute = await _context.MeetingMinutes.FindAsync(id);
            if (meetingMinute == null)
            {
                return NotFound();
            }
            ViewData["MeetingID"] = new SelectList(_context.Meeting, "ID", "MeetingTopicName", meetingMinute.MeetingID);
            return View(meetingMinute);
        }

        // POST: MeetingMinutes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,MeetingDiscussion,Duration,MeetingID")] MeetingMinute meetingMinute)
        {
            if (id != meetingMinute.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(meetingMinute);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MeetingMinuteExists(meetingMinute.ID))
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
            ViewData["MeetingID"] = new SelectList(_context.Meeting, "ID", "MeetingTopicName", meetingMinute.MeetingID);
            return View(meetingMinute);
        }

        // GET: MeetingMinutes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MeetingMinutes == null)
            {
                return NotFound();
            }

            var meetingMinute = await _context.MeetingMinutes
                .Include(m => m.Meeting)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (meetingMinute == null)
            {
                return NotFound();
            }

            return View(meetingMinute);
        }

        // POST: MeetingMinutes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MeetingMinutes == null)
            {
                return Problem("Entity set 'PAC_Context.MeetingMinutes'  is null.");
            }
            var meetingMinute = await _context.MeetingMinutes.FindAsync(id);
            if (meetingMinute != null)
            {
                _context.MeetingMinutes.Remove(meetingMinute);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MeetingMinuteExists(int id)
        {
          return _context.MeetingMinutes.Any(e => e.ID == id);
        }
    }
}
