using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NiagaraCollegeProject.Models
{
    public class MeetingMinute
    {
        public int ID { get; set; }
        [Display(Name = "Meeting Comment")]
        [StringLength(200, ErrorMessage = "Meeting comment cannot be more than 200 characters long.")]
        public string MeetingComment { get; set; }

        [Display(Name = "Meeting Minute Documents")]
        public ICollection<MeetingMinuteDocuments> MeetingMinuteDocuments { get; set; } = new HashSet<MeetingMinuteDocuments>();
        public bool IsArchived { get; set; }

        [Display(Name = "MeetingID")]
        public int MeetingID { get; set; }


        [Display(Name = "Meeting")]
        public Meeting Meeting { get; set; }
    }
}
