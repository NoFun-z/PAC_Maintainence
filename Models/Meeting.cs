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
        public DateTime MeetingStartTimeDate { get; set; }

        public ICollection<MeetingMinute> MeetingMinute { get; set; } = new HashSet<MeetingMinute>();
        public ICollection<MeetingAttendees> Attendees { get; set; } = new HashSet<MeetingAttendees>();

        public ICollection<AgendaItem> AgendaItems { get; set; } = new HashSet<AgendaItem>();

        [Display(Name = "Meeting Documents")]
        public ICollection<MeetingDocuments> MeetingDocuments { get; set; } = new HashSet<MeetingDocuments>();

        [Display(Name = "Meeting Notes")]
        [StringLength(300, ErrorMessage = "Meeting Notes cannot be more than 300 characters long.")]
        public string MeetingNotes { get; set; }

        [Display(Name = "Recording Assistant")]
        public int? MemberID { get; set; }

        public Member Member { get; set; }

        [Display(Name = "Divison")]
        public int? AcademicDivisionID { get; set; }

        public AcademicDivision AcademicDivision { get; set; }

        [Display(Name = "PAC")]
        public int? PACID { get; set; }

        public PAC PAC { get; set; }
    }
}