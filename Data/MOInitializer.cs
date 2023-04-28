using NiagaraCollegeProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using NiagaraCollegeProject.Data;
using static System.Net.Mime.MediaTypeNames;
using System;
using NiagaraCollegeProject.Utilities;
using System.Collections.ObjectModel;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace PacTeam.Data
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
                        PACName = "Game Development",
                        AcademicDivisionID = 1
                    },
                    new PAC
                    {
                        PACName = "Public Relations",
                        AcademicDivisionID = 1,
                    },
                    new PAC
                    {
                        PACName = "Computer Programming and Analysis",
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
                        PACName = "Photography",
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
                if (!context.Poll.Any())
                {
                    context.Poll.AddRange(
                    new Poll
                    {
                        Question = "Spring 2023 Meeting Scheulde",
                        isActive = true,
                        ExpiryDate = DateTime.Today.AddDays(20),
                        PacID = 1
                    },
                    new Poll
                    {
                        Question = "Fall 2024 Meeting Scheulde",
                        isActive = true,
                        ExpiryDate = DateTime.Today.AddDays(40),
                        PacID = 2

                    }, new Poll
                    {
                        Question = "Winter 2025 Meeting Scheulde",
                        isActive = true,
                        ExpiryDate = DateTime.Today.AddDays(30),
                        PacID = 3

                    }, new Poll
                    {
                        Question = "Sed eget quam at ante cursus",
                        isActive = true,
                        ExpiryDate = DateTime.Today.AddDays(46),
                        PacID = 4
                    }, new Poll
                    {
                        Question = "Morbi libero neque, placerat",
                        isActive = true,
                        ExpiryDate = DateTime.Today.AddDays(10),
                        PacID = 5
                    }, new Poll
                    {
                        Question = "gravida maximus at eget ligula",
                        isActive = false,
                        ExpiryDate = DateTime.Today.AddDays(15),
                        PacID = 6
                    }, new Poll
                    {
                        Question = "Nullam sed tristique nulla",
                        isActive = false,
                        ExpiryDate = DateTime.Today.AddDays(55),
                        PacID = 7
                    }, new Poll
                    {
                        Question = "molestie porttitor nec",
                        isActive = true,
                        ExpiryDate = DateTime.Today.AddDays(15),
                        PacID = 8
                    }, new Poll
                    {
                        Question = "fringilla eu tortor quis, eleifend",
                        isActive = false,
                        ExpiryDate = DateTime.Today.AddDays(125),
                        PacID = 9
                    }, new Poll
                    {
                        Question = "Cras gravida felis mattis nulla",
                        isActive = true,
                        ExpiryDate = DateTime.Today.AddDays(45),
                        PacID = 10
                    });
                    context.SaveChanges();
                }
                if (!context.PollOption.Any())
                {
                    context.PollOption.AddRange(
                    new PollOption
                    {
                        OptionText = "Orci varius natoque penatibus et magnis",
                        Votes = 0,
                        PollID = 1
                    },
                    new PollOption
                    {
                        OptionText = "Etiam vitae eros augue",
                        Votes = 0,
                        PollID = 1
                    },
                    new PollOption
                    {
                        OptionText = "orci vel aliquam blandit",
                        Votes = 0,
                        PollID = 1
                    }, new PollOption
                    {
                        OptionText = "libero. Donec id sagittis nisl, non suscipit",
                        Votes = 0,
                        PollID = 1
                    }, new PollOption
                    {
                        OptionText = "gravida sem, a iaculis mauris",
                        Votes = 0,
                        PollID = 2
                    }, new PollOption
                    {
                        OptionText = "Morbi quis felis at odio",
                        Votes = 0,
                        PollID = 2
                    }, new PollOption
                    {
                        OptionText = "velit congue congue. In id est nisi.",
                        Votes = 0,
                        PollID = 2
                    }, new PollOption
                    {
                        OptionText = "felis blandit. Aenean",
                        Votes = 0,
                        PollID = 3
                    }, new PollOption
                    {
                        OptionText = "Maecenas faucibus torto",
                        Votes = 0,
                        PollID = 4
                    }, new PollOption
                    {
                        OptionText = " justo mi gravida sem, a iaculis mauris",
                        Votes = 0,
                        PollID = 4
                    }, new PollOption
                    {
                        OptionText = "semper. Praesent",
                        Votes = 0,
                        PollID = 5
                    }, new PollOption
                    {
                        OptionText = "dignissim quis velit. ",
                        Votes = 0,
                        PollID = 5
                    }, new PollOption
                    {
                        OptionText = "Fusce eu metus consequat, sodales",
                        Votes = 0,
                        PollID = 5
                    });
                    context.SaveChanges();
                }

                if (!context.Members.Any())
                {
                    context.Members.AddRange(
                         new Member
                         {
                             FirstName = "Kait",
                             LastName = "Ulisse",
                             Email = "admin1@outlook.com",
                             AcademicDivisionID = 1,
                             PACID = 1,
                             SignUpDate = Convert.ToDateTime(new DateTime(2010, 04, 15).ToString("yyyy-MM-dd")),
                             ReNewDate_ = Convert.ToDateTime(new DateTime(2022, 02, 19).ToString("yyyy-MM-dd")),
                             SalutationID = 2,
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
                             FirstName = "Max",
                             LastName = "Black",
                             Email = "super1@outlook.com",
                             AcademicDivisionID = 3,
                             PACID = 16,
                             SignUpDate = Convert.ToDateTime(new DateTime(2017, 05, 09).ToString("yyyy-MM-dd")),
                             ReNewDate_ = Convert.ToDateTime(new DateTime(2023, 01, 18).ToString("yyyy-MM-dd")),
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
                              AcademicDivisionID = 2,
                              PACID = 12,
                              SignUpDate = Convert.ToDateTime(new DateTime(2008, 01, 1).ToString("yyyy-MM-dd")),
                              ReNewDate_ = Convert.ToDateTime(new DateTime(2021, 05, 27).ToString("yyyy-MM-dd")),
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
                              CompanyPostalCode = "L2H3E6",
                              CompanyPhoneNumber = "2896758833",
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
                              AcademicDivisionID = 3,
                              PACID = 18,
                              SignUpDate = Convert.ToDateTime(new DateTime(2016, 09, 21).ToString("yyyy-MM-dd")),
                              ReNewDate_ = Convert.ToDateTime(new DateTime(2021, 04, 05).ToString("yyyy-MM-dd")),
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
                              CompanyPhoneNumber = "2897343913",
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
                              AcademicDivisionID = 2,
                              PACID = 9,
                              SignUpDate = Convert.ToDateTime(new DateTime(2019, 07, 27).ToString("yyyy-MM-dd")),
                              ReNewDate_ = Convert.ToDateTime(new DateTime(2020, 11, 19).ToString("yyyy-MM-dd")),
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
                              AcademicDivisionID = 1,
                              PACID = 6,
                              SignUpDate = Convert.ToDateTime(new DateTime(2018, 09, 24).ToString("yyyy-MM-dd")),
                              ReNewDate_ = Convert.ToDateTime(new DateTime(2020, 12, 25).ToString("yyyy-MM-dd")),
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
                              AcademicDivisionID = 3,
                              PACID = 20,
                              SignUpDate = Convert.ToDateTime(new DateTime(2022, 08, 16).ToString("yyyy-MM-dd")),
                              ReNewDate_ = Convert.ToDateTime(new DateTime(2022, 08, 18).ToString("yyyy-MM-dd")),
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
                              AcademicDivisionID = 1,
                              PACID = 7,
                              SignUpDate = Convert.ToDateTime(new DateTime(2011, 10, 20).ToString("yyyy-MM-dd")),
                              ReNewDate_ = Convert.ToDateTime(new DateTime(2020, 06, 07).ToString("yyyy-MM-dd")),
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
                              AcademicDivisionID = 3,
                              PACID = 17,
                              SignUpDate = Convert.ToDateTime(new DateTime(2009, 09, 19).ToString("yyyy-MM-dd")),
                              ReNewDate_ = Convert.ToDateTime(new DateTime(2022, 09, 23).ToString("yyyy-MM-dd")),
                              PhoneNumber = "2899461835",
                              StreetAddress = "154 Fem Ave",
                              Province = Member.Provinces.Ontario,
                              City = "Niagara Falls",
                              PostalCode = "L2F7G3",
                              IsArchived = true,
                              CompanyName = "Code Bright",
                              CompanyStreetAddress = "5962 Grand Pavilion Way",
                              CompanyProvince = Member.Provinces.Ontario,
                              CompanyCity = "Alexandria",
                              CompanyPostalCode = "R8G9F8",
                              CompanyPhoneNumber = "2894356418",
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
                              AcademicDivisionID = 3,
                              PACID = 21,
                              SignUpDate = Convert.ToDateTime(new DateTime(2012, 04, 11).ToString("yyyy-MM-dd")),
                              ReNewDate_ = Convert.ToDateTime(new DateTime(2022, 03, 12).ToString("yyyy-MM-dd")),
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
                              AcademicDivisionID = 1,
                              PACID = 4,
                              SignUpDate = Convert.ToDateTime(new DateTime(2007, 06, 01).ToString("yyyy-MM-dd")),
                              ReNewDate_ = Convert.ToDateTime(new DateTime(2020, 08, 07).ToString("yyyy-MM-dd")),
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
                              AcademicDivisionID = 2,
                              PACID = 14,
                              SignUpDate = Convert.ToDateTime(new DateTime(2011, 11, 15).ToString("yyyy-MM-dd")),
                              ReNewDate_ = Convert.ToDateTime(new DateTime(2021, 11, 29).ToString("yyyy-MM-dd")),
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
                        MeetingStartTimeDate = DateTime.Today.AddDays(10),
                        MeetingNotes = "Refer to Meeting Documents",
                        MemberID = 1,
                        AcademicDivisionID = 1,
                        PACID = 2,
                        Attendees = new List<MeetingAttendees>
                        {
                            new MeetingAttendees
                            {
                                MemberID = 1,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 3,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 2,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 5,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 7,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 4,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 9,
                                MeetingID = 3
                            }
                        }
                    },
                    new Meeting
                    {
                        MeetingTopicName = "Change game development courses",
                        MeetingStartTimeDate = DateTime.Today.AddDays(25),
                        MeetingNotes = "Refer to Meeting Documents",
                        MemberID = 1,
                        AcademicDivisionID = 2,
                        PACID = 9,
                        Attendees = new List<MeetingAttendees>
                        {
                            new MeetingAttendees
                            {
                                MemberID = 6,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 3,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 2,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 5,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 7,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 1,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 4,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 9,
                                MeetingID = 3
                            }
                        }
                    },
                    new Meeting
                    {
                        MeetingTopicName = "Talk about running summer classes",
                        MeetingStartTimeDate = DateTime.Today.AddDays(50),
                        MeetingNotes = "Refer to Meeting Documents",
                        MemberID = 1,
                        AcademicDivisionID = 3,
                        PACID = 17,
                        Attendees = new List<MeetingAttendees>
                        { 
                            new MeetingAttendees
                            {
                                MemberID = 4,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 3,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 2,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 5,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 7,
                                MeetingID = 3
                            }
                        }
                    },
                    new Meeting
                    {
                        MeetingTopicName = "Spring 2024",
                        MeetingStartTimeDate = DateTime.Today.AddDays(90),
                        MeetingNotes = "Refer to Meeting Documents",
                        MemberID = 1,
                        AcademicDivisionID = 3,
                        PACID = 17,
                        Attendees = new List<MeetingAttendees>
                        {
                            new MeetingAttendees
                            {
                                MemberID = 8,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 2,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 5,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 7,
                                MeetingID = 3
                            }
                        }
                    },
                    new Meeting
                    {
                        MeetingTopicName = "Fall 2023",
                        MeetingStartTimeDate = DateTime.Today.AddDays(50),
                        MeetingNotes = "Refer to Meeting Documents",
                        MemberID = 1,
                        AcademicDivisionID = 3,
                        PACID = 17,
                        Attendees = new List<MeetingAttendees>
                        {
                            new MeetingAttendees
                            {
                                MemberID = 9,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 3,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 2,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 5,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 7,
                                MeetingID = 3
                            }
                        }
                    },
                    new Meeting
                    {
                        MeetingTopicName = "Winter 2024",
                        MeetingStartTimeDate = DateTime.Today.AddDays(120),
                        MeetingNotes = "Refer to Meeting Documents",
                        MemberID = 1,
                        AcademicDivisionID = 3,
                        PACID = 17,
                        Attendees = new List<MeetingAttendees>
                        {
                            new MeetingAttendees
                            {
                                MemberID = 1,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 3,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 2,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 5,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 7,
                                MeetingID = 3
                            }
                        }
                    },
                    new Meeting
                    {
                        MeetingTopicName = "Advise on lab upgrades",
                        MeetingStartTimeDate = DateTime.Today.AddDays(70),
                        MeetingNotes = "Refer to Meeting Documents",
                        MemberID = 1,
                        AcademicDivisionID = 3,
                        PACID = 17,
                        Attendees = new List<MeetingAttendees>
                        {
                            new MeetingAttendees
                            {
                                MemberID = 4,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 2,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 5,
                                MeetingID = 3
                            },
                            new MeetingAttendees
                            {
                                MemberID = 7,
                                MeetingID = 3
                            }
                        }
                    });
                    context.SaveChanges();

                    if (!context.ActionItems.Any())
                    {
                        context.ActionItems.AddRange(
                             new ActionItem
                             {
                                 AgendaName = "Advise",
                                 Description = "Current curriculum not offering enough chance for future career",
                                 Comments = "Focus more on practical knowledge",
                                 TaskDueDate = DateTime.Today.AddDays(50),
                                 Completed = false,
                                 MeetingID= 1,
                                 MemberID= 2
                             }, new ActionItem
                             {
                                 AgendaName = "Printing Format",
                                 Description = "Print and format upcoming NC documents",
                                 Comments = "Adjust document formats",
                                 TaskDueDate = DateTime.Today.AddDays(90),
                                 Completed = true,
                                 MeetingID = 2,
                                 MemberID = 3
                             }, new ActionItem
                             {
                                 AgendaName = "Research new lab computers",
                                 Description = "",
                                 Comments = "find pricing",
                                 TaskDueDate = DateTime.Today.AddDays(120),
                                 Completed = true,
                                 MeetingID = 5,
                                 MemberID = 3
                             }, new ActionItem
                             {
                                 AgendaName = "Virtual meeting with client",
                                 Description = "Set up appropriate timezone for next 3 client meetings",
                                 Comments = "Prefer UTC-4 timezone",
                                 TaskDueDate = DateTime.Today.AddDays(30),
                                 Completed = true,
                                 MeetingID = 3,
                                 MemberID = 3
                             }, new ActionItem
                             {
                                 AgendaName = "Research on new product",
                                 Description = "Find and edit product #15378 mentioned in previous meeting",
                                 Comments = "Need the task completed ASAP",
                                 TaskDueDate = DateTime.Today.AddDays(20),
                                 Completed = false,
                                 MeetingID = 6,
                                 MemberID = 4
                             }, new ActionItem
                             {
                                 AgendaName = "Hire new graphic desingers for project #A02",
                                 Description = "Find private company",
                                 Comments = "No more than 2 people",
                                 TaskDueDate = DateTime.Today.AddDays(10),
                                 Completed = false,
                                 MeetingID = 2,
                                 MemberID = 6
                             }, new ActionItem
                             {
                                 AgendaName = "Resources defer",
                                 Description = "Limit the items from inventory",
                                 Comments = "Change some of the items' details too",
                                 TaskDueDate = DateTime.Today.AddDays(35),
                                 Completed = true,
                                 MeetingID = 2,
                                 MemberID = 6
                             }, new ActionItem
                             {
                                 AgendaName = "Focus on promoting business for Winter 2024",
                                 Description = "Marketing plans",
                                 Comments = "Only on advertising",
                                 TaskDueDate = DateTime.Today.AddDays(46),
                                 Completed = true,
                                 MeetingID = 2,
                                 MemberID = 7
                             }, new ActionItem
                             {
                                 AgendaName = "Gather information on business changes",
                                 Description = "Suggesting solutions",
                                 Comments = "Prioritized task",
                                 TaskDueDate = DateTime.Today.AddDays(90),
                                 Completed = true,
                                 MeetingID = 2,
                                 MemberID = 3
                             }, new ActionItem
                             {
                                 AgendaName = "Adjust meeting minutes",
                                 Description = "For next meeting",
                                 Comments = "Need more meeting minutes",
                                 TaskDueDate = DateTime.Today.AddDays(90),
                                 Completed = false,
                                 MeetingID = 2,
                                 MemberID = 2
                             }, new ActionItem
                             {
                                 AgendaName = "Staying on client #R03 contract",
                                 Description = "Expand client's requirements",
                                 Comments = "Audit needed",
                                 TaskDueDate = DateTime.Today.AddDays(90),
                                 Completed = true,
                                 MeetingID = 2,
                                 MemberID = 3
                             }, new ActionItem
                             {
                                 AgendaName = "Transfer provided documents to human resources",
                                 Description = "Collect documents back from them",
                                 Comments = "Email their supervisor for more details",
                                 TaskDueDate = DateTime.Today.AddDays(20),
                                 Completed = false,
                                 MeetingID = 5,
                                 MemberID = 8
                             }) ;
                        context.SaveChanges();
                    }
                    if (!context.MemberActionItems.Any())
                    {
                        context.MemberActionItems.AddRange(
                             new MemberActionItems
                             {
                                 ActionItemID = 1,
                                 MemberID = 2
                             }, new MemberActionItems
                             {
                                 ActionItemID = 2,
                                 MemberID = 3
                             }, new MemberActionItems
                             {
                                 ActionItemID = 3,
                                 MemberID = 3
                             }, new MemberActionItems
                             {
                                 ActionItemID = 4,
                                 MemberID = 3
                             }, new MemberActionItems
                             {
                                 ActionItemID = 5,
                                 MemberID = 4
                             }, new MemberActionItems
                             {
                                 ActionItemID = 6,
                                 MemberID = 6
                             }, new MemberActionItems
                             {
                                 ActionItemID = 7,
                                 MemberID = 6
                             }, new MemberActionItems
                             {
                                 ActionItemID = 7,
                                 MemberID = 7
                             }, new MemberActionItems
                             {
                                 ActionItemID = 8,
                                 MemberID = 3
                             }, new MemberActionItems
                             {
                                 ActionItemID = 9,
                                 MemberID = 2
                             }, new MemberActionItems
                             {
                                 ActionItemID = 10,
                                 MemberID = 3
                             }, new MemberActionItems
                             {
                                 ActionItemID = 11,
                                 MemberID = 8
                             });
                        context.SaveChanges();
                    }
                }               
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.GetBaseException().Message);
            }

        }
    }
}
