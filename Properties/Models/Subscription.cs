using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NiagaraCollegeProject.Models
{
    public class Subscription
    {
        public int Id { get; set; }

        [StringLength(512)]
        public string PushEndpoint { get; set; }

        [StringLength(512)]
        public string PushP256DH { get; set; }

        [StringLength(512)]
        public string PushAuth { get; set; }

        [Required(ErrorMessage = "You must select the Staff Member.")]
        [Display(Name = "Member")]
        public int MemberID { get; set; }
        public Member Member { get; set; }
    }
}
