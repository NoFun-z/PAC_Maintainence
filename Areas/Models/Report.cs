using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NiagaraCollegeProject.Models
{
    public class Report : Auditable
    {
        public int ID { get; set; }

        public string Summary { get; set; }

        [Display(Name = "Report Documents")]
        public ICollection<ReportDocuments> ReportDocuments { get; set; } = new HashSet<ReportDocuments>();

        private DateTime UploadDate = DateTime.Today;

        public DateTime UploadedOn
        {
            get { return UploadDate; }
            set { UploadDate = UploadedOn; }
        }

    }
}
