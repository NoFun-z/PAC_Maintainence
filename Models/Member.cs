using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Xml.Linq;

namespace NiagaraCollegeProject.Models
{
    [ModelMetadataType(typeof(MemberMetaData))]
    public class Member : Auditable//, IValidatableObject
    {
        public int ID { get; set; }

        public string FullName
        {
            get
            {
                return FirstName
                    + " " + LastName;
            }
        }

        public string PersonalPhoneFormatted
        {
            get
            {
                if (PhoneNumber == null) return "";
                else return "(" + PhoneNumber.Substring(0, 3) + ") " + PhoneNumber.Substring(3, 3) + "-" + PhoneNumber[6..];
            }
        }

        public string WorkPhoneFormatted
        {
            get
            {
                if (CompanyPhoneNumber == null) return "";
                else return "(" + CompanyPhoneNumber.Substring(0, 3) + ") " + CompanyPhoneNumber.Substring(3, 3) + "-" + CompanyPhoneNumber[6..];
            }
        }
        public string MemberFormattedAddress
        {
            get
            {
                return StreetAddress + ", " + City + ", " + Province + ", " + PostalCode;
            }
        }

        public string CompanyFormattedAddress
        {
            get
            {
                return CompanyStreetAddress + ", " + CompanyCity + ", " + CompanyProvince + ", " + CompanyPostalCode;
            }
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid phone number in the format 1231231234")]
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public string EducationSummary { get; set; }


        public string OccupationalSummary { get; set; }

        public string StreetAddress { get; set; }

        public enum Provinces
        {
            [Display(Name = "Alberta")]
            Alberta,
            [Display(Name = "British Columbia")]
            BritishColumbia,
            [Display(Name = "Manitoba")]
            Manitoba,
            [Display(Name = "New Brunswick")]
            NewBrunswick,
            [Display(Name = "Newfoundland and Labrador")]
            NewfoundlandandLabrador,
            [Display(Name = "Northwest Territories")]
            NorthwestTerritories,
            [Display(Name = "Nova Scotia")]
            NovaScotia,
            [Display(Name = "Nunavut")]
            Nunavut,
            [Display(Name = "Ontario")]
            Ontario,
            [Display(Name = "Prince Edward Island")]
            PrinceEdwardIsland,
            [Display(Name = "Quebec")]
            Quebec,
            [Display(Name = "Saskatchewan")]
            Saskatchewan,
            [Display(Name = "Yukon")]
            Yukon
        }

        public Provinces Province { get; set; }

        public string City { get; set; }
        [RegularExpression(@"[A-Za-z][0-9][A-Za-z][0-9][A-Za-z][0-9]", ErrorMessage = "Please enter a valid postal code in the format L2H3E9")]
        public string PostalCode { get; set; }

        public bool NCGraduate { get; set; }

        private DateTime signUpDate { get; set; }

        public DateTime SignUpDate
        {
            get { return signUpDate; }
            set
            {
                if (value == DateTime.MinValue)
                {
                    signUpDate = DateTime.Today;
                }
                else
                {
                    signUpDate = value;
                }
            }
        }

        private DateTime ReNewDate { get; set; }

        public DateTime ReNewDate_
        {
            get { return ReNewDate; }
            set
            {
                if (value == DateTime.MinValue)
                {
                    ReNewDate = DateTime.Today;
                }
                else
                {
                    ReNewDate = value;
                }
            }
        }

        private DateTime RenewalDue;
        public DateTime RenewalDueBy
        {
            get { return RenewalDue; }
            set { RenewalDue = ReNewDate.AddYears(3); }
        }


        public enum Contact
        {
            Work,
            Personal
        }
        public enum MemberPacRole
        {
            [Display(Name = "NC Staff")]
            NCStaff,
            [Display(Name = "Chair")]
            Chair,
            [Display(Name = "Vice Chair")]
            ViceChair,
            [Display(Name = "Coordinator")]
            Coordinator
        }
        public MemberPacRole MemberRole { get; set; }

        public string CompanyName { get; set; }

        public string CompanyStreetAddress { get; set; }

        public Provinces CompanyProvince { get; set; }

        public string CompanyCity { get; set; }

        [RegularExpression(@"[A-Za-z][0-9][A-Za-z][0-9][A-Za-z][0-9]", ErrorMessage = "Please enter a valid postal code in the format L2H3E9")]
        public string CompanyPostalCode { get; set; }


        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid phone number in the format 1231231234")]
        public string CompanyPhoneNumber { get; set; }

        public string CompanyEmail { get; set; }

        public Contact PreferredContact { get; set; }

        public string CompanyPositionTitle { get; set; }

        public bool IsArchived { get; set; }

        public bool NotRenewing { get; set; }

		public ICollection<MemberDocuments> MemberDocuments { get; set; } = new HashSet<MemberDocuments>();

        public ICollection<ActionItem> ActionItems { get; set; } = new HashSet<ActionItem>();
        public ICollection<MemberActionItems> MemberActionItems { get; set; } = new HashSet<MemberActionItems>();
        public ICollection<MeetingAttendees> Attendees { get; set; } = new HashSet<MeetingAttendees>();
        public ICollection<Meeting> Meetings { get; set; } = new HashSet<Meeting>();

        public int AcademicDivisionID { get; set; }

        public AcademicDivision AcademicDivision { get; set; }

        public int PACID { get; set; }

        public PAC PAC { get; set; }

        public int? SalutationID { get; set; }

        public Salutation Salutation { get; set; }

        public MemberPhoto MemberPhoto { get; set; }

        [Display(Name = "Number of Notification Subscriptions")]
        public int NumberOfPushSubscriptions { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; } = new HashSet<Subscription>();

    }
}
