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
using NiagaraCollegeProject.Utilities;
using System.Diagnostics.Metrics;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using System.Drawing.Printing;
using PagedList;
using System.Globalization;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

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
        public async Task<IActionResult> Index(int? page, int? pageSizeID)
        {
            var members = await _context.Members
                .Include(m => m.Subscriptions)
                .Include(m => m.ActionItems)
                .Include(m => m.AcademicDivision)
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
                    MemberStatus = e.MemberStatus,
                    IsArchived = e.IsArchived,
                    NCGraduate = e.NCGraduate,
                    SignUpDate = e.SignUpDate,
                    ReNewDate_ = e.ReNewDate_,
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

            //int pageSize = 10;
            //var pagedData = await PaginatedList<MemberAdminVM>.CreateAsync(members.AsQueryable(), page ?? 1, pageSize);
            //int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "TeamMembers");
            //ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            //var pagedData = members.ToPagedList(page ?? 1, pageSize);

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
                .Include(d => d.MemberDocuments)
                .Select(m => new MemberAdminVM
                {
                    EducationSummary = m.EducationSummary,
                    OccupationalSummary = m.OccupationalSummary,
                    StreetAddress = m.StreetAddress,
                    Province = (MemberVM.Provinces)m.Province,
                    City = m.City,
                    PostalCode = m.PostalCode,
                    MemberStatus = m.MemberStatus,
                    NCGraduate = m.NCGraduate,
                    SignUpDate = m.SignUpDate,
                    ReNewDate_ = m.ReNewDate_,
                    RenewalDueBy = m.RenewalDueBy,
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
                    PAC = m.PAC,
                    SalutationID = m.SalutationID,
                    Salutation = m.Salutation,
                })
                .FirstOrDefaultAsync();

            PopulateAssignedRoleData(member);

            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName", member.PACID);
            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName", member.AcademicDivisionID);
            ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", member.SalutationID);

            return View(member);
        }

        // POST: Members/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstName,LastName,Gender,PhoneNumber," +
            "Email,EducationSummary,OccupationalSummary,StreetAddress,Province,City,PostalCode," +
            "MemberStatus,NCGraduate,SignUpDate,ReNewDate_,RenewalDueBy,PreferredContact,CompanyPositionTitle,CompanyName,CompanyStreetAddress,CompanyProvince,CompanyCity,CompanyPostalCode,CompanyPhoneNumber,CompanyName,CompanyEmail," +
            "AcademicDivisionID,PACID,SalutationID")] Member member, string[] selectedRoles)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(member);
                    await _context.SaveChangesAsync();

                    InsertIdentityUser(member.Email, selectedRoles);

                    //Send Email to new Employee - commented out till email configured
                    //await InviteUserToResetPassword(member, null);

                    return RedirectToAction(nameof(Index));
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
                MemberStatus = member.MemberStatus,
                NCGraduate = member.NCGraduate,
                SignUpDate = member.SignUpDate,
                ReNewDate_ = member.ReNewDate_,
                RenewalDueBy = member.RenewalDueBy,
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
                    MemberStatus = m.MemberStatus,
                    NCGraduate = m.NCGraduate,
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
        public async Task<IActionResult> Edit(int id, bool MemberStatus, string[] selectedRoles)
        {
            var memberToUpdate = await _context.Members
                .Include(m => m.Subscriptions)
                .Include(m => m.ActionItems)
                .Include(m => m.AcademicDivision).ThenInclude(m => m.PACs)
                .Include(m => m.Salutation)
                .Include(d => d.MemberDocuments)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (memberToUpdate == null)
            {
                return NotFound();
            }

            //Note the current Email and member status
            bool memberStatus = memberToUpdate.MemberStatus;
            string databaseEmail = memberToUpdate.Email;

            if (await TryUpdateModelAsync<Member>(memberToUpdate, "",
                e => e.FirstName, e => e.LastName, e => e.PhoneNumber, e => e.Email, e => e.EducationSummary,
                e => e.OccupationalSummary, e => e.StreetAddress, e => e.Province, e => e.City, e => e.PostalCode,
                e => e.MemberStatus, e => e.NCGraduate, e => e.PreferredContact, e => e.CompanyPositionTitle,
                e => e.CompanyCity, e => e.AcademicDivisionID, e => e.PACID,
                e => e.SalutationID, e => e.CompanyEmail, e => e.CompanyName, e => e.CompanyPostalCode, e => e.CompanyStreetAddress, e => e.CompanyProvince
                , e => e.CompanyPhoneNumber))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    //Save successful so go on to related changes

                    //Check for changes in the Active state
                    if (memberToUpdate.MemberStatus == false && memberStatus == true)
                    {
                        //Deactivating them so delete the IdentityUser
                        //This deletes the user's login from the security system
                        await DeleteIdentityUser(memberToUpdate.Email);

                    }
                    else if (memberToUpdate.MemberStatus == true && memberStatus == false)
                    {
                        //You reactivating the user, create them and
                        //give them the selected roles
                        InsertIdentityUser(memberToUpdate.Email, selectedRoles);
                    }
                    else if (memberToUpdate.MemberStatus == true && memberStatus == true)
                    {
                        //No change to Active status so check for a change in Email
                        //If you Changed the email, Delete the old login and create a new one
                        //with the selected roles
                        if (memberToUpdate.Email != databaseEmail)
                        {
                            //Add the new login with the selected roles
                            InsertIdentityUser(memberToUpdate.Email, selectedRoles);

                            //This deletes the user's old login from the security system
                            await DeleteIdentityUser(databaseEmail);
                        }
                        else
                        {
                            //Finially, Still Active and no change to Email so just Update
                            await UpdateUserRoles(selectedRoles, memberToUpdate.Email);
                        }
                    }

                    return RedirectToAction(nameof(Index));
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
                        ModelState.AddModelError("Email", "Unable to save changes. Remember, you cannot have duplicate Email addresses.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }
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
                MemberStatus = memberToUpdate.MemberStatus,
                NCGraduate = memberToUpdate.NCGraduate,
                SignUpDate = memberToUpdate.SignUpDate,
                RenewalDueBy = memberToUpdate.RenewalDueBy,
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
                memberAdminVM.UserRoles.Add(role);
            }
            PopulateAssignedRoleData(memberAdminVM);

            ViewData["PACID"] = new SelectList(_context.PAC, "ID", "PACName", memberAdminVM.PACID);
            ViewData["AcademicDivisionID"] = new SelectList(_context.AcademicDivisions, "ID", "DivisionName", memberAdminVM.AcademicDivisionID);
            ViewData["SalutationID"] = new SelectList(_context.Salutations, "ID", "SalutationTitle", memberAdminVM.SalutationID);

            return View(memberAdminVM);
        }

        public JsonResult GetPacList(int Id)
        {
            var result = _context.PAC
                .Where(r => r.AcademicDivisionID == Id).ToList();
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

        private void InsertIdentityUser(string Email, string[] selectedRoles)
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

        private async Task InviteUserToResetPassword(Member member, string message)
        {
            message ??= "Hello " + member.FirstName + "<br /><p>Please navigate to:<br />" +
                        "<a href='https://theapp.azurewebsites.net/' title='https://theapp.azurewebsites.net/' target='_blank' rel='noopener'>" +
                        "https://theapp.azurewebsites.net</a><br />" +
                        " and create a new password for " + member.Email + " using Forgot Password.</p>";
            try
            {
                await _emailSender.SendOneAsync(member.FullName, member.Email,
                "Account Registration", message);
                TempData["message"] = "Invitation email sent to " + member.FullName + " at " + member.Email;
            }
            catch (Exception)
            {
                TempData["message"] = "Could not send Invitation email to " + member.FullName + " at " + member.Email;
            }


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
                return Problem("Entity set 'PAC_Context.Members'  is null.");
            }
            var member = await _context.Members.FindAsync(id);

            if (member != null)
            {
                member.MemberStatus = false;
                member.IsArchived = true;

                _context.Entry(member).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _context.Entry(member).Reload();
            }

            //var members = membe

            return RedirectToAction(nameof(Index));
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

            ExcelPackage excel;
            using (var memoryStream = new MemoryStream())
            {
                await theExcel.CopyToAsync(memoryStream);
                excel = new ExcelPackage(memoryStream);
            }
            var workSheet = excel.Workbook.Worksheets[3];
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
                    _context.SaveChanges();

                    foreach (Member memobj in MemberAccounts)
                    {
                        InsertIdentityUser(memobj.Email, selectedRoles);
                    }


                    //Send Email to new Employee - commented out till email configured
                    //await InviteUserToResetPassword(member, null);

                    return RedirectToAction("Index", "TeamMembers");
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
            //_context.Members.AddRange(MemberAccounts);
            //_context.SaveChanges();
            return RedirectToAction("Index", "TeamMembers");
        }

        // GET/POST: MedicalTrials/Notification/5
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
            //do not delete
            //var report = _context.Members
            //      .Include(m=>m.AcademicDivision)
            //      .Include(m => m.PAC)
            //      .Include(m=>m.Salutation)
            //      .GroupBy(m => new { m.ID, m.FirstName, m.LastName, m.PhoneNumber, m.CompanyPhoneNumber, m.EducationSummary, m.OccupationalSummary, m.StreetAddress
            //      ,m.Province, m.City, m.PostalCode, m.MemberStatus, m.NCGraduate, m.SignUpDate, m.RenewalDueBy, m.CompanyName, m.CompanyStreetAddress, m.CompanyProvince, m.CompanyCity
            //      ,m.CompanyPostalCode,m.CompanyEmail,m.PreferredContact,m.CompanyPositionTitle,m.AcademicDivision,m.PAC,m.Salutation,m.Email})
            //      .Select(grp => new MemberReportsSummaryVM
            //      {
            //          ID = grp.Key.ID,
            //          FirstName = grp.Key.FirstName,
            //          LastName = grp.Key.LastName,
            //          PhoneNumber = grp.Key.PhoneNumber,                    
            //          Email = grp.Key.Email,
            //          EducationSummary = grp.Key.EducationSummary,
            //          OccupationalSummary = grp.Key.OccupationalSummary,
            //          StreetAddress = grp.Key.StreetAddress,
            //          Province = (MemberReportsSummaryVM.Provinces)grp.Key.Province,
            //          City = grp.Key.City,
            //          PostalCode = grp.Key.PostalCode,
            //          MemberStatus = grp.Key.MemberStatus.ToString(),
            //          NCGraduate = grp.Key.NCGraduate,
            //          SignUpDate = grp.Key.SignUpDate,
            //          RenewalDueBy = grp.Key.RenewalDueBy,
            //          PreferredContact = (MemberReportsSummaryVM.Contact)grp.Key.PreferredContact,
            //          CompanyPositionTitle = grp.Key.CompanyPositionTitle,
            //          CompanyPhoneNumber = grp.Key.CompanyPhoneNumber,
            //          CompanyEmail = grp.Key.CompanyEmail,
            //          CompanyStreetAddress = grp.Key.CompanyStreetAddress,
            //          CompanyProvince = (MemberReportsSummaryVM.Provinces)grp.Key.CompanyProvince,
            //          CompanyCity = grp.Key.CompanyCity,
            //          CompanyPostalCode = grp.Key.CompanyPostalCode,
            //          AcademicDivision = grp.Key.AcademicDivision.DivisionName,
            //          PAC = grp.Key.PAC.PACName,
            //          Salutation = grp.Key.Salutation.SalutationTitle
            //      }).OrderBy(m => m.LastName).ThenBy(m => m.FirstName);

            var report = from e in _context.Members
                          .Include(m => m.AcademicDivision)
                          .Include(m => m.PAC)
                          .Include(m => m.Salutation)
                         orderby e.FirstName descending
                         select new
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
                             MemberStatus = e.MemberStatus.ToString(),
                             NCGraduate = e.NCGraduate,
                             SignUpDate = e.SignUpDate.ToString("MM/dd/yyyy"),
                             RenewalDueBy = e.RenewalDueBy.ToString("MM/dd/yyyy"),
                             PreferredContact = (MemberVM.Contact)e.PreferredContact,
                             CompanyPositionTitle = e.CompanyPositionTitle,
                             CompanyPhoneNumber = e.CompanyPhoneNumber,
                             CompanyEmail = e.CompanyEmail,
                             CompanyStreetAddress = e.CompanyStreetAddress,
                             CompanyProvince = (MemberVM.Provinces)e.CompanyProvince,
                             CompanyCity = e.CompanyCity,
                             CompanyPostalCode = e.CompanyPostalCode,
                             AcademicDivision = e.AcademicDivision.DivisionName,
                             PAC = e.PAC.PACName,
                             Salutation = e.Salutation.SalutationTitle
                         };



                  
            int numRows = report.Count();

            if (numRows > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    var workSheet = excel.Workbook.Worksheets.Add("MemberReport");
                    workSheet.Cells[3, 1].LoadFromCollection(report, true);                   
                    foreach (ExcelRangeBase cell in workSheet.Cells[4,12,numRows+3,12])
                    {
                        var text = cell.Text;
                        if (text == "True")
                        {
                            workSheet.Cells[cell.Start.Row, cell.Start.Column,cell.Start.Row,cell.Start.Column].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            workSheet.Cells[cell.Start.Row, cell.Start.Column, cell.Start.Row, cell.Start.Column].Style.Fill.BackgroundColor.SetColor(Color.Green);
                        }
                        else
                        {

                            workSheet.Cells[cell.Start.Row, cell.Start.Column, cell.Start.Row, cell.Start.Column].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            workSheet.Cells[cell.Start.Row, cell.Start.Column, cell.Start.Row, cell.Start.Column].Style.Fill.BackgroundColor.SetColor(Color.Red);
                        }
                    }
                    foreach (ExcelRangeBase cell in workSheet.Cells[4, 15, numRows + 3, 15])
                    {
                        DateTime date = DateTime.ParseExact(cell.Text.ToString(), "MM/dd/yyyy",null);

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
