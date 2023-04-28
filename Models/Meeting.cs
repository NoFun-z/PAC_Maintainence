using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NiagaraCollegeProject.Models
{
    public class Meeting : Auditable
    {
        public int ID { get; set; }

        [Display(Name = "Meeting Topic/Name")]
        [Required(ErrorMessage = "You cannot leave the Meeting Topic/Name blank.")]
        public string MeetingTopicName { get; set; }

        [Display(Name = "Meeting Start Date")]
        [Required(ErrorMessage = "Meeting must have a start date.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy h:mm tt}")]

        public DateTime MeetingStartTimeDate { get; set; }

        public ICollection<MeetingMinute> MeetingMinute { get; set; } = new HashSet<MeetingMinute>();
        public ICollection<MeetingAttendees> Attendees { get; set; } = new HashSet<MeetingAttendees>();


        [Display(Name = "Meeting Documents")]
        public ICollection<MeetingDocuments> MeetingDocuments { get; set; } = new HashSet<MeetingDocuments>();

        [Display(Name = "Meeting Notes")]
        [StringLength(300, ErrorMessage = "Meeting Notes cannot be more than 300 characters long.")]
        public string MeetingNotes { get; set; }

        public bool IsArchived { get; set; }

        [Required(ErrorMessage = "You must select a recording assistant.")]
        [Display(Name = "Recording Assistant")]
        public int MemberID { get; set; }
        public Member Member { get; set; }

        [Required(ErrorMessage = "You must select a school.")]
        [Display(Name = "School Name")]
        public int AcademicDivisionID { get; set; }

        public AcademicDivision AcademicDivision { get; set; }

        [Required(ErrorMessage = "You must select a PAC.")]
        [Display(Name = "PAC")]
        public int PACID { get; set; }

        public PAC PAC { get; set; }
    }
}