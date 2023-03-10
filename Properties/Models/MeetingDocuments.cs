using MVC_Music.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NiagaraCollegeProject.Models
{
    public class MeetingDocuments : UploadedFile
    {
        [Display(Name = "Meeting Documents")]
        public int MeetingID { get; set; }

        public Meeting Meeting { get; set; }
    }
}
