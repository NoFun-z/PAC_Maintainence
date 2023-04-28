using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace NiagaraCollegeProject.Models
{
    public class PAC : Auditable
    {

        public int ID { get; set; }

        [Display(Name = "PAC Name")]
        [Required(ErrorMessage = "You cannot leave the PAC Name blank.")]
        public string PACName { get; set; }
        public ICollection<Member> Members { get; set; } = new HashSet<Member>();
        public ICollection<Meeting> Meetings { get; set; } = new HashSet<Meeting>();

        [Display(Name = "School Name")]
        public int AcademicDivisionID { get; set; }

        [Display(Name = "School Name")]
        public AcademicDivision AcademicDivision { get; set; }

    }
}
