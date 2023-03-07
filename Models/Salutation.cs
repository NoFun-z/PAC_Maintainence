using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NiagaraCollegeProject.Models
{
    public class Salutation
    {
     
        public int? ID { get; set; }

        [Display(Name = "Salutation Title")]
        [Required(ErrorMessage = "You cannot leave the Salutation Title blank.")]
        public string SalutationTitle { get; set; }

        public ICollection<Member> Members { get; set; } = new HashSet<Member>();
    }
}
