using Microsoft.AspNetCore.Mvc;
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

        public bool MemberStatus { get; set; }


        public enum Contact
        {
            Work,
            Personal
        }

        public string CompanyName { get; set; }

        public string CompanyStreetAddress { get; set; }

        public Provinces CompanyProvince { get; set; }


        public string CompanyCity { get; set; }

        [RegularExpression(@"[A-Za-z][0-9][A-Za-z][0-9][A-Za-z][0-9]", ErrorMessage = "Please enter a valid postal code in the format L#L#L#")]
        public string CompanyPostalCode { get; set; }


        [RegularExpression("^\\d{10}$", ErrorMessage = "Not a valid phone number")]
        public string CompanyPhoneNumber { get; set; }

        public string CompanyEmail { get; set; }

        public Contact PreferredContact { get; set; }

        public string CompanyPositionTitle { get; set; }

        public bool IsArchived { get; set; }

        public ICollection<MemberDocuments> MemberDocuments { get; set; } = new HashSet<MemberDocuments>();

        public ICollection<ActionItem> ActionItems { get; set; } = new HashSet<ActionItem>();
        public ICollection<MeetingAttendees> Attendees { get; set; } = new HashSet<MeetingAttendees>();

        public int? AcademicDivisionID { get; set; }

        public AcademicDivision AcademicDivision { get; set; }

        public int? PACID { get; set; }

        public PAC PAC { get; set; }

        public int? SalutationID { get; set; }

        public Salutation Salutation { get; set; }

        public MemberPhoto MemberPhoto { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; } = new HashSet<Subscription>();

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{

        //    if (PhoneNumber == "")
        //    {
        //        yield return new ValidationResult("You cannot leave the phone number blank.", new[] { "PhoneNumber" });
        //    }
        //    if (StreetAddress == "")
        //    {
        //        yield return new ValidationResult("You cannot leave the street address blank.", new[] { "StreetAddress" });
        //    }
        //    if (City == "")
        //    {
        //        yield return new ValidationResult("You cannot leave the city blank.", new[] { "City" });
        //    }
        //    if (PostalCode == "")
        //    {
        //        yield return new ValidationResult("You cannot leave the postal code blank.", new[] { "PostalCode" });
        //    }
        //    if (CompanyPositionTitle == "")
        //    {
        //        yield return new ValidationResult("You cannot leave the position title blank.", new[] { "CompanyPositionTitle" });
        //    }
        //    if (CompanyName == "")
        //    {
        //        yield return new ValidationResult("You cannot leave company name blank.", new[] { "CompanyName" });
        //    }
        //    if (CompanyStreetAddress == "")
        //    {
        //        yield return new ValidationResult("You cannot leave company address blank.", new[] { "CompanyStreetAddress" });
        //    }
        //    if (CompanyCity == "")
        //    {
        //        yield return new ValidationResult("You cannot leave company city blank.", new[] { "CompanyCity" });
        //    }
        //    if (CompanyPostalCode == "")
        //    {
        //        yield return new ValidationResult("You cannot leave company postal code blank.", new[] { "CompanyPostalCode" });

        //    }
        //    if (CompanyPhoneNumber == "")
        //    {
        //        yield return new ValidationResult("You cannot leave company phone number blank.", new[] { "CompanyPhoneNumber" });
        //    }
        //    if (CompanyEmail == "")
        //    {
        //        yield return new ValidationResult("You cannot leave company email blank.", new[] { "CompanyEmail" });
        //    }
        //}
    }
}
