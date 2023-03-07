using NiagaraCollegeProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using NiagaraCollegeProject.Data;
using static System.Net.Mime.MediaTypeNames;
using System;
using NiagaraCollegeProject.Utilities;
using System.Collections.ObjectModel;

namespace MedicalOffice.Data
{

    public static class MOInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            PAC_Context context = applicationBuilder.ApplicationServices.CreateScope()
                .ServiceProvider.GetRequiredService<PAC_Context>();
            try
            {
                context.Database.Migrate(); //This is the same as Update-Database

                //To randomly generate data
                Random random = new Random();


                //Look for any Doctors.  Since we can't have patients without Doctors.
                if (!context.AcademicDivisions.Any())
                {
                    context.AcademicDivisions.AddRange(
                    new AcademicDivision
                    {
                        DivisionName = "Media"
                    },
                    new AcademicDivision
                    {
                        DivisionName = "Technology"
                    },
                    new AcademicDivision
                    {
                        DivisionName = "Trades"
                    });
                    context.SaveChanges();
                }

                if (!context.PAC.Any())
                {
                    context.PAC.AddRange(
                    new PAC
                    {
                        PACName = "Graphic Design",
                        AcademicDivisionID = 1
                    },
                    new PAC
                    {
                        PACName = "Acting",
                        AcademicDivisionID = 1
                    },
                    new PAC
                    {
                        PACName = "Game Devlopment",
                        AcademicDivisionID = 1
                    },
                    new PAC
                    {
                        PACName = "Public Relations",
                        AcademicDivisionID = 1,
                    },
                    new PAC
                    {
                        PACName = "Computer Programming",
                        AcademicDivisionID = 1
                    },
                    new PAC
                    {
                        PACName = "Broadcasting, Radio, Television and Filming",
                        AcademicDivisionID = 1
                    },
                    new PAC
                    {
                        PACName = "Social Media Management",
                        AcademicDivisionID = 1
                    },
                    new PAC
                    {
                        PACName = "Photographer",
                        AcademicDivisionID = 1
                    },
                    new PAC
                    {
                        PACName = "Mechanical Engineering",
                        AcademicDivisionID = 2
                    },
                    new PAC
                    {
                        PACName = "Renewable Energies",
                        AcademicDivisionID = 2
                    },
                    new PAC
                    {
                        PACName = "Computer, Electrical & Electronics Engineering",
                        AcademicDivisionID = 2
                    },
                    new PAC
                    {
                        PACName = "Computer Systems Technician",
                        AcademicDivisionID = 2
                    },
                    new PAC
                    {
                        PACName = "Construction & Civil Engineering",
                        AcademicDivisionID = 2
                    },
                    new PAC
                    {
                        PACName = "Industrial Automation",
                        AcademicDivisionID = 2
                    },
                    new PAC
                    {
                        PACName = "Photonics Engineering",
                        AcademicDivisionID = 2
                    },
                    new PAC
                    {
                        PACName = "Motive Power",
                        AcademicDivisionID = 3
                    },
                    new PAC
                    {
                        PACName = "Hairstyling",
                        AcademicDivisionID = 3
                    },
                    new PAC
                    {
                        PACName = "Welding Skills",
                        AcademicDivisionID = 3
                    },
                    new PAC
                    {
                        PACName = "Mechanical Techniques",
                        AcademicDivisionID = 3
                    },
                    new PAC
                    {
                        PACName = "Electrical Techniques",
                        AcademicDivisionID = 3
                    },
                    new PAC
                    {
                        PACName = "Carpentry and Renovation",
                        AcademicDivisionID = 3
                    });
                    context.SaveChanges();
                }
                if (!context.Salutations.Any())
                {
                    context.Salutations.AddRange(
                    new Salutation
                    {
                        SalutationTitle = "BLANK"
                    },
                    new Salutation
                    {
                        SalutationTitle = "Miss"
                    },
                    new Salutation
                    {
                        SalutationTitle = "Dr."
                    },
                    new Salutation
                    {
                        SalutationTitle = "Mr."
                    },
                    new Salutation
                    {
                        SalutationTitle = "Mrs."
                    },
                    new Salutation
                    {
                        SalutationTitle = "Ms."
                    },
                    new Salutation
                    {
                        SalutationTitle = "Prof."
                    });
                    context.SaveChanges();
                }

                if (!context.Members.Any())
                {
                    context.Members.AddRange(
                         new Member
                         {
                             FirstName = "Dean",
                             LastName = "(Admin)",
                             Email = "admin1@outlook.com",
                             MemberStatus = true,
                             AcademicDivisionID = 1,
                             PACID = 1,
                             SalutationID = 4,
                             PhoneNumber = "2897654832",
                             StreetAddress = "20 King Street",
                             Province = Member.Provinces.Ontario,
                             City = "St.Catharines",
                             PostalCode = "L4T2R7",
                             IsArchived = false,
                             CompanyName = "Telus Business",
                             CompanyStreetAddress = "214 King St",
                             CompanyProvince = Member.Provinces.Ontario,
                             CompanyCity = "Toronto",
                             CompanyPostalCode = "G4R9F5",
                             CompanyPhoneNumber = "2894671498",
                             CompanyEmail = "TelusBussiness@outlook.com",
                             PreferredContact = Member.Contact.Work,
                             CompanyPositionTitle = "Mechanical Engineering"
                         },
                         new Member
                         {
                             FirstName = "Kait",
                             LastName = "Ulisse",
                             Email = "super1@outlook.com",
                             MemberStatus = true,
                             AcademicDivisionID = 3,
                             PACID = 16,
                             SalutationID = 5,
                             PhoneNumber = "2896344492",
                             StreetAddress = "1654 North York",
                             Province = Member.Provinces.Ontario,
                             City = "Toronto",
                             PostalCode = "R7T5A6",
                             IsArchived = false,
                             CompanyName = "Cogeco Inc",
                             CompanyStreetAddress = "4379 Mississauga Road",
                             CompanyProvince = Member.Provinces.Ontario,
                             CompanyCity = "Mississauga",
                             CompanyPostalCode = "R3N2J9",
                             CompanyPhoneNumber = "2897614947",
                             CompanyEmail = "cogecoinc@outlook.com",
                             PreferredContact = Member.Contact.Work,
                             CompanyPositionTitle = "Social Media Management",
                         },
                          new Member
                          {
                              FirstName = "John",
                              LastName = "White",
                              Email = "staff1@outlook.com",
                              MemberStatus = true,
                              AcademicDivisionID = 2,
                              PACID = 12,
                              PhoneNumber = "2897641834",
                              StreetAddress = "35 King Street",
                              Province = Member.Provinces.Ontario,
                              City = "St.Catharines",
                              PostalCode = "L2T3G8",
                              IsArchived = false,
                              CompanyName = "Telus Business",
                              CompanyStreetAddress = "214 King St",
                              CompanyProvince = Member.Provinces.Ontario,
                              CompanyCity = "Toronto",
                              CompanyPostalCode = "G4RG9F",
                              CompanyPhoneNumber = "2894671498",
                              CompanyEmail = "TelusBussiness@outlook.com",
                              PreferredContact = Member.Contact.Work,
                              CompanyPositionTitle = "IT help desk",
                              SalutationID = 4

                          },
                          new Member
                          {
                              FirstName = "Jamie",
                              LastName = "Black",
                              Email = "JamB@outlook.com",
                              MemberStatus = true,
                              AcademicDivisionID = 3,
                              PACID = 18,
                              PhoneNumber = "2894681597",
                              StreetAddress = "279 Glenadle Ave",
                              Province = Member.Provinces.Ontario,
                              City = "St.Catharines",
                              PostalCode = "L4H4R4",
                              IsArchived = false,
                              CompanyName = "Balanced plus",
                              CompanyStreetAddress = "56 Pleasant St",
                              CompanyProvince = Member.Provinces.Ontario,
                              CompanyCity = "Toronto",
                              CompanyPostalCode = "G2T6F8",
                              CompanyPhoneNumber = "289734913",
                              CompanyEmail = "balancedplus@outlook.com",
                              PreferredContact = Member.Contact.Work,
                              CompanyPositionTitle = "networking technician",
                              SalutationID = 4
                          },
                          new Member
                          {
                              FirstName = "Eleri",
                              LastName = "Rowland",
                              Email = "EleriRow@outlook.com",
                              MemberStatus = true,
                              AcademicDivisionID = 2,
                              PACID = 9,
                              PhoneNumber = "2894651324",
                              StreetAddress = "456 North York",
                              Province = Member.Provinces.Ontario,
                              City = "Toronto",
                              PostalCode = "G7O2A4",
                              CompanyName = "Packlet Labs",
                              IsArchived = false,
                              CompanyStreetAddress = "6733 Mississauga Road",
                              CompanyProvince = Member.Provinces.Ontario,
                              CompanyCity = "Mississauga",
                              CompanyPostalCode = "L5N6J5",
                              CompanyPhoneNumber = "2891234567",
                              CompanyEmail = "packletlab@outlook.com",
                              PreferredContact = Member.Contact.Work,
                              CompanyPositionTitle = "graphic designer",
                              SalutationID = 4
                          },
                          new Member
                          {
                              FirstName = "Amanda",
                              LastName = "House",
                              Email = "AmandaH@outlook.com",
                              MemberStatus = true,
                              AcademicDivisionID = 1,
                              PACID = 6,
                              PhoneNumber = "2897539513",
                              StreetAddress = "North Service Rd",
                              Province = Member.Provinces.Ontario,
                              City = "St.Catharines",
                              PostalCode = "L3D5T8",
                              IsArchived = false,
                              CompanyName = "LME Services",
                              CompanyStreetAddress = "1564 Impress Ave",
                              CompanyProvince = Member.Provinces.Ontario,
                              CompanyCity = "Hamilton",
                              CompanyPostalCode = "R8G9F8",
                              CompanyPhoneNumber = "2894325687",
                              CompanyEmail = "lmeservice@outlook.com",
                              PreferredContact = Member.Contact.Work,
                              CompanyPositionTitle = "web develper",
                              SalutationID = 5
                          },
                          new Member
                          {
                              FirstName = "Siena",
                              LastName = "Dean",
                              Email = "SienaD@outlook.com",
                              MemberStatus = true,
                              AcademicDivisionID = 3,
                              PACID = 20,
                              PhoneNumber = "2897539513",
                              StreetAddress = "15 Fem Ave",
                              Province = Member.Provinces.Ontario,
                              City = "Niagara Falls",
                              PostalCode = "L3R7H3",
                              IsArchived = false,
                              CompanyName = "Code Bright",
                              CompanyStreetAddress = "5962 Grand Pavilion Way",
                              CompanyProvince = Member.Provinces.Ontario,
                              CompanyCity = "Alexandria",
                              CompanyPostalCode = "R8G9F8",
                              CompanyPhoneNumber = "2894356185",
                              CompanyEmail = "codebright@outlook.com",
                              PreferredContact = Member.Contact.Work,
                              CompanyPositionTitle = "data analyst",
                              SalutationID = 5
                          },
                          new Member
                          {
                              FirstName = "Annika",
                              LastName = "Stein",
                              Email = "AnnikaS@outlook.com",
                              MemberStatus = true,
                              AcademicDivisionID = 1,
                              PACID = 7,
                              PhoneNumber = "2894359489",
                              StreetAddress = "27 Moseby St",
                              Province = Member.Provinces.Ontario,
                              City = "Niagara On The Lake",
                              PostalCode = "L6R7L3",
                              IsArchived = false,
                              CompanyName = "Quebecor",
                              CompanyStreetAddress = "1254 Niven Rd",
                              CompanyProvince = Member.Provinces.Ontario,
                              CompanyCity = "Niagara On The Lake",
                              CompanyPostalCode = "R4A4F8",
                              CompanyPhoneNumber = "2891354687",
                              CompanyEmail = "quebecor@outlook.com",
                              PreferredContact = Member.Contact.Work,
                              CompanyPositionTitle = "Photographer",
                              SalutationID = 5
                          },
                          new Member
                          {
                              FirstName = "Jessie",
                              LastName = "Dejesus",
                              Email = "JessieD@outlook.com",
                              MemberStatus = true,
                              AcademicDivisionID = 3,
                              PACID = 17,
                              PhoneNumber = "2899461835",
                              StreetAddress = "154 Fem Ave",
                              Province = Member.Provinces.Ontario,
                              City = "Niagara Falls",
                              PostalCode = "L2F7G3",
                              IsArchived = false,
                              CompanyName = "Code Bright",
                              CompanyStreetAddress = "5962 Grand Pavilion Way",
                              CompanyProvince = Member.Provinces.Ontario,
                              CompanyCity = "Alexandria",
                              CompanyPostalCode = "R8G9F8",
                              CompanyPhoneNumber = "289435618",
                              CompanyEmail = "codebright@outlook.com",
                              PreferredContact = Member.Contact.Work,
                              CompanyPositionTitle = "Mechanical Techniques",
                              SalutationID = 5
                          },
                          new Member
                          {
                              FirstName = "Stella",
                              LastName = "Love",
                              Email = "StellaL@outlook.com",
                              MemberStatus = true,
                              AcademicDivisionID = 3,
                              PACID = 21,
                              PhoneNumber = "2894378461",
                              StreetAddress = "465 Kent St",
                              Province = Member.Provinces.Ontario,
                              City = "London",
                              PostalCode = "F8G2U5",
                              IsArchived = false,
                              CompanyName = "Wells Fargo",
                              CompanyStreetAddress = "57 South St",
                              CompanyProvince = Member.Provinces.Ontario,
                              CompanyCity = "London",
                              CompanyPostalCode = "F2G4O5",
                              CompanyPhoneNumber = "2894391567",
                              CompanyEmail = "wellsfargo@outlook.com",
                              PreferredContact = Member.Contact.Work,
                              CompanyPositionTitle = "Importation Manager",
                              SalutationID = 5
                          },
                          new Member
                          {
                              FirstName = "Light",
                              LastName = "Yagami",
                              Email = "LightY@outlook.com",
                              MemberStatus = true,
                              AcademicDivisionID = 1,
                              PACID = 4,
                              PhoneNumber = "2891234567",
                              StreetAddress = "77 Cedar St",
                              Province = Member.Provinces.NovaScotia,
                              City = "Halifax",
                              PostalCode = "H7G7G7",
                              IsArchived = false,
                              CompanyName = "Borden Ladner Gervais",
                              CompanyStreetAddress = "520 3rd Avenue SW Suite 1900",
                              CompanyProvince = Member.Provinces.Alberta,
                              CompanyCity = "Calgary",
                              CompanyPostalCode = "T2P0R3",
                              CompanyPhoneNumber = "4032329500",
                              CompanyEmail = "BordenLaw@outlook.com",
                              PreferredContact = Member.Contact.Work,
                              CompanyPositionTitle = "Detective",
                              SalutationID = 5
                          },
                          new Member
                          {
                              FirstName = "Jeremy",
                              LastName = "Tapia",
                              Email = "JeremyT@outlook.com",
                              MemberStatus = true,
                              AcademicDivisionID = 2,
                              PACID = 14,
                              PhoneNumber = "2894681348",
                              StreetAddress = "27 Coach Dr",
                              Province = Member.Provinces.Ontario,
                              City = "Niagara on The Lake",
                              PostalCode = "L2G3R6",
                              IsArchived = false,
                              CompanyName = "OweBest Technologies",
                              CompanyStreetAddress = "1783 Manitoba Street,",
                              CompanyProvince = Member.Provinces.BritishColumbia,
                              CompanyCity = "Vancouver",
                              CompanyPostalCode = "F2D4K2",
                              CompanyPhoneNumber = "2899491927",
                              CompanyEmail = "owebesttech@outlook.com",
                              PreferredContact = Member.Contact.Work,
                              CompanyPositionTitle = "game development",
                              SalutationID = 4
                          });



                    context.SaveChanges();
                }
                if (!context.Meetings.Any())
                {

                    context.Meetings.AddRange(
                    new Meeting
                    {
                        MeetingTopicName = "Talk about new additions to CPA program",
                        MeetingStartTimeDate = DateTime.Now.AddDays(10),
                        MeetingNotes = "Refer to Meeting Documents",
                        MemberID = 1

                    },
                    new Meeting
                    {
                        MeetingTopicName = "Change game development courses",
                        MeetingStartTimeDate = DateTime.Now.AddDays(25),
                        MeetingNotes = "Refer to Meeting Documents",
                        MemberID = 1
                    },
                    new Meeting
                    {
                        MeetingTopicName = "Talk about running summer classes",
                        MeetingStartTimeDate = DateTime.Now.AddDays(50),
                        MeetingNotes = "Refer to Meeting Documents",
                        MemberID = 1
                    });
                    context.SaveChanges();

                }
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.GetBaseException().Message);
            }

        }
    }
}
