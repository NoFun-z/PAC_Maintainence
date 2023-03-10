using MVC_Music.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NiagaraCollegeProject.Models
{
    public class ActionDocuments : UploadedFile
    {
        [Display(Name = "Action Item")]
        public int ActionItemID { get; set; }

        public ActionItem ActionItem { get; set; }
    }
}
