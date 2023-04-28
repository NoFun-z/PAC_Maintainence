using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;
using NiagaraCollegeProject.ViewModels;
using NiagaraCollegeProject.Utilities;
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
                .Include(m => m.MemberPhoto)
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
                   NCGraduate = e.NCGraduate,
                   SignUpDate = e.SignUpDate,
                   ReNewDate_ = e.ReNewDate_,
                   RenewalDueBy = e.RenewalDueBy,
                   MemberRole = (MemberVM.MemberPacRole)e.MemberRole,
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
                    NCGraduate = e.NCGraduate,
                    MemberRole = (MemberVM.MemberPacRole)e.MemberRole,
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string chkRemoveImage, IFormFile thePicture, List<IFormFile> theFiles)
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
                  NCGraduate = e.NCGraduate,
                  MemberRole = (MemberVM.MemberPacRole)e.MemberRole,
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
            var memberToUpdate = await _context.Members
                .Include(m => m.Subscriptions)
                .Include(m => m.ActionItems)
                .Include(m => m.PAC)
                .Include(m => m.AcademicDivision)
                .Include(m => m.Salutation)
                .Include(d => d.MemberDocuments)
                .Include(d => d.MemberPhoto)
                .FirstOrDefaultAsync(m => m.ID == id);

            //Note: Using TryUpdateModel we do not need to invoke the ViewModel
            //Only allow some properties to be updated
            if (await TryUpdateModelAsync<Member>(memberToUpdate, "",
              e => e.FirstName, e => e.LastName, e => e.PhoneNumber, e => e.Email, e => e.EducationSummary,
                e => e.OccupationalSummary, e => e.StreetAddress, e => e.Province, e => e.City, e => e.PostalCode,
                e => e.IsArchived, e => e.NCGraduate, e => e.PreferredContact, e => e.CompanyPositionTitle,
                e => e.CompanyCity, e => e.AcademicDivisionID, e => e.PACID,
                e => e.SalutationID, e => e.CompanyEmail, e => e.CompanyName, e => e.CompanyPostalCode, e => e.CompanyStreetAddress, e => e.CompanyProvince
                , e => e.CompanyPhoneNumber, e => e.MemberPhoto, e => e.Subscriptions,eEndSize=>eEndSize.MemberRole))
            {
                try
                {
                    if (EmailValidator.IsValidEmail(memberToUpdate.Email) == false)
                    {
                        ModelState.AddModelError(nameof(memberToUpdate.Email), "Please enter a valid email. Ex. test1234@hotmail.com");
                        ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                        return View(member);
                    }
                    if (EmailValidator.IsValidEmail(memberToUpdate.CompanyEmail) == false)
                    {
                        ModelState.AddModelError(nameof(memberToUpdate.CompanyEmail), "Please enter a valid company email. Ex. test1234@hotmail.com");
                        ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                        return View(member);
                    }
                    if (chkRemoveImage != null)
                    {
                        memberToUpdate.MemberPhoto = null;
                    }
                    else
                    {
                        await AddPicture(memberToUpdate, thePicture);
                    }
                    await AddDocumentsAsync(memberToUpdate, theFiles);

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
                catch (ArgumentNullException)
                {
                    ModelState.AddModelError("", "Some fields are missing please make sure all required data is supplied.");
                    ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                    return View(member);
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes please try again, if this continues contact your admin.");
                    ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                    return View(member);
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
            return RedirectToAction(nameof(Details));
        }

        //RENEW
        // GET: EmployeeAccount/Renew/5
        public async Task<IActionResult> Renew(int? id)
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
                    NCGraduate = e.NCGraduate,
                    MemberRole = (MemberVM.MemberPacRole)e.MemberRole,
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

			//GET THE MEMBER INFO
			Member mem = _context.Members
				.Include(m => m.Subscriptions)
				.Include(m => m.ActionItems)
				.Include(m => m.PAC)
				.Include(m => m.AcademicDivision)
				.Include(m => m.Salutation)
				.Include(d => d.MemberDocuments)
				.Include(d => d.MemberPhoto)
				.Where(d => d.ID == id.GetValueOrDefault())
				.AsNoTracking()
				.FirstOrDefault();

			ViewBag.Mem = mem;

			return View(member);
        }

        // POST: EmployeeAccount/Renew/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renew(int id, string chkRemoveImage, IFormFile thePicture, List<IFormFile> theFiles)
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
                  NCGraduate = e.NCGraduate,
                  MemberRole = (MemberVM.MemberPacRole)e.MemberRole,
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
            var memberToUpdate = await _context.Members
                .Include(m => m.Subscriptions)
                .Include(m => m.ActionItems)
                .Include(m => m.PAC)
                .Include(m => m.AcademicDivision)
                .Include(m => m.Salutation)
                .Include(d => d.MemberDocuments)
                .Include(d => d.MemberPhoto)
                .FirstOrDefaultAsync(m => m.ID == id);

            //Note: Using TryUpdateModel we do not need to invoke the ViewModel
            //Only allow some properties to be updated
            if (await TryUpdateModelAsync<Member>(memberToUpdate, "",
              e => e.FirstName, e => e.LastName, e => e.PhoneNumber, e => e.Email, e => e.ReNewDate_, e => e.EducationSummary,
                e => e.OccupationalSummary, e => e.StreetAddress, e => e.Province, e => e.City, e => e.PostalCode,
                e => e.IsArchived, e => e.NCGraduate, e => e.PreferredContact, e => e.CompanyPositionTitle,
                e => e.CompanyCity, e => e.AcademicDivisionID, e => e.PACID,
                e => e.SalutationID, e => e.CompanyEmail, e => e.CompanyName, e => e.CompanyPostalCode, e => e.CompanyStreetAddress, e => e.CompanyProvince
                , e => e.CompanyPhoneNumber, e => e.MemberPhoto, e => e.Subscriptions, eEndSize => eEndSize.MemberRole))
            {
                try
                {
                    if (EmailValidator.IsValidEmail(memberToUpdate.Email) == false)
                    {
                        ModelState.AddModelError(nameof(memberToUpdate.Email), "Please enter a valid email. Ex. test1234@hotmail.com");
                        ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                        return View(member);
                    }
                    if (EmailValidator.IsValidEmail(memberToUpdate.CompanyEmail) == false)
                    {
                        ModelState.AddModelError(nameof(memberToUpdate.CompanyEmail), "Please enter a valid company email. Ex. test1234@hotmail.com");
                        ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                        return View(member);
                    }
                    if (chkRemoveImage != null)
                    {
                        memberToUpdate.MemberPhoto = null;
                    }
                    else
                    {
                        await AddPicture(memberToUpdate, thePicture);
                    }
                    await AddDocumentsAsync(memberToUpdate, theFiles);

                    memberToUpdate.ReNewDate_ = DateTime.Now;
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
                catch (ArgumentNullException)
                {
                    ModelState.AddModelError("", "Some fields are missing please make sure all required data is supplied.");
                    ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                    return View(member);
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes please try again, if this continues contact your admin.");
                    ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                    return View(member);
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
            return RedirectToAction(nameof(Details));
        }

        //NOT RENEW ACTION
        // GET: EmployeeAccount/NotRenew/5
        public async Task<IActionResult> NotRenew(int? id)
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
                    NCGraduate = e.NCGraduate,
                    MemberRole = (MemberVM.MemberPacRole)e.MemberRole,
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

            //GET THE MEMBER INFO
            Member mem = _context.Members
                .Include(m => m.Subscriptions)
                .Include(m => m.ActionItems)
                .Include(m => m.PAC)
                .Include(m => m.AcademicDivision)
                .Include(m => m.Salutation)
                .Include(d => d.MemberDocuments)
                .Include(d => d.MemberPhoto)
                .Where(d => d.ID == id.GetValueOrDefault())
                .AsNoTracking()
                .FirstOrDefault();

            ViewBag.Mem = mem;

            return View(member);
        }

        // POST: EmployeeAccount/NotRenew/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NotRenew(int id)
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
                  NCGraduate = e.NCGraduate,
                  MemberRole = (MemberVM.MemberPacRole)e.MemberRole,
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
            var memberToUpdate = await _context.Members
                .Include(m => m.Subscriptions)
                .Include(m => m.ActionItems)
                .Include(m => m.PAC)
                .Include(m => m.AcademicDivision)
                .Include(m => m.Salutation)
                .Include(d => d.MemberDocuments)
                .Include(d => d.MemberPhoto)
                .FirstOrDefaultAsync(m => m.ID == id);

            //Note: Using TryUpdateModel we do not need to invoke the ViewModel
            //Only allow some properties to be updated
            if (await TryUpdateModelAsync<Member>(memberToUpdate, "", e => e.NotRenewing))
            {
                try
                {
                    memberToUpdate.NotRenewing = true;
                    _context.Update(memberToUpdate);
                    await _context.SaveChangesAsync();
                    UpdateUserNameCookie(memberToUpdate.FullName);
                    return RedirectToAction("Renew", new { id = id });
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
                catch (ArgumentNullException)
                {
                    ModelState.AddModelError("", "Some fields are missing please make sure all required data is supplied.");
                    ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                    return View(member);
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes please try again, if this continues contact your admin.");
                    ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberToUpdate.SalutationID);
                    return View(member);
                }
                catch (DbUpdateException)
                {
                    //Since we do not allow changing the email, we cannot introduce a duplicate
                    ModelState.AddModelError("", "Something went wrong in the database.");
                }
            }

            return RedirectToAction("Renew", new { id = id });
        }

        private async Task AddDocumentsAsync(Member member, List<IFormFile> theFiles)
        {
            foreach (var f in theFiles)
            {
                if (f != null)
                {
                    string mimeType = f.ContentType;
                    string fileName = Path.GetFileName(f.FileName);
                    long fileLength = f.Length;
                    //Note: you could filter for mime types if you only want to allow
                    //certain types of files.  I am allowing everything.
                    if (!(fileName == "" || fileLength == 0))//Looks like we have a file!!!
                    {
                        MemberDocuments d = new MemberDocuments();
                        using (var memoryStream = new MemoryStream())
                        {
                            await f.CopyToAsync(memoryStream);
                            d.FileContent.Content = memoryStream.ToArray();
                        }
                        d.MimeType = mimeType;
                        d.FileName = fileName;
                        member.MemberDocuments.Add(d);
                    };
                }
            }
        }
        private async Task AddPicture(Member member, IFormFile thePicture)
        {
            //Get the picture and save it with the Patient (2 sizes)
            if (thePicture != null)
            {
                string mimeType = thePicture.ContentType;
                long fileLength = thePicture.Length;
                if (!(mimeType == "" || fileLength == 0))//Looks like we have a file!!!
                {
                    if (mimeType.Contains("image"))
                    {
                        using var memoryStream = new MemoryStream();
                        await thePicture.CopyToAsync(memoryStream);
                        var pictureArray = memoryStream.ToArray();//Gives us the Byte[]

                        //Check if we are replacing or creating new
                        if (member.MemberPhoto != null)
                        {
                            //We already have pictures so just replace the Byte[]
                            member.MemberPhoto.Content = ResizeImage.shrinkImageWebp(pictureArray, 500, 600);
                        }
                        else //No pictures saved so start new
                        {
                            member.MemberPhoto = new MemberPhoto
                            {
                                Content = ResizeImage.shrinkImageWebp(pictureArray, 500, 600),
                                MimeType = "image/webp"
                            };
                        }
                    }
                }
            }
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
