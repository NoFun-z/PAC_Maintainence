using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Xml.Linq;

namespace NiagaraCollegeProject.Models
{
    public class MemberMetaData
    {
        [Display(Name = "Member")]
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

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "You cannot leave the first name blank.")]
        [StringLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "You cannot leave the last name blank.")]
        [StringLength(100, ErrorMessage = "Last name cannot be more than 100 characters long.")]
        public string LastName { get; set; }

        //[Required(ErrorMessage = "You must provide a phone number")]
        [Display(Name = "Phone")]
        //[Required(ErrorMessage = "You cannot leave the phone number blank.")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "You cannot leave the email blank.")]
        [StringLength(50, ErrorMessage = "Email name cannot be more than 70 characters long.")]
        //[RegularExpression("^\\S+@\\S+\\.\\S+$", ErrorMessage = "Please enter a valid email address. Ex. test123@hotmail.com")]
        public string Email { get; set; }

        [Display(Name = "Education Summary")]
        [DataType(DataType.MultilineText)]
        [StringLength(300, ErrorMessage = "Education summary cannot be more than 300 characters long.")]
        public string EducationSummary { get; set; }

        [Display(Name = "Occupational Summary")]
        [DataType(DataType.MultilineText)]
        [StringLength(300, ErrorMessage = "Occupational Summary cannot be more than 300 characters long.")]
        public string OccupationalSummary { get; set; }

        [Display(Name = "Street Address")]
        //[Required(ErrorMessage = "You cannot leave the street address blank.")]
        [StringLength(50, ErrorMessage = "Street address cannot be more than 50 characters long.")]
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
        [Display(Name = "Province")]
        //[Required(ErrorMessage = "Please select a province.")]
        public Provinces Province { get; set; }

        [Display(Name = "City")]
        //[Required(ErrorMessage = "You cannot leave the city blank.")]
        [StringLength(30, ErrorMessage = "City cannot be more than 30 characters long.")]
        public string City { get; set; }

        [Display(Name = "Postal Code")]
        //[Required(ErrorMessage = "You cannot leave the postal code blank.")]
        [StringLength(6, ErrorMessage = "Postal code cannot be more than 6 characters long.")]
        public string PostalCode { get; set; }       

        [Display(Name = "NC Graduate")]
        public bool NCGraduate { get; set; }

        private DateTime signUpDate { get; set; }
        [Display(Name = "Date Joined")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
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
        [Display(Name = "Last Updated")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
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
        [Display(Name = "Re-new By")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
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
        [Display(Name = "Member Role")]
        [Required(ErrorMessage = "Please select a member role.")]

        public MemberPacRole? MemberRole { get; set; }

        [Display(Name = "Preferred Email")]
        //[Required(ErrorMessage = "Please select a preferred contact method.")]
        public Contact PreferredContact { get; set; }

        [Display(Name = "Documents")]
        public ICollection<MemberDocuments> MemberDocuments { get; set; } = new HashSet<MemberDocuments>();

        [Display(Name = "Action Items")]
        public ICollection<ActionItem> ActionItems { get; set; } = new HashSet<ActionItem>();
        [Display(Name = "School Name")]
        [Required(ErrorMessage = "You must select a school.")]

        public int AcademicDivisionID { get; set; }

        [Display(Name = "School Name")]
        public AcademicDivision AcademicDivision { get; set; }

        [Display(Name = "PACID")]
        [Required(ErrorMessage = "You must select a PAC.")]
        public int PACID { get; set; }

        [Display(Name = "PAC")]
        public PAC PAC { get; set; }

        [Display(Name = "SalutationID")]
        public int? SalutationID { get; set; }

        [Display(Name = "Salutation")]
        public Salutation Salutation { get; set; }


        [Display(Name = "Position Title")]
        //[Required(ErrorMessage = "You cannot leave the position title blank.")]
        [StringLength(30, ErrorMessage = "Position title cannot be more than 30 characters long.")]
        public string CompanyPositionTitle { get; set; }

        [Display(Name = "Company")]
        //[Required(ErrorMessage = "You cannot leave company name blank.")]
        public string CompanyName { get; set; }

        [Display(Name = "Company Address")]
        //[Required(ErrorMessage = "You cannot leave company address blank.")]
        public string CompanyStreetAddress { get; set; }

        [Display(Name = "Company Province")]
        //[Required(ErrorMessage = "Please select a province.")]
        public Provinces CompanyProvince { get; set; }

        [Display(Name = "Company City")]
        //[Required(ErrorMessage = "You cannot leave company city blank.")]
        public string CompanyCity { get; set; }

        [Display(Name = "Company Postal Code")]
        //[Required(ErrorMessage = "You cannot leave company postal code blank.")]
        [RegularExpression(@"[A-Za-z][0-9][A-Za-z][0-9][A-Za-z][0-9]", ErrorMessage = "Please enter a valid postal code in the format L#L#L#. Example. L3C1H6")]
        public string CompanyPostalCode { get; set; }

        [Display(Name = "Company Phone Number")]
        //[Required(ErrorMessage = "You cannot leave company phone number blank.")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Not a valid phone number")]
        public string CompanyPhoneNumber { get; set; }

        [Display(Name = "Company Email")]
        //[Required(ErrorMessage = "You cannot leave company email blank.")]
        public string CompanyEmail { get; set; }

        public bool IsArchived { get; set; }
    }
}
