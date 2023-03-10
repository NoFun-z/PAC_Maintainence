using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;
using NiagaraCollegeProject.Utilities;
using NiagaraCollegeProject.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using WebPush;

namespace NiagaraCollegeProject.Controllers
{
    [Authorize]
    public class MemberAccountController : Controller
    {
        //Specialized controller just used to allow an 
        //Authenticated user to maintain their own account details.

        private readonly PAC_Context _context;
        private readonly IConfiguration _configuration;

        public MemberAccountController(PAC_Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: EmployeeAccount
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Details));
        }

        // GET: EmployeeAccount/Details/5
        public async Task<IActionResult> Details()
        {
            if (String.IsNullOrEmpty(_configuration.GetSection("VapidKeys")["PublicKey"]))
            {
                return RedirectToAction("GenerateKeys");
            }
            var member = await _context.Members
                .Include(m => m.Subscriptions)
                .Include(m => m.ActionItems)
                .Include(m => m.PAC)
                .Include(m => m.AcademicDivision)
                .Include(m => m.Salutation)
                .Include(d => d.MemberDocuments)
               .Where(e => e.Email == User.Identity.Name)
               .Select(e => new MemberVM
               {
                   ID = e.ID,
                   FirstName = e.FirstName,
                   LastName = e.LastName,
                   PhoneNumber = e.PhoneNumber,
                   Email = e.Email,
                   EducationSummary = e.EducationSummary,
                   OccupationalSummary = e.OccupationalSummary,
                   StreetAddress = e.StreetAddress,
                   Province = (MemberVM.Provinces)e.Province,
                   City = e.City,
                   PostalCode = e.PostalCode,
                   MemberStatus = e.MemberStatus,
                   NCGraduate = e.NCGraduate,
                   SignUpDate = e.SignUpDate,
                   RenewalDueBy = e.RenewalDueBy,
                   CompanyName = e.CompanyName,
                   CompanyStreetAddress = e.CompanyStreetAddress,
                   CompanyProvince = (MemberVM.Provinces)e.CompanyProvince,
                   CompanyCity = e.CompanyCity,
                   CompanyPostalCode = e.CompanyPostalCode,
                   CompanyPhoneNumber = e.CompanyPhoneNumber,
                   CompanyEmail = e.CompanyEmail,
                   PreferredContact = (MemberVM.Contact)e.PreferredContact,
                   CompanyPositionTitle = e.CompanyPositionTitle,
                   MemberDocuments = e.MemberDocuments,
                   ActionItems = e.ActionItems,
                   AcademicDivisionID = e.AcademicDivisionID,
                   AcademicDivision = e.AcademicDivision,
                   PACID = e.PACID,
                   PAC = e.PAC,
                   SalutationID = e.SalutationID,
                   Salutation = e.Salutation,
                   MemberPhoto = e.MemberPhoto,
                   NumberOfPushSubscriptions = e.Subscriptions.Count
               })
               .FirstOrDefaultAsync();
            if (member == null)
            {
                return NotFound();
            }

            ViewBag.PublicKey = _configuration.GetSection("VapidKeys")["PublicKey"];
            return View(member);
        }

        // GET: EmployeeAccount/ Create the record of the Push Subscription
        public IActionResult Push(int MemberID)
        {
            ViewBag.PublicKey = _configuration.GetSection("VapidKeys")["PublicKey"];
            ViewData["MemberID"] = MemberID;
            return View();
        }

        // POST: EmployeeAccount/ Create the record of the Push Subscription
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Push([Bind("PushEndpoint,PushP256DH,PushAuth,MemberID")] Subscription sub, string btnSubmit)
        {
            if (btnSubmit == "Unsubscribe")//Delete the subscription record
            {
                try
                {
                    var sToRemove = _context.Subscriptions.Where(s => s.PushAuth == sub.PushAuth
                        && s.PushEndpoint == sub.PushEndpoint
                        && s.PushAuth == sub.PushAuth).FirstOrDefault();
                    if (sToRemove != null)
                    {
                        _context.Subscriptions.Remove(sToRemove);
                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction(nameof(Details));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Error: Could not remove the record of the subscription.");
                }
            }
            else//Create the subscription record
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        _context.Add(sub);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Error: Could not create the record of the subscription.");
                }
            }
            ViewData["MemberID"] = sub.MemberID;
            return View(sub);
        }

        public IActionResult GenerateKeys()
        {
            var keys = VapidHelper.GenerateVapidKeys();
            ViewBag.PublicKey = keys.PublicKey;
            ViewBag.PrivateKey = keys.PrivateKey;
            return View();
        }

        // GET: EmployeeAccount/Edit/5
        public async Task<IActionResult> Edit()
        {
            var member = await _context.Members
                .Where(e => e.Email == User.Identity.Name)
                .Select(e => new MemberVM
                {
                    ID = e.ID,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    PhoneNumber = e.PhoneNumber,
                    Email = e.Email,
                    EducationSummary = e.EducationSummary,
                    OccupationalSummary = e.OccupationalSummary,
                    StreetAddress = e.StreetAddress,
                    Province = (MemberVM.Provinces)e.Province,
                    City = e.City,
                    PostalCode = e.PostalCode,
                    MemberStatus = e.MemberStatus,
                    NCGraduate = e.NCGraduate,
                    CompanyName = e.CompanyName,
                    CompanyStreetAddress = e.CompanyStreetAddress,
                    CompanyProvince = (MemberVM.Provinces)e.CompanyProvince,
                    CompanyCity = e.CompanyCity,
                    CompanyPostalCode = e.CompanyPostalCode,
                    CompanyPhoneNumber = e.CompanyPhoneNumber,
                    CompanyEmail = e.CompanyEmail,
                    PreferredContact = (MemberVM.Contact)e.PreferredContact,
                    CompanyPositionTitle = e.CompanyPositionTitle,
                    MemberDocuments = e.MemberDocuments,
                    ActionItems = e.ActionItems,
                    SalutationID = e.SalutationID,
                    Salutation = e.Salutation,
                    MemberPhoto = e.MemberPhoto,
                    Subscriptions = e.Subscriptions
                })
                .FirstOrDefaultAsync();
            if (member == null)
            {
                return NotFound();
            }

            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName");
            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName");
            ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle");
            return View(member);
        }

        // POST: EmployeeAccount/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var memberToUpdate = await _context.Members
                .FirstOrDefaultAsync(m => m.ID == id);

            //Note: Using TryUpdateModel we do not need to invoke the ViewModel
            //Only allow some properties to be updated
            if (await TryUpdateModelAsync<Member>(memberToUpdate, "",
              e => e.FirstName, e => e.LastName, e => e.PhoneNumber, e => e.Email, e => e.EducationSummary,
                e => e.OccupationalSummary, e => e.StreetAddress, e => e.Province, e => e.City, e => e.PostalCode,
                e => e.MemberStatus, e => e.NCGraduate, e => e.PreferredContact, e => e.CompanyPositionTitle,
                e => e.CompanyCity, e => e.AcademicDivisionID, e => e.PACID,
                e => e.SalutationID, e => e.CompanyEmail, e => e.CompanyName, e => e.CompanyPostalCode, e => e.CompanyStreetAddress, e => e.CompanyProvince
                , e => e.CompanyPhoneNumber, e => e.MemberPhoto, e => e.Subscriptions))
            {
                try
                {
                    _context.Update(memberToUpdate);
                    await _context.SaveChangesAsync();
                    UpdateUserNameCookie(memberToUpdate.FullName);
                    return RedirectToAction(nameof(Details));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(memberToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    //Since we do not allow changing the email, we cannot introduce a duplicate
                    ModelState.AddModelError("", "Something went wrong in the database.");
                }
            }

            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName");
            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName");
            ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle");
            return View(memberToUpdate);

        }

        public JsonResult GetPacList(int Id)
        {
            var result = _context.PAC
                .Where(r => r.AcademicDivisionID == Id).ToList();
            return Json(new SelectList(result, "ID", "PACName"));
        }

        private void UpdateUserNameCookie(string userName)
        {
            CookieHelper.CookieSet(HttpContext, "userName", userName, 960);
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.ID == id);
        }
    }
}
