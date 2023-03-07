using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NiagaraCollegeProject.Models
{
    public class MeetingMinute
    {
        public int ID { get; set; }
        [Display(Name = "Meeting Discussion")]
        [StringLength(200, ErrorMessage = "Meeting Discussion cannot be more than 200 characters long.")]
        public string MeetingDiscussion { get; set; }

        [Display(Name = "Duration")]
        [Required(ErrorMessage = "You cannot leave the Duration blank.")]
        public int Duration { get; set; }

        [Display(Name = "Meeting Minute Documents")]
        public ICollection<MeetingMinuteDocuments> MeetingMinuteDocuments { get; set; } = new HashSet<MeetingMinuteDocuments>();
        public bool IsArchived { get; set; }

        public ICollection<AgendaItem> AgendaItems { get; set; } = new HashSet<AgendaItem>();

        [Display(Name = "MeetingID")]
        public int MeetingID { get; set; }


        [Display(Name = "Meeting")]
        public Meeting Meeting { get; set; }
    }
}
