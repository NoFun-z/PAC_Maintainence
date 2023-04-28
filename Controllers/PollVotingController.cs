using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.ViewModels;
using NiagaraCollegeProject.Models;
using Microsoft.AspNetCore.Identity;
using NiagaraCollegeProject.Utilities;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using iTextSharp.text;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing.Printing;
using Microsoft.AspNetCore.Authorization;

namespace NiagaraCollegeProject.Controllers
{
    [Authorize(Roles = "Admin, Supervisor,Staff")]

    public class PollVotingController : Controller
    {
        private readonly PAC_Context _context;
        private readonly ApplicationDbContext _identityContext;
        private readonly IMyEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public PollVotingController(PAC_Context context,
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
            var user = _context.Members.Where(m => m.Email == User.Identity.Name).FirstOrDefault();

            var userVotedPolls = _context.PollVote
                .Where(x => x.UserID == user.ID)
                .AsNoTracking();

            var PollOptions = _context.PollOption
               .AsNoTracking();


            if (User.IsInRole("Admin") || User.IsInRole("Supervisor"))
            {
                return View(_context.Poll
                    .Where(x=>PollOptions.Where(p => p.PollID == x.ID).Count() >= 2 && x.ExpiryDate >= DateTime.Today && x.isActive == true).AsNoTracking());
            }
            if (User.IsInRole("Staff"))
            {
                return View(_context.Poll
                .Where(x => userVotedPolls.All(p => p.PollID != x.ID) && x.PacID == user.PACID && PollOptions.Where(p => p.PollID == x.ID).Count() >= 2 && x.ExpiryDate >= DateTime.Today && x.isActive == true)
                .ToList());
            }
            return View();
        }
        public IActionResult Vote(int ID)
        {

            var pollOptions = _context.PollOption
                .Include(m => m.Poll)
                .Where(m => m.PollID == ID).AsNoTracking();
            if (pollOptions.FirstOrDefault().Poll.Question.Count() > 0) TempData["PollName"] = pollOptions.First().Poll.Question;
            return View(pollOptions);
        }
        [HttpPost]
        public async Task<ActionResult> VoteConfirmed([FromBody]int ID)
        {
            
            var user = _context.Members.Where(m => m.Email == User.Identity.Name).FirstOrDefault();
            var pollOptions = _context.PollOption
                .Include(m => m.Poll)
                .FirstOrDefault(m => m.ID == ID);

            var poll = _context.Poll
                .Where(x => x.ID == pollOptions.Poll.ID)
                .FirstOrDefault();
            if (_context.PollVote.Where(v => v.UserID == user.ID).ToList().Any(v => v.PollID == poll.ID) == true)
            {
                HttpContext.Session.Remove("SuccessMessage");
                HttpContext.Session.SetString("FailureMessage", "Your already participated in this poll.");
                return RedirectToAction("Index", "PollVoting");
            }
            else
            {
               
                pollOptions.Votes ++;
                await _context.SaveChangesAsync();


                PollVote pollVote = new PollVote();

                pollVote.PollID = poll.ID;
                pollVote.OptionTextID = pollOptions.ID;
                pollVote.UserID = user.ID;

                _context.PollVote.Add(pollVote);
                HttpContext.Session.Remove("FailureMessage");

                HttpContext.Session.SetString("SuccessMessage", "Thank you for your participation.") ;
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "PollVoting");
            }
        }
        
    }
}
