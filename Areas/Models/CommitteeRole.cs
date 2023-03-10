using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NiagaraCollegeProject.Models
{
    public class CommitteeRole
    {
        public int ID { get; set; }

        public string Role { get; set; }

        [Display(Name = "CommitteeID")]
        public int CommitteeID { get; set; }


        [Display(Name = "Committee")]
        public AcademicDivision Committee { get; set; }

        public ICollection<Member> Members { get; set; } = new HashSet<Member>();

    }
}
