using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.ViewModels;
using NiagaraCollegeProject.Models;
using Microsoft.AspNetCore.Identity;
using NiagaraCollegeProject.Utilities;

namespace NiagaraCollegeProject.Controllers
{
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

        public ActionResult Index()
        {
            var member = _context.Members
                .Include(m => m.PAC)
                .Include(m => m.AcademicDivision)
                .Where(d => d.IsArchived == true).ToList();
            return View(member);
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
            member.MemberStatus = true;

            _context.SaveChanges();
            return RedirectToAction("Index", "TeamMembers", new { id = member.ID });
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
                return Problem("Entity set 'PAC_Context.Members'  is null.");
            }
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                _context.Members.Remove(member);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
