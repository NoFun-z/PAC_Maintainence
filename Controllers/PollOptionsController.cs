using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;

namespace NiagaraCollegeProject.Controllers
{
    [Authorize(Roles = "Admin, Supervisor")]

    public class PollOptionsController : Controller
    {
        private readonly PAC_Context _context;

        public PollOptionsController(PAC_Context context)
        {
            _context = context;
        }

        // GET: PollOptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PollOption == null)
            {
                return NotFound();
            }

            var pollOption = await _context.PollOption.Include(m=>m.Poll).FirstOrDefaultAsync(m=>m.ID == id);
            if (pollOption == null)
            {
                return NotFound();
            }
            ViewData["PollID"] = new SelectList(_context.Poll, "ID", "ID", pollOption.PollID);
            return View(pollOption);
        }

        // POST: PollOptions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var pollOptionToUpdate = await _context.PollOption.FirstOrDefaultAsync(m=>m.ID== id);

            var pollID = pollOptionToUpdate.PollID.ToString();
            if (pollOptionToUpdate == null)
            {
                return NotFound(new { message = "Something went wrong looking for that meeting, please try again" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Something went wrong please try again." });
            }

            if (await TryUpdateModelAsync<PollOption>(pollOptionToUpdate, "",
                  d => d.OptionText, d => d.Votes))
                try
                {

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Poll option modified.";
                    return RedirectToAction("Details", "Polls", new { id = pollID });


                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PollOptionExists(pollOptionToUpdate.ID))
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
                    return View(pollOptionToUpdate);
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes please try again, if this continues contact your admin.");
                    return View(pollOptionToUpdate);
                }

            ViewData["PollID"] = new SelectList(_context.Poll, "ID", "ID", pollOptionToUpdate.PollID);
            TempData["FailureMessage"] = "Unable to edit poll option please try again.";
            return View(pollOptionToUpdate);
        }

        // GET: PollOptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PollOption == null)
            {
                return NotFound();
            }

            var pollOption = await _context.PollOption
                .Include(p => p.Poll)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (pollOption == null)
            {
                return NotFound();
            }

            return View(pollOption);
        }

        // POST: PollOptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PollOption == null)
            {
                return Problem("Entity set 'PAC_Context.PollOption'  is null.");
            }
            var pollOption = await _context.PollOption.FindAsync(id);
            if (pollOption != null)
            {
                _context.PollOption.Remove(pollOption);
            }
            
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Poll option deleted.";

            return RedirectToAction("Details", "Polls", new { id = pollOption.PollID });
        }

        private bool PollOptionExists(int id)
        {
          return _context.PollOption.Any(e => e.ID == id);
        }
    }
}
