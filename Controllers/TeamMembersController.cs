using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;
using NiagaraCollegeProject.ViewModels;
using System.Diagnostics.Metrics;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Drawing;
using System.Drawing.Printing;
using PagedList;
using System.Globalization;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using NiagaraCollegeProject.Utilities;

namespace NiagaraCollegeProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TeamMembersController : Controller
    {
        private readonly PAC_Context _context;
        private readonly ApplicationDbContext _identityContext;
        private readonly IMyEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public TeamMembersController(PAC_Context context,
            ApplicationDbContext identityContext, IMyEmailSender emailSender,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _identityContext = identityContext;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        // GET: Members
        public async Task<IActionResult> Index(int? page, int? pageSizeID, string successMessage)
        {
            if (!string.IsNullOrEmpty(successMessage))
            {
                ViewBag.SuccessMessage = successMessage;
            }

            var members = await _context.Members
                .Include(m => m.Subscriptions)
                .Include(m => m.ActionItems)
                .Include(m => m.AcademicDivision)
                .Include(m => m.MemberPhoto)
                .Include(m => m.Salutation)
                .Include(d => d.MemberDocuments)
                .Select(e => new MemberAdminVM
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
                    IsArchived = e.IsArchived,
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
                }).ToListAsync();

            foreach (var e in members)
            {
                var user = await _userManager.FindByEmailAsync(e.Email);
                if (user != null)
                {
                    e.UserRoles = (List<string>)await _userManager.GetRolesAsync(user);
                }
            };

            var filteredMember = members.Where(x => x.IsArchived == false);
            return View(filteredMember);
        }

        // GET: Member/Create
        public async Task<IActionResult> Create()
        {
            var member = await _context.Members
                .Include(m => m.Subscriptions)
                .Include(m => m.ActionItems)
                .Include(m => m.AcademicDivision).ThenInclude(m => m.PACs)
                .Include(m => m.Salutation)
                .Include(m => m.MemberPhoto)
                .Include(d => d.MemberDocuments)
                .Select(m => new MemberAdminVM
                {
                    EducationSummary = m.EducationSummary,
                    OccupationalSummary = m.OccupationalSummary,
                    StreetAddress = m.StreetAddress,
                    Province = (MemberVM.Provinces)m.Province,
                    City = m.City,
                    PostalCode = m.PostalCode,
                    NCGraduate = m.NCGraduate,
                    SignUpDate = m.SignUpDate,
                    ReNewDate_ = m.ReNewDate_,
                    RenewalDueBy = m.RenewalDueBy,
                    MemberRole = (MemberVM.MemberPacRole)m.MemberRole,
                    CompanyName = m.CompanyName,
                    CompanyStreetAddress = m.CompanyStreetAddress,
                    CompanyProvince = (MemberVM.Provinces)m.CompanyProvince,
                    CompanyCity = m.CompanyCity,
                    IsArchived = m.IsArchived,
                    CompanyPostalCode = m.CompanyPostalCode,
                    CompanyPhoneNumber = m.CompanyPhoneNumber,
                    CompanyEmail = m.CompanyEmail,
                    PreferredContact = (MemberVM.Contact)m.PreferredContact,
                    CompanyPositionTitle = m.CompanyPositionTitle,
                    ActionItems = m.ActionItems,
                    AcademicDivisionID = m.AcademicDivisionID,
                    AcademicDivision = m.AcademicDivision,
                    PACID = m.PACID,
                    PAC = m.PAC,
                    SalutationID = m.SalutationID,
                    Salutation = m.Salutation
                })
                .FirstOrDefaultAsync();

            PopulateAssignedRoleData(member);

            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName", 0);
            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName", 0);

            return View();
        }

        // POST: Members/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstName,LastName,Gender,PhoneNumber," +
            "Email,EducationSummary,OccupationalSummary,StreetAddress,Province,City,PostalCode," +
            "NCGraduate,SignUpDate,ReNewDate_,RenewalDueBy,PreferredContact,CompanyPositionTitle,CompanyName,CompanyStreetAddress,CompanyProvince,CompanyCity,CompanyPostalCode,CompanyPhoneNumber,CompanyName,CompanyEmail," +
            "AcademicDivisionID,PACID,SalutationID,MemberRole,isArchived")] Member member, string[] selectedRoles)
        {

            try
            {
                if (EmailValidator.IsValidEmail(member.Email) == false)
                {
                    ModelState.AddModelError(nameof(member.Email), "Please enter a valid email. Ex. test1234@hotmail.com");
                    //We are here because something went wrong and need to redisplay
                    MemberAdminVM memberAdminVM = new MemberAdminVM
                    {
                        ID = member.ID,
                        FirstName = member.FirstName,
                        LastName = member.LastName,
                        PhoneNumber = member.PhoneNumber,
                        Email = member.Email,
                        EducationSummary = member.EducationSummary,
                        OccupationalSummary = member.OccupationalSummary,
                        StreetAddress = member.StreetAddress,
                        Province = (MemberVM.Provinces)member.Province,
                        City = member.City,
                        PostalCode = member.PostalCode,
                        NCGraduate = member.NCGraduate,
                        SignUpDate = member.SignUpDate,
                        ReNewDate_ = member.ReNewDate_,
                        RenewalDueBy = member.RenewalDueBy,
                        IsArchived = member.IsArchived,
                        MemberRole = (MemberVM.MemberPacRole)member.MemberRole,
                        CompanyName = member.CompanyName,
                        CompanyStreetAddress = member.CompanyStreetAddress,
                        CompanyProvince = (MemberVM.Provinces)member.CompanyProvince,
                        CompanyCity = member.CompanyCity,
                        CompanyPostalCode = member.CompanyPostalCode,
                        CompanyPhoneNumber = member.CompanyPhoneNumber,
                        CompanyEmail = member.CompanyEmail,
                        PreferredContact = (MemberVM.Contact)member.PreferredContact,
                        CompanyPositionTitle = member.CompanyPositionTitle,
                        ActionItems = member.ActionItems,
                        AcademicDivisionID = member.AcademicDivisionID,
                        AcademicDivision = member.AcademicDivision,
                        PACID = member.PACID,
                        PAC = member.PAC,
                        SalutationID = member.SalutationID,
                        Salutation = member.Salutation,
                        NumberOfPushSubscriptions = member.Subscriptions.Count
                    };

                    foreach (var role in selectedRoles)
                    {
                        memberAdminVM.UserRoles.Add(role);
                    }
                    PopulateAssignedRoleData(memberAdminVM);

                    ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName", memberAdminVM.PACID);
                    ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName", memberAdminVM.AcademicDivisionID);
                    ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberAdminVM.SalutationID);

                    return View(memberAdminVM);
                }
                if (await _userManager.FindByEmailAsync(member.Email) == null)
                {
                 
                    if (ModelState.IsValid)
                    {
                        _context.Add(member);
                        await _context.SaveChangesAsync();

                        InsertIdentityUser(member.Email, selectedRoles);

                        //Send Email to new Employee - commented out till email configured
                        await InviteUserToResetPassword(member);
                        TempData["SuccessMessage"] = "Account for " + member.FullName + " was created and an email invitation has been sent.";
                        return RedirectToAction("Index", "Members", new { successMessage = "Account created successfully!" });
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Unable to add new member because the email entered is already attached to another user. Please try another email.");

                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("Email", "Unable to save changes. Remember, you cannot have duplicate Email addresses.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            //We are here because something went wrong and need to redisplay
            MemberAdminVM memberAdminVMError = new MemberAdminVM
            {
                ID = member.ID,
                FirstName = member.FirstName,
                LastName = member.LastName,
                PhoneNumber = member.PhoneNumber,
                Email = member.Email,
                EducationSummary = member.EducationSummary,
                OccupationalSummary = member.OccupationalSummary,
                StreetAddress = member.StreetAddress,
                Province = (MemberVM.Provinces)member.Province,
                City = member.City,
                PostalCode = member.PostalCode,
                NCGraduate = member.NCGraduate,
                SignUpDate = member.SignUpDate,
                ReNewDate_ = member.ReNewDate_,
                RenewalDueBy = member.RenewalDueBy,
                IsArchived = member.IsArchived,
                MemberRole = (MemberVM.MemberPacRole)member.MemberRole,
                CompanyName = member.CompanyName,
                CompanyStreetAddress = member.CompanyStreetAddress,
                CompanyProvince = (MemberVM.Provinces)member.CompanyProvince,
                CompanyCity = member.CompanyCity,
                CompanyPostalCode = member.CompanyPostalCode,
                CompanyPhoneNumber = member.CompanyPhoneNumber,
                CompanyEmail = member.CompanyEmail,
                PreferredContact = (MemberVM.Contact)member.PreferredContact,
                CompanyPositionTitle = member.CompanyPositionTitle,
                ActionItems = member.ActionItems,
                AcademicDivisionID = member.AcademicDivisionID,
                AcademicDivision = member.AcademicDivision,
                PACID = member.PACID,
                PAC = member.PAC,
                SalutationID = member.SalutationID,
                Salutation = member.Salutation,
                NumberOfPushSubscriptions = member.Subscriptions.Count
            };

            foreach (var role in selectedRoles)
            {
                memberAdminVMError.UserRoles.Add(role);
            }
            PopulateAssignedRoleData(memberAdminVMError);

            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName", memberAdminVMError.PACID);
            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName", memberAdminVMError.AcademicDivisionID);
            ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberAdminVMError.SalutationID);

            return View(memberAdminVMError);

        }


        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Members == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.Subscriptions)
                .Include(m => m.ActionItems)
                .Include(m => m.MemberPhoto)
                .Include(m => m.AcademicDivision).ThenInclude(m => m.PACs)
                .Include(m => m.Salutation)
                .Include(d => d.MemberDocuments)
                .Where(x => x.ID == id)
                .Select(m => new MemberAdminVM
                {
                    ID = m.ID,
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                    PhoneNumber = m.PhoneNumber,
                    Email = m.Email,
                    EducationSummary = m.EducationSummary,
                    OccupationalSummary = m.OccupationalSummary,
                    StreetAddress = m.StreetAddress,
                    Province = (MemberVM.Provinces)m.Province,
                    City = m.City,
                    PostalCode = m.PostalCode,
                    NCGraduate = m.NCGraduate,
                    MemberRole = (MemberVM.MemberPacRole)m.MemberRole,
                    CompanyName = m.CompanyName,
                    CompanyStreetAddress = m.CompanyStreetAddress,
                    CompanyProvince = (MemberVM.Provinces)m.CompanyProvince,
                    CompanyCity = m.CompanyCity,
                    CompanyPostalCode = m.CompanyPostalCode,
                    CompanyPhoneNumber = m.CompanyPhoneNumber,
                    CompanyEmail = m.CompanyEmail,
                    PreferredContact = (MemberVM.Contact)m.PreferredContact,
                    CompanyPositionTitle = m.CompanyPositionTitle,
                    ActionItems = m.ActionItems,
                    AcademicDivisionID = m.AcademicDivisionID,
                    AcademicDivision = m.AcademicDivision,
                    PACID = m.PACID,
                    IsArchived = m.IsArchived,
                    PAC = m.PAC,
                    SalutationID = m.SalutationID,
                    Salutation = m.Salutation,
                })
                .FirstOrDefaultAsync();

            if (member == null)
            {
                return NotFound();
            }

            //Get the user from the Identity system
            var user = await _userManager.FindByEmailAsync(member.Email);
            if (user != null)
            {
                //Add the current roles
                var r = await _userManager.GetRolesAsync(user);
                member.UserRoles = (List<string>)r;
            }
            PopulateAssignedRoleData(member);

            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName", member.PACID);
            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName", member.AcademicDivisionID);
            ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", member.SalutationID);

            return View(member);
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string[] selectedRoles, string chkRemoveImage, IFormFile thePicture, List<IFormFile> theFiles)
        {
            var memberToUpdate = await _context.Members
                .Include(m => m.Subscriptions)
                .Include(m => m.ActionItems)
                .Include(m => m.MemberPhoto)
                .Include(m => m.AcademicDivision).ThenInclude(m => m.PACs)
                .Include(m => m.Salutation)
                .Include(d => d.MemberDocuments)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (memberToUpdate == null)
            {
                return NotFound(new { message = "Something went wrong looking for that member, please try again." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Something went wrong please try again." });
            }

            //Note the current Email and member status
            bool memberIsArchived = memberToUpdate.IsArchived;
            string databaseEmail = memberToUpdate.Email;

            if (await TryUpdateModelAsync<Member>(memberToUpdate, "",
                e => e.FirstName, e => e.LastName, e => e.PhoneNumber, e => e.Email, e => e.EducationSummary,
                e => e.OccupationalSummary, e => e.StreetAddress, e => e.Province, e => e.City, e => e.PostalCode,
                e => e.IsArchived, e => e.NCGraduate, e => e.PreferredContact, e => e.CompanyPositionTitle,
                e => e.CompanyCity, e => e.AcademicDivisionID, e => e.PACID,
                e => e.SalutationID, e => e.CompanyEmail, e => e.CompanyName, e => e.CompanyPostalCode, e => e.CompanyStreetAddress, e => e.CompanyProvince
                , e => e.CompanyPhoneNumber, e=>e.MemberRole))
            {
                try
                {
                    if (EmailValidator.IsValidEmail(memberToUpdate.Email) == false)
                    {
                        ModelState.AddModelError(nameof(memberToUpdate.Email), "Please enter a valid email. Ex. test1234@hotmail.com");
                        //We are here because something went wrong and need to redisplay
                        MemberAdminVM memberAdminVM = new MemberAdminVM
                        {
                            ID = memberToUpdate.ID,
                            FirstName = memberToUpdate.FirstName,
                            LastName = memberToUpdate.LastName,
                            PhoneNumber = memberToUpdate.PhoneNumber,
                            Email = memberToUpdate.Email,
                            EducationSummary = memberToUpdate.EducationSummary,
                            OccupationalSummary = memberToUpdate.OccupationalSummary,
                            StreetAddress = memberToUpdate.StreetAddress,
                            Province = (MemberVM.Provinces)memberToUpdate.Province,
                            City = memberToUpdate.City,
                            PostalCode = memberToUpdate.PostalCode,
                            NCGraduate = memberToUpdate.NCGraduate,
                            SignUpDate = memberToUpdate.SignUpDate,
                            ReNewDate_ = memberToUpdate.ReNewDate_,
                            RenewalDueBy = memberToUpdate.RenewalDueBy,
                            IsArchived = memberToUpdate.IsArchived,
                            MemberRole = (MemberVM.MemberPacRole)memberToUpdate.MemberRole,
                            CompanyName = memberToUpdate.CompanyName,
                            CompanyStreetAddress = memberToUpdate.CompanyStreetAddress,
                            CompanyProvince = (MemberVM.Provinces)memberToUpdate.CompanyProvince,
                            CompanyCity = memberToUpdate.CompanyCity,
                            CompanyPostalCode = memberToUpdate.CompanyPostalCode,
                            CompanyPhoneNumber = memberToUpdate.CompanyPhoneNumber,
                            CompanyEmail = memberToUpdate.CompanyEmail,
                            PreferredContact = (MemberVM.Contact)memberToUpdate.PreferredContact,
                            CompanyPositionTitle = memberToUpdate.CompanyPositionTitle,
                            ActionItems = memberToUpdate.ActionItems,
                            AcademicDivisionID = memberToUpdate.AcademicDivisionID,
                            AcademicDivision = memberToUpdate.AcademicDivision,
                            PACID = memberToUpdate.PACID,
                            PAC = memberToUpdate.PAC,
                            SalutationID = memberToUpdate.SalutationID,
                            Salutation = memberToUpdate.Salutation,
                            NumberOfPushSubscriptions = memberToUpdate.Subscriptions.Count
                        };

                        foreach (var role in selectedRoles)
                        {
                            memberAdminVM.UserRoles.Add(role);
                        }
                        PopulateAssignedRoleData(memberAdminVM);

                        ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName", memberAdminVM.PACID);
                        ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName", memberAdminVM.AcademicDivisionID);
                        ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberAdminVM.SalutationID);

                        return View(memberAdminVM);
                    }
                        await AddPicture(memberToUpdate, thePicture);
                    await AddDocumentsAsync(memberToUpdate, theFiles);
                    await _context.SaveChangesAsync();

                    if (chkRemoveImage != null)
                    {
                        memberToUpdate.MemberPhoto = null;
                    }
                    else
                    {
                        await AddPicture(memberToUpdate, thePicture);
                    }

                    if (memberToUpdate.Email != databaseEmail)
                    {
                        //Add the new login with the selected roles
                        InsertIdentityUser(memberToUpdate.Email, selectedRoles);
                        await AdminChangedUserEmail(memberToUpdate);

                        //This deletes the user's old login from the security system
                        await DeleteIdentityUser(databaseEmail);
                    }
                    else
                    {
                        //Finially, Still Active and no change to Email so just Update
                        await UpdateUserRoles(selectedRoles, memberToUpdate.Email);
                    }
                    TempData["SuccessMessage"] = "Member edited successfully!";
                    return RedirectToAction("Index", "Members", new { successMessage = "Account edited successfully!" });
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

                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        ModelState.AddModelError("Email", "Email is already attached to another user. Please try another email.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }
            //We are here because something went wrong and need to redisplay
            MemberAdminVM memberAdminVMError = new MemberAdminVM
            {
                ID = memberToUpdate.ID,
                FirstName = memberToUpdate.FirstName,
                LastName = memberToUpdate.LastName,
                PhoneNumber = memberToUpdate.PhoneNumber,
                Email = memberToUpdate.Email,
                EducationSummary = memberToUpdate.EducationSummary,
                OccupationalSummary = memberToUpdate.OccupationalSummary,
                StreetAddress = memberToUpdate.StreetAddress,
                Province = (MemberVM.Provinces)memberToUpdate.Province,
                City = memberToUpdate.City,
                IsArchived = memberToUpdate.IsArchived,
                PostalCode = memberToUpdate.PostalCode,
                NCGraduate = memberToUpdate.NCGraduate,
                SignUpDate = memberToUpdate.SignUpDate,
                RenewalDueBy = memberToUpdate.RenewalDueBy,
                MemberRole = (MemberVM.MemberPacRole)memberToUpdate.MemberRole,
                CompanyName = memberToUpdate.CompanyName,
                CompanyStreetAddress = memberToUpdate.CompanyStreetAddress,
                CompanyProvince = (MemberVM.Provinces)memberToUpdate.CompanyProvince,
                CompanyCity = memberToUpdate.CompanyCity,
                CompanyPostalCode = memberToUpdate.CompanyPostalCode,
                CompanyPhoneNumber = memberToUpdate.CompanyPhoneNumber,
                CompanyEmail = memberToUpdate.CompanyEmail,
                PreferredContact = (MemberVM.Contact)memberToUpdate.PreferredContact,
                CompanyPositionTitle = memberToUpdate.CompanyPositionTitle,
                ActionItems = memberToUpdate.ActionItems,
                AcademicDivisionID = memberToUpdate.AcademicDivisionID,
                AcademicDivision = memberToUpdate.AcademicDivision,
                PACID = memberToUpdate.PACID,
                PAC = memberToUpdate.PAC,
                SalutationID = memberToUpdate.SalutationID,
                Salutation = memberToUpdate.Salutation,
            };
            foreach (var role in selectedRoles)
            {
                memberAdminVMError.UserRoles.Add(role);
            }
            PopulateAssignedRoleData(memberAdminVMError);

            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName", memberAdminVMError.PACID);
            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName", memberAdminVMError.AcademicDivisionID);
            ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberAdminVMError.SalutationID);

            return View(memberAdminVMError);
        }

        public JsonResult GetPacList(int Id)
        {
            var result = _context.PAC
                .Where(r => r.AcademicDivisionID == Id).OrderBy(a => a.PACName).ToList();
            return Json(new SelectList(result, "ID", "PACName"));
        }

        private void PopulateAssignedRoleData(MemberAdminVM member)
        {//Prepare checkboxes for all Roles
            var allRoles = _identityContext.Roles;
            var currentRoles = member.UserRoles;
            var viewModel = new List<RoleVM>();
            foreach (var r in allRoles)
            {
                viewModel.Add(new RoleVM
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    Assigned = currentRoles.Contains(r.Name)
                });
            }
            ViewBag.Roles = viewModel;
        }

        private async Task UpdateUserRoles(string[] selectedRoles, string Email)
        {
            var _user = await _userManager.FindByEmailAsync(Email);//IdentityUser
            if (_user != null)
            {
                var UserRoles = (List<string>)await _userManager.GetRolesAsync(_user);//Current roles user is in

                if (selectedRoles == null)
                {
                    //No roles selected so just remove any currently assigned
                    foreach (var r in UserRoles)
                    {
                        await _userManager.RemoveFromRoleAsync(_user, r);
                    }
                }
                else
                {
                    //At least one role checked so loop through all the roles
                    //and add or remove as required

                    //We need to do this next line because foreach loops don't always work well
                    //for data returned by EF when working async.  Pulling it into an IList<>
                    //first means we can safely loop over the colleciton making async calls and avoid
                    //the error 'New transaction is not allowed because there are other threads running in the session'
                    IList<IdentityRole> allRoles = _identityContext.Roles.ToList<IdentityRole>();

                    foreach (var r in allRoles)
                    {
                        if (selectedRoles.Contains(r.Name))
                        {
                            if (!UserRoles.Contains(r.Name))
                            {
                                await _userManager.AddToRoleAsync(_user, r.Name);
                            }
                        }
                        else
                        {
                            if (UserRoles.Contains(r.Name))
                            {
                                await _userManager.RemoveFromRoleAsync(_user, r.Name);
                            }
                        }
                    }
                }
            }
        }

        private void InsertIdentityUser(string Email, string[]? selectedRoles)
        {
            //Create the IdentityUser in the IdentitySystem
            //Note: this is similar to what we did in ApplicationSeedData
            if (_userManager.FindByEmailAsync(Email).Result == null)
            {
                IdentityUser user = new IdentityUser
                {
                    UserName = Email,
                    Email = Email,
                    EmailConfirmed = true //since we are creating it!
                };
                //Create a random password with a default 8 characters
                string password = MakePassword.Generate();
                IdentityResult result = _userManager.CreateAsync(user, password).Result;

                if (result.Succeeded)
                {
                    foreach (string role in selectedRoles)
                    {
                        _userManager.AddToRoleAsync(user, role).Wait();
                    }
                }
            }
            else
            {
                TempData["message"] = "The Login Account for " + Email + " was already in the system.";
            }
        }

        private async Task DeleteIdentityUser(string Email)
        {
            var userToDelete = await _identityContext.Users.Where(u => u.Email == Email).FirstOrDefaultAsync();
            if (userToDelete != null)
            {
                _identityContext.Users.Remove(userToDelete);
                await _identityContext.SaveChangesAsync();
            }
        }

        private async Task<IActionResult> InviteUserToResetPassword(Member member)
        {
            var memberSchool = _context.AcademicDivisions.Where(m=>m.ID== member.AcademicDivisionID).FirstOrDefault();
            var memberPAC = _context.PAC.Where(m => m.ID == member.PACID).FirstOrDefault();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(member.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }
                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(
                    member.Email,
                    "Finish Creating Your Niagara College PAC Account!",
                    NewUserEmailTemplate.NewUserTemplate(HtmlEncoder.Default.Encode(callbackUrl).ToString(), member.FullName, memberPAC.PACName, memberSchool.DivisionName));

                return RedirectToPage("./ForgotPasswordConfirmation");
            }
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        private async Task<IActionResult> AdminChangedUserEmail(Member member)
        {
            

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(member.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }
                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(
                    member.Email,
                    "Niagara College Support",
                    AdminChangedUserPasswordEmail.AdminChangedUserPasswordTemplate(HtmlEncoder.Default.Encode(callbackUrl).ToString(),member.FullName));

                return RedirectToPage("./ForgotPasswordConfirmation");
            }
            return RedirectToPage("./ForgotPasswordConfirmation");
        }


        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Members == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.PAC)
                .Include(m => m.AcademicDivision)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Members == null)
            {
                return Problem("Entity set 'PAC_Context.Members' is null.");
            }
            var member = await _context.Members.FindAsync(id);

            if (member != null)
            {
                member.IsArchived = true;
                //Deactivating them so delete the IdentityUser
                //This deletes the user's login from the security system
                //await DeleteIdentityUser(member.Email);

                _context.Entry(member).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Account archived successfully!";

                return RedirectToAction("Index", "Members", new { successMessage = "Account Archived successfully!" });
            }

            //var members = membe

            return RedirectToAction("Index", "Members");
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

        [HttpPost]
        public async Task<IActionResult> InsertFromExcel(IFormFile theExcel)
        {
            //Note: This is a very basic example and has 
            //no ERROR HANDLING.  It also assumes that
            //duplicate values are allowed, both in the 
            //uploaded data and the DbSet.
            string[] selectedRoles = { "Staff" };
            if (theExcel == null)
            {
                TempData["FailureMessage"] = "Please ensure you have selected an Excel file.";
            }

            else
            {
                try
                {
                    ExcelPackage excel;
                    using (var memoryStream = new MemoryStream())
                    {
                        await theExcel.CopyToAsync(memoryStream);
                        excel = new ExcelPackage(memoryStream);
                    }
                    //If Kait is using her own template which the members list is in the 4th worksheet then change Worksheets[3]
                    //If Kait has to use our own template that can be downloaded on site, then keep it like this Worksheets[0]
                    var workSheet = excel.Workbook.Worksheets[0];
                    var start = workSheet.Row(3);
                    var end = workSheet.Row(16);
                    var cStart = workSheet.Row(20);
                    var cEnd = workSheet.Row(33);

                    //Start a new list to hold imported objects
                    List<Member> MemberAccounts = new List<Member>();

                    List<string[]> MemName = new List<string[]>();
                    for (int row = cStart.Row; row < cEnd.Row; row++)
                    {
                        MemName.Add(workSheet.Cells[row, 3].Text.Split(" "));
                    }


                    for (int row = start.Row; row <= end.Row && row < MemName.Count + 3; row++)
                    {
                        if (workSheet.Cells[row, 4].Text == "" || workSheet.Cells[row, 4].Text == null)
                        {
                            if (MemName[row - 3].Length > 2)
                            {
                                Member mem = new Member
                                {
                                    FirstName = MemName[row - 3][0],
                                    LastName = MemName[row - 3][2],
                                    Email = MemName[row - 3][0] + MemName[row - 3][2] + "@outlook.com",
                                    SignUpDate = DateTime.Today,
                                    AcademicDivisionID = 1,
                                    PACID = 1
                                };
                                MemberAccounts.Add(mem);
                            }
                            else
                            {
                                Member mem = new Member
                                {
                                    FirstName = MemName[row - 3][0],
                                    LastName = MemName[row - 3][1],
                                    Email = MemName[row - 3][0] + MemName[row - 3][1] + "@outlook.com",
                                    SignUpDate = DateTime.Today,
                                    AcademicDivisionID = 1,
                                    PACID = 1
                                };
                                MemberAccounts.Add(mem);
                            }
                        }
                        else
                        {
                            // Row by row...
                            if (MemName[row - 3].Length > 2)
                            {
                                if (!workSheet.Cells[row, 6].Text.Contains(",") || workSheet.Cells[row, 6].Text.Split(" ").Length == 2)
                                {
                                    if (workSheet.Cells[row, 6].Text.Length < 6)
                                    {
                                        int year = int.Parse(workSheet.Cells[row, 6].Text);
                                        DateTime time = Convert.ToDateTime(new DateTime(year, 1, 1).ToString("yyyy-MM-dd"));
                                        Member mem = new Member
                                        {
                                            FirstName = MemName[row - 3][0],
                                            LastName = MemName[row - 3][2],
                                            Email = MemName[row - 3][0] + MemName[row - 3][2] + "@outlook.com",
                                            ReNewDate_ = Convert.ToDateTime(DateTime.ParseExact(workSheet.Cells[row, 4].Text, "MMMM d, yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")),
                                            SignUpDate = time,
                                            PreferredContact = workSheet.Cells[row, 8].Text.ToUpper() == "WORK" ? Member.Contact.Work : Member.Contact.Personal,
                                            AcademicDivisionID = 1,
                                            PACID = 1
                                        };
                                        MemberAccounts.Add(mem);
                                    }
                                    else
                                    {
                                        if (workSheet.Cells[row, 6].Text.Split(" ")[0].Length < 7)
                                        {
                                            string month = "";
                                            switch (workSheet.Cells[row, 6].Text.Split(" ")[0].ToUpper())
                                            {
                                                case "JAN":
                                                    month = "January";
                                                    break;
                                                case "FEB":
                                                    month = "February";
                                                    break;
                                                case "MAR":
                                                    month = "Mars";
                                                    break;
                                                case "APR":
                                                    month = "April";
                                                    break;
                                                case "APRIL":
                                                    month = "April";
                                                    break;
                                                case "MAY":
                                                    month = "May";
                                                    break;
                                                case "JUN":
                                                    month = "June";
                                                    break;
                                                case "JUNE":
                                                    month = "June";
                                                    break;
                                                case "JUL":
                                                    month = "July";
                                                    break;
                                                case "JULY":
                                                    month = "July";
                                                    break;
                                                case "AUG":
                                                    month = "August";
                                                    break;
                                                case "AUGUST":
                                                    month = "August";
                                                    break;
                                                case "SEP":
                                                    month = "September";
                                                    break;
                                                case "OCT":
                                                    month = "October";
                                                    break;
                                                case "NOV":
                                                    month = "November";
                                                    break;
                                                case "DEC":
                                                    month = "December";
                                                    break;
                                            }

                                            string inputTime = month + " " + workSheet.Cells[row, 6].Text.Split(" ")[1];
                                            DateTime Time = DateTime.ParseExact(inputTime + " 01", "MMMM yyyy dd", CultureInfo.InvariantCulture);
                                            Member mem = new Member
                                            {
                                                FirstName = MemName[row - 3][0],
                                                LastName = MemName[row - 3][2],
                                                Email = MemName[row - 3][0] + MemName[row - 3][2] + "@outlook.com",
                                                ReNewDate_ = Convert.ToDateTime(DateTime.ParseExact(workSheet.Cells[row, 4].Text, "MMMM d, yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")),
                                                SignUpDate = Convert.ToDateTime(Time.ToString("yyyy-MM-dd")),
                                                PreferredContact = workSheet.Cells[row, 8].Text.ToUpper() == "WORK" ? Member.Contact.Work : Member.Contact.Personal,
                                                AcademicDivisionID = 1,
                                                PACID = 1
                                            };
                                            MemberAccounts.Add(mem);
                                        }
                                    }
                                }
                                else
                                {
                                    Member mem = new Member
                                    {
                                        FirstName = MemName[row - 3][0],
                                        LastName = MemName[row - 3][2],
                                        Email = MemName[row - 3][0] + MemName[row - 3][2] + "@outlook.com",
                                        ReNewDate_ = Convert.ToDateTime(DateTime.ParseExact(workSheet.Cells[row, 4].Text, "MMMM d, yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")),
                                        SignUpDate = Convert.ToDateTime(DateTime.ParseExact(workSheet.Cells[row, 6].Text, "MMMM d, yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")),
                                        PreferredContact = workSheet.Cells[row, 8].Text.ToUpper() == "WORK" ? Member.Contact.Work : Member.Contact.Personal,
                                        AcademicDivisionID = 1,
                                        PACID = 1
                                    };
                                    MemberAccounts.Add(mem);
                                }
                            }
                            else
                            {
                                if (!workSheet.Cells[row, 6].Text.Contains(",") || workSheet.Cells[row, 6].Text.Split(" ").Length == 2)
                                {
                                    if (workSheet.Cells[row, 6].Text.Length < 6)
                                    {
                                        int year = int.Parse(workSheet.Cells[row, 6].Text);
                                        DateTime time = Convert.ToDateTime(new DateTime(year, 1, 1).ToString("yyyy-MM-dd"));
                                        Member mem = new Member
                                        {
                                            FirstName = MemName[row - 3][0],
                                            LastName = MemName[row - 3][1],
                                            Email = MemName[row - 3][0] + MemName[row - 3][1] + "@outlook.com",
                                            ReNewDate_ = Convert.ToDateTime(DateTime.ParseExact(workSheet.Cells[row, 4].Text, "MMMM d, yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")),
                                            SignUpDate = time,
                                            PreferredContact = workSheet.Cells[row, 8].Text.ToUpper() == "WORK" ? Member.Contact.Work : Member.Contact.Personal,
                                            AcademicDivisionID = 1,
                                            PACID = 1
                                        };
                                        MemberAccounts.Add(mem);
                                    }
                                    else
                                    {
                                        if (workSheet.Cells[row, 6].Text.Split(" ")[0].Length < 7)
                                        {
                                            string month = "";
                                            switch (workSheet.Cells[row, 6].Text.Split(" ")[0].ToUpper())
                                            {
                                                case "JAN":
                                                    month = "January";
                                                    break;
                                                case "FEB":
                                                    month = "February";
                                                    break;
                                                case "MAR":
                                                    month = "Mars";
                                                    break;
                                                case "APR":
                                                    month = "April";
                                                    break;
                                                case "APRIL":
                                                    month = "April";
                                                    break;
                                                case "MAY":
                                                    month = "May";
                                                    break;
                                                case "JUN":
                                                    month = "June";
                                                    break;
                                                case "JUNE":
                                                    month = "June";
                                                    break;
                                                case "JUL":
                                                    month = "July";
                                                    break;
                                                case "JULY":
                                                    month = "July";
                                                    break;
                                                case "AUG":
                                                    month = "August";
                                                    break;
                                                case "AUGUST":
                                                    month = "August";
                                                    break;
                                                case "SEP":
                                                    month = "September";
                                                    break;
                                                case "OCT":
                                                    month = "October";
                                                    break;
                                                case "NOV":
                                                    month = "November";
                                                    break;
                                                case "DEC":
                                                    month = "December";
                                                    break;
                                            }

                                            string inputTime = month + " " + workSheet.Cells[row, 6].Text.Split(" ")[1];
                                            DateTime Time = DateTime.ParseExact(inputTime + " 01", "MMMM yyyy dd", CultureInfo.InvariantCulture);
                                            Member mem = new Member
                                            {
                                                FirstName = MemName[row - 3][0],
                                                LastName = MemName[row - 3][1],
                                                Email = MemName[row - 3][0] + MemName[row - 3][1] + "@outlook.com",
                                                ReNewDate_ = Convert.ToDateTime(DateTime.ParseExact(workSheet.Cells[row, 4].Text, "MMMM d, yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")),
                                                SignUpDate = Convert.ToDateTime(Time.ToString("yyyy-MM-dd")),
                                                PreferredContact = workSheet.Cells[row, 8].Text.ToUpper() == "WORK" ? Member.Contact.Work : Member.Contact.Personal,
                                                AcademicDivisionID = 1,
                                                PACID = 1
                                            };
                                            MemberAccounts.Add(mem);
                                        }
                                    }
                                }
                                else
                                {
                                    Member mem = new Member
                                    {
                                        FirstName = MemName[row - 3][0],
                                        LastName = MemName[row - 3][1],
                                        Email = MemName[row - 3][0] + MemName[row - 3][1] + "@outlook.com",
                                        ReNewDate_ = Convert.ToDateTime(DateTime.ParseExact(workSheet.Cells[row, 4].Text, "MMMM d, yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")),
                                        SignUpDate = Convert.ToDateTime(DateTime.ParseExact(workSheet.Cells[row, 6].Text, "MMMM d, yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")),
                                        PreferredContact = workSheet.Cells[row, 8].Text.ToUpper() == "WORK" ? Member.Contact.Work : Member.Contact.Personal,
                                        AcademicDivisionID = 1,
                                        PACID = 1
                                    };
                                    MemberAccounts.Add(mem);
                                }
                            }
                        }
                    }
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            _context.Members.AddRange(MemberAccounts);
                            TempData["SuccessMessage"] = $"{MemberAccounts.Count()} Acount(s) have been added to the database.";

                            _context.SaveChanges();

                            foreach (Member memobj in MemberAccounts)
                            {
                                InsertIdentityUser(memobj.Email, selectedRoles);
                            }


                            //Send Email to new Employee - commented out till email configured
                            //await InviteUserToResetPassword(member);

                            return RedirectToAction("Index", "Members");
                        }
                    }
                    catch (DbUpdateException dex)
                    {
                        if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                        {
                            ModelState.AddModelError("Email", "Unable to save changes. Remember, you cannot have duplicate Email addresses.");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                        }
                    }
                }
                catch
                {
                    TempData["FailureMessage"] = "There was an error trying to bulk upload member(s) with the Excel file selected, please try again.";

                }
            }


            //_context.Members.AddRange(MemberAccounts);
            //_context.SaveChanges();
            return RedirectToAction("Index", "Members");
        }

        // GET/POST: Notification/5
        public async Task<IActionResult> Notification(int? id, string Subject, string emailContent)
        {
            if (id == null)
            {
                return NotFound();
            }
            Member t = await _context.Members.FindAsync(id);

            ViewData["id"] = id;
            ViewData["FullName"] = t.FullName;

            if (string.IsNullOrEmpty(Subject) || string.IsNullOrEmpty(emailContent))
            {
                ViewData["Message"] = "You must enter both a Subject and some message Content before sending the message.";
            }
            else
            {
                int folksCount = 0;
                try
                {
                    //Send a Notice.
                    List<EmailAddress> folks = (from p in _context.Members
                                                where p.ID == id
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
                            Subject = Subject,
                            Content = "<p>" + emailContent + "</p><p>Please access the <strong>Niagara College</strong> web site to review.</p>"

                        };
                        await _emailSender.SendToManyAsync(msg);
                        ViewData["Message"] = "Message sent to " + folksCount + " Member"
                            + ((folksCount == 1) ? "." : "s.");
                    }
                    else
                    {
                        ViewData["Message"] = "Message NOT sent!  No Members in the system.";
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = ex.GetBaseException().Message;
                    ViewData["Message"] = "Error: Could not send email message to the " + folksCount + "Member"
                        + ((folksCount == 1) ? "" : "s") + " in the system.";
                }
            }
            return View();
        }

        public IActionResult DownloadReport()
        {
            var report = from e in _context.Members
                          .Include(m => m.AcademicDivision)
                          .Include(m => m.PAC)
                          .Include(m => m.Salutation)
                         orderby e.FirstName descending
                         select new
                         {
                             ID = e.ID,
							 Salutation = e.Salutation.SalutationTitle,
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
                             MemberIsArchived = e.IsArchived.ToString(),
                             NCGraduate = e.NCGraduate,
                             SignUpDate = e.SignUpDate.ToString("MM/dd/yyyy"),
                             ReNewDate_ = e.ReNewDate_.AddYears(3).ToString("MM/dd/yyyy"),
                             PreferredContact = (MemberVM.Contact)e.PreferredContact,
                             CompanyPositionTitle = e.CompanyPositionTitle,
                             CompanyPhoneNumber = e.CompanyPhoneNumber,
                             CompanyEmail = e.CompanyEmail,
                             CompanyStreetAddress = e.CompanyStreetAddress,
                             CompanyProvince = (MemberVM.Provinces)e.CompanyProvince,
                             CompanyCity = e.CompanyCity,
                             CompanyPostalCode = e.CompanyPostalCode,
                             AcademicDivision = e.AcademicDivision.DivisionName,
                             PAC = e.PAC.PACName
                         };




            int numRows = report.Count();

            if (numRows > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    var workSheet = excel.Workbook.Worksheets.Add("MemberReport");
                    workSheet.Cells[3, 1].LoadFromCollection(report, true);
                    foreach (ExcelRangeBase cell in workSheet.Cells[4, 13, numRows + 3, 13])
                    {
                        var text = cell.Text;
                        if (text == "True")
                        {
                            workSheet.Cells[cell.Start.Row, cell.Start.Column, cell.Start.Row, cell.Start.Column].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            workSheet.Cells[cell.Start.Row, cell.Start.Column, cell.Start.Row, cell.Start.Column].Style.Fill.BackgroundColor.SetColor(Color.Red);
                        }
                        else
                        {

                            workSheet.Cells[cell.Start.Row, cell.Start.Column, cell.Start.Row, cell.Start.Column].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            workSheet.Cells[cell.Start.Row, cell.Start.Column, cell.Start.Row, cell.Start.Column].Style.Fill.BackgroundColor.SetColor(Color.Green);
                        }
                    }
                    foreach (ExcelRangeBase cell in workSheet.Cells[4, 16, numRows + 3, 16])
                    {
                        DateTime date = DateTime.ParseExact(cell.Text.ToString(), "MM/dd/yyyy", null);

                        if (date > DateTime.ParseExact(DateTime.Today.ToString("MM/dd/yyyy"), "MM/dd/yyyy", null))
                        {
                            workSheet.Cells[cell.Start.Row, cell.Start.Column, cell.Start.Row, cell.Start.Column].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            workSheet.Cells[cell.Start.Row, cell.Start.Column, cell.Start.Row, cell.Start.Column].Style.Fill.BackgroundColor.SetColor(Color.Green);
                        }
                        else
                        {

                            workSheet.Cells[cell.Start.Row, cell.Start.Column, cell.Start.Row, cell.Start.Column].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            workSheet.Cells[cell.Start.Row, cell.Start.Column, cell.Start.Row, cell.Start.Column].Style.Fill.BackgroundColor.SetColor(Color.Red);
                        }
                    }
                    using (ExcelRange TotalMusicians = workSheet.Cells[numRows + 4, 2])
                    {
                        TotalMusicians.Style.Font.Bold = true;
                        workSheet.Cells[numRows + 4, 2].Value = "Total Members: " + numRows;
                    }
                    using (ExcelRange headings = workSheet.Cells[3, 1, 3, 26])
                    {
                        headings.Style.Font.Bold = true;
                        var fill = headings.Style.Fill;
                    }
                    workSheet.Cells.AutoFitColumns();
                    workSheet.Cells[1, 1].Value = "Member Report";
                    using (ExcelRange Rng = workSheet.Cells[1, 1, 1, 26])
                    {
                        Rng.Merge = true;
                        Rng.Style.Font.Bold = true;
                        Rng.Style.Font.Size = 18;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    DateTime utcDate = DateTime.UtcNow;
                    TimeZoneInfo esTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, esTimeZone);
                    using (ExcelRange Rng = workSheet.Cells[2, 1])
                    {
                        Rng.Value = "Created: " + localDate.ToShortTimeString() + " on " +
                            localDate.ToShortDateString();
                        Rng.Style.Font.Bold = true;
                        Rng.Style.Font.Size = 12;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    try
                    {
                        Byte[] theData = excel.GetAsByteArray();
                        string filename = "MemberReport.xlsx";
                        string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        return File(theData, mimeType, filename);
                    }
                    catch (Exception)
                    {
                        return BadRequest("Could not build and download the file.");
                    }
                }
            }
            return NotFound("No data.");
        }


        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.ID == id);
        }
    }
}