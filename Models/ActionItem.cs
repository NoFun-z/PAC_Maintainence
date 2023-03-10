using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NiagaraCollegeProject.Models
{
    public class ActionItem : Auditable
    {
        public int ID { get; set; }

        [Display(Name = "Item Name")]
        [Required(ErrorMessage = "You cannot leave the action name blank.")]
        [StringLength(50, ErrorMessage = "Action name cannot be more than 50 characters long.")]
        public string AgendaName { get; set; }

        [Display(Name = "Description")]
        [Required(ErrorMessage = "You cannot leave the Description blank.")]
        [StringLength(300, ErrorMessage = "Description cannot be more than 30 characters long.")]
        public string Description { get; set; }

        [Display(Name = "Member Comment")]
        [Required(ErrorMessage = "You cannot leave the Comments blank.")]
        [StringLength(300, ErrorMessage = "Comments cannot be more than 300 characters long.")]
        public string Comments { get; set; }

        [Required]
        [Range(typeof(DateTime), "1/1/1900", "{0:MM/dd/yyyy}", ErrorMessage = "Invalid date")]
        public DateTime TaskDueDate { get; set; }

        public bool Completed { get; set; }

        [Display(Name = "Action Item Document")]
        public ICollection<ActionDocuments> ActionDocuments { get; set; } = new HashSet<ActionDocuments>();

        //[Display(Name = "Action Item Supporting Documents")]
        //public ICollection<ActionDocuments> SupportingDocuments { get; set; } = new HashSet<ActionDocuments>();

        [Display(Name = "MeetingID")]
        public int? MeetingID { get; set; }

        [Display(Name = "Meeting")]
        public Meeting Meeting { get; set; }

        [Display(Name = "MemberID")]
        public int MemberID { get; set; }

        [Display(Name = "Member")]
        public Member Member { get; set; }
    }
}
