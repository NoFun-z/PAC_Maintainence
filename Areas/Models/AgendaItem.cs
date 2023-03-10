using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NiagaraCollegeProject.Models
{
    public class AgendaItem
    {
        public int ID { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "You cannot leave the agenda name blank.")]
        [StringLength(50, ErrorMessage = "Agenda name cannot be more than 50 characters long.")]
        public string AgendaName { get; set; }

        [Display(Name = "Documents")]
        public ICollection<AgendaDocuments> AgendaDocuments { get; set; } = new HashSet<AgendaDocuments>();

        private DateTime uploadDate = DateTime.Today;
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime UploadDate
        {
            get { return uploadDate; }
            set { uploadDate = UploadDate; }
        }

        [Display(Name = "MemberID")]
        public int MemberID { get; set; }

        [Display(Name = "Member")]
        public Member Member { get; set; }

        [Display(Name = "MeetingID")]
        public int MeetingID { get; set; }

        [Display(Name = "Meeting")]
        public Meeting Meeting { get; set; }
    }
}
