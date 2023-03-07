using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace NiagaraCollegeProject.Models
{
    public class AcademicDivision
    {

        public int ID { get; set; }

        [Display(Name = "Division Name")]
        [Required(ErrorMessage = "You cannot leave the committee name blank.")]
        public string DivisionName { get; set; }
        public ICollection<Member> Members { get; set; } = new HashSet<Member>();
        public ICollection<PAC> PACs { get; set; } = new HashSet<PAC>();

    }
}
