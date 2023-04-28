using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;
using NiagaraCollegeProject.Utilities;
using NiagaraCollegeProject.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using iTextSharp.text.pdf.qrcode;
using System.Text.Encodings.Web;

namespace NiagaraCollegeProject.Controllers
{
    [Authorize(Roles = "Admin, Supervisor")]

    public class PollsController : Controller
    {
        private readonly PAC_Context _context;
        private readonly ApplicationDbContext _identityContext;
        private readonly IMyEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public PollsController(PAC_Context context,
            ApplicationDbContext identityContext, IMyEmailSender emailSender,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _identityContext = identityContext;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        // GET: Polls
        public async Task<IActionResult> Index(string sortDirectionCheck, string sortFieldID, int? page, int? pageSizeID, string SearchString,
            int? PACID, string actionButton, string sortDirection = "asc", string sortField = "Poll")
        {
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);
            ViewData["Filtering"] = "btn-outline-primary";
            PopulateDropDownLists();
            var pAC_Context = _context.Poll.Include(p => p.PAC).AsNoTracking();
            string[] sortOptions = new[] { "QUESTION ↕", "ACTIVE STATUS ↕", "PAC ↕", "EXPIRY DATE ↕" };

            if (PACID.HasValue)
            {
                pAC_Context = pAC_Context.Where(c => c.PacID == PACID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                pAC_Context = pAC_Context.Where(x => x.Question.ToUpper().Contains(SearchString.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }
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
            if (sortField == "QUESTION ↕")
            {
                if (sortDirection == "asc")
                {
                    pAC_Context = pAC_Context
                        .OrderBy(p => p.Question);
                }
                else
                {
                    pAC_Context = pAC_Context
                        .OrderByDescending(p => p.Question);
                }
            }
            else if (sortField == "ACTIVE STATUS ↕")
            {
                if (sortDirection == "asc")
                {
                    pAC_Context = pAC_Context
                        .OrderBy(p => p.isActive);
                }
                else
                {
                    pAC_Context = pAC_Context
                        .OrderByDescending(p => p.isActive);
                }
            }
            else if (sortField == "PAC ↕")
            {
                if (sortDirection == "asc")
                {
                    pAC_Context = pAC_Context
                        .OrderByDescending(p => p.PacID);
                }
                else
                {
                    pAC_Context = pAC_Context
                        .OrderBy(p => p.PacID);
                }
            }
            else if (sortField == "EXPIRY DATE ↕")
            {
                if (sortDirection == "asc")
                {
                    pAC_Context = pAC_Context
                        .OrderByDescending(p => p.ExpiryDate);
                }
                else
                {
                    pAC_Context = pAC_Context
                        .OrderBy(p => p.ExpiryDate);
                }
            }

            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            //SelectList for Sorting Options
            ViewBag.sortFieldID = new SelectList(sortOptions, sortField.ToString());
            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName");
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "Members");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Poll>.CreateAsync(pAC_Context.AsNoTracking(), page ?? 1, pageSize);

            if (User.IsInRole("Admin") || User.IsInRole("Supervisor"))
            {
                pagedData = await PaginatedList<Poll>.CreateAsync(pAC_Context.AsNoTracking(), page ?? 1, pageSize);
            }
            if (User.IsInRole("Staff"))
            {
                int? filteredPac = _context.Members.Where(m => m.Email == User.Identity.Name).FirstOrDefault().PACID;
                var filteredMembers = pAC_Context.Where(m => m.PacID == filteredPac);
                pagedData = await PaginatedList<Poll>.CreateAsync(filteredMembers.AsNoTracking(), page ?? 1, pageSize);
            }

            return View(pagedData);
        }

        // GET: Polls/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Poll == null)
            {
                return NotFound();
            }


            var poll = await _context.Poll
                .Include(p => p.PAC)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (poll == null)
            {
                return NotFound();
            }
            var pollOptions = _context.PollOption
                .Include(d => d.Poll)
                .Where(d => d.PollID == poll.ID)
                .AsNoTracking();

            ViewBag.pollOptions = pollOptions;

            return View(poll);
        }

        // GET: Polls/Create
        public IActionResult Create()
        {
            ViewData["PacID"] = new SelectList(_context.PAC, "ID", "PACName");
            return View();
        }

        // POST: Polls/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Question,isActive,ExpiryDate,PacID")] Poll poll, IFormCollection form)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = new List<string>();

                if (form["field1"].ToString() == "" || form["field2"].ToString() == "") modelErrors.Add("You must have a minimum of 2 poll options.");
                if (form["PACID"].ToString() == "") modelErrors.Add("Please select a PAC.");

                foreach (var modelState in ModelState.Values)
                {
                    foreach (var modelError in modelState.Errors)
                    {
                        if (modelError.ErrorMessage != "The value '' is invalid.") modelErrors.Add(modelError.ErrorMessage);
                    }
                }
                Response.StatusCode = 400;
                return Json(modelErrors);
            }

            if (ModelState.IsValid)
            {
                _context.Add(poll);
                await _context.SaveChangesAsync();
            }
            var pollOptions = form.Where(x => x.Key.Contains("field") && x.Value != "");
            foreach (var option in pollOptions)
            {
                var addOption = new PollOption
                {
                    OptionText = option.Value,
                    Votes = 0,
                    PollID = poll.ID
                };
                _context.PollOption.Add(addOption);
                await _context.SaveChangesAsync();

            }
            await Notification(poll.PacID, poll.ID, poll.ExpiryDate.ToString());

            return Json(new { redirectID = "/Polls/Details/" + poll.ID });
        
        }

        // GET: Polls/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Poll == null)
            {
                return NotFound();
            }

            var poll = await _context.Poll.FindAsync(id);
            if (poll == null)
            {
                return NotFound();
            }
            ViewData["PacID"] = new SelectList(_context.PAC, "ID", "PACName", poll.PacID);
            return View(poll);
        }

        // POST: Polls/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Question,isActive,ExpiryDate,PacID")] Poll poll)
        {
            if (id != poll.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(poll);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PollExists(poll.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Poll modified.";
                return RedirectToAction("Details", "Polls", new { id });

            }
            ViewData["PacID"] = new SelectList(_context.PAC, "ID", "PACName", poll.PacID);
            TempData["FailureMessage"] = "Unable to edit Poll please try again.";
            return View(poll);
        }

        // GET: Polls/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Poll == null)
            {
                return NotFound();
            }

            var poll = await _context.Poll
                .Include(p => p.PAC)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (poll == null)
            {
                return NotFound();
            }

            return View(poll);
        }

        // POST: Polls/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Poll == null)
            {
                return Problem("Entity set 'PAC_Context.Poll'  is null.");
            }
            var poll = await _context.Poll.FindAsync(id);
            var pollOption = await _context.PollOption.Where(m=>m.PollID == poll.ID).ToListAsync();
            var pollOptionVote = await _context.PollVote.Where(m => m.PollID == poll.ID).ToListAsync();
            if (poll != null)
            {
                _context.Poll.Remove(poll);
                foreach(PollOption op in pollOption)
                {
                    _context.PollOption.Remove(op);
                }
                foreach (PollVote vote in pollOptionVote)
                {
                    _context.PollVote.Remove(vote);
                }
            }
            
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Poll deleted!";
            return RedirectToAction(nameof(Index));
        }

        private bool PollExists(int id)
        {
          return _context.Poll.Any(e => e.ID == id);
        }

        // GET: PollOptions/Create
        public IActionResult Add(int? PollID , string PollQuestion)
        {
            if (!PollID.HasValue)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewData["PollQuestion"] = PollQuestion;

            PollOption pollOption = new PollOption()
            {
                PollID = PollID.GetValueOrDefault()
            };

            ViewData["PollID"] = new SelectList(_context.Poll, "ID", "ID");
            return View(pollOption);
        }

        // POST: PollOptions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("ID,OptionText,Votes,PollID")] PollOption pollOption, string PollQuestion)
        {
            var id = pollOption.PollID.ToString();
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(pollOption);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", "Polls", new { id });

                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                    "persists see your system administrator.");
            }
            ViewData["PollID"] = new SelectList(_context.Poll, "ID", "ID", pollOption.PollID);
            ViewData["PollQuestion"] = PollQuestion;
            TempData["SuccessMessage"] = "Poll option added.";
            return View(pollOption);
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }
        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }
        public async Task<IActionResult> Notification(int id,int pollID,string expiryDate)
        {
            int folksCount = 0;
            var callbackUrl = $"{ this.Request.Scheme}://{this.Request.Host}/PollVoting/Vote/"+pollID;
            List<EmailAddress> folks = (from p in _context.Members.Where(d => d.PACID == id && d.IsArchived == false)
                                        select new EmailAddress
                                        {
                                            Name = p.FullName,
                                            Address = p.Email
                                        }).ToList();
            folksCount = folks.Count();
            if (folksCount > 0)
            {
                var msg = new EmailMessage()
                {
                    ToAddresses = folks,
                    Subject = "New Poll has been added, Come vote!",
                    Content = UserPollNotifEmail.UserPollNotif(HtmlEncoder.Default.Encode(callbackUrl).ToString(), expiryDate)
                };
                await _emailSender.SendToManyAsync(msg);

            }

            TempData["SuccessMessage"] = $"{folks.Count()}" + " Member(s) have been notified via email about the new Poll.";
            return View();

        }
        private SelectList PACSelectList(int? selectedID)
        {
            return new SelectList(_context.PAC
                .OrderBy(a => a.PACName), "ID", "PACName", selectedID);
        }

        private void PopulateDropDownLists(PAC pac = null)
        {
            ViewData["PACID"] = PACSelectList(pac?.ID);
        }
    }
}
