using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace NiagaraCollegeProject.Data
{
    public static class ApplicationDbInitializer
    {
        public static async void Seed(IApplicationBuilder applicationBuilder)
        {
            ApplicationDbContext context = applicationBuilder.ApplicationServices.CreateScope()
                .ServiceProvider.GetRequiredService<ApplicationDbContext>();
            try
            {
                ////Delete the database if you need to apply a new Migration
                //context.Database.EnsureDeleted();
                //Create the database if it does not exist and apply the Migration
                context.Database.Migrate();

                //Create Roles
                var RoleManager = applicationBuilder.ApplicationServices.CreateScope()
                    .ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                string[] roleNames = { "Admin", "Supervisor", "Staff" };
                IdentityResult roleResult;
                foreach (var roleName in roleNames)
                {
                    var roleExist = await RoleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }
                //Create Users
                var userManager = applicationBuilder.ApplicationServices.CreateScope()
                    .ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                if (userManager.FindByEmailAsync("admin1@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "admin1@outlook.com",
                        Email = "admin1@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "NCPROG1440").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Admin").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("super1@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "super1@outlook.com",
                        Email = "super1@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "NCPROG1440").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Supervisor").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("staff1@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "staff1@outlook.com",
                        Email = "staff1@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "NCPROG1440").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Staff").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("JamB@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "JamB@outlook.com",
                        Email = "JamB@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "NCPROG1440").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Staff").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("EleriRow@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "EleriRow@outlook.com",
                        Email = "EleriRow@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "NCPROG1440").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Staff").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("AmandaH@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "AmandaH@outlook.com",
                        Email = "AmandaH@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "NCPROG1440").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Staff").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("SienaD@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "SienaD@outlook.com",
                        Email = "SienaD@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "NCPROG1440").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Staff").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("AnnikaS@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "AnnikaS@outlook.com",
                        Email = "AnnikaS@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "NCPROG1440").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Staff").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("JessieD@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "JessieD@outlook.com",
                        Email = "JessieD@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "NCPROG1440").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Staff").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("StellaL@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "StellaL@outlook.com",
                        Email = "StellaL@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "NCPROG1440").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Staff").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("LightY@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "LightY@outlook.com",
                        Email = "LightY@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "NCPROG1440").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Supervisor").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("JeremyT@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "JeremyT@outlook.com",
                        Email = "JeremyT@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "NCPROG1440").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Staff").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("user1@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "user1@outlook.com",
                        Email = "user1@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "password").Result;
                    //Not in any role
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.GetBaseException().Message);
            }
        }
    }

}
