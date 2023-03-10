using MVC_Music.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NiagaraCollegeProject.Models
{
    public class MemberDocuments : UploadedFile
    {
        [Display(Name = "Member Documents")]
        public int MemberID { get; set; }

        public Member Member { get; set; }
    }
}
