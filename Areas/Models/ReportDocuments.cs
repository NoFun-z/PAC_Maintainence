using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Xml.Linq;
using MVC_Music.Models;

namespace NiagaraCollegeProject.Models
{
    // 
    public class ReportDocuments : UploadedFile
    {
        [Display(Name = "Report Documents")]
        public int ReportID { get; set; }

        public Report Report { get; set; }
    }
}
