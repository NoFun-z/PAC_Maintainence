using MVC_Music.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NiagaraCollegeProject.Models
{
    public class MeetingMinuteDocuments : UploadedFile
    {
        [Display(Name = "Meeting Minute Documents")]
        public int MeetingMinuteID { get; set; }

        public MeetingMinute MeetingMinute { get; set; }
    }
}
