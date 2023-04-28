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

        private DateTime _uploadedDate;

        public DateTime UploadedDate
        {
            get { return _uploadedDate; }
            set { _uploadedDate = value; }
        }

        public MeetingDocuments()
        {
            UploadedDate = DateTime.Now;
        }
        public bool IsArchived { get; set; }
    }
}
