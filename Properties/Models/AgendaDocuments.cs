using MVC_Music.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NiagaraCollegeProject.Models
{
    public class AgendaDocuments : UploadedFile
    {
        [Display(Name = "Agenda")]
        public int AgendaID { get; set; }

        public AgendaItem AgendaItem { get; set; }
    }
}
