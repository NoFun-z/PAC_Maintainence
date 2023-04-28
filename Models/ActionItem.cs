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
        [StringLength(300, ErrorMessage = "Description cannot be more than 30 characters long.")]
        public string Description { get; set; }

        [Display(Name = "Member Comment")]
        [StringLength(300, ErrorMessage = "Comments cannot be more than 300 characters long.")]
        public string Comments { get; set; }

        [Display(Name = "Due Date")]
        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy h:mm tt}")]
        public DateTime TaskDueDate { get; set; }

        public bool Completed { get; set; }

        public bool IsArchived { get; set; }

        [Display(Name = "Action Item Document")]
        public ICollection<ActionDocuments> ActionDocuments { get; set; } = new HashSet<ActionDocuments>();
        public ICollection<MemberActionItems> MemberActionItems { get; set; } = new HashSet<MemberActionItems>();

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
