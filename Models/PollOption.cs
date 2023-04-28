using System.ComponentModel.DataAnnotations;

namespace NiagaraCollegeProject.Models
{
    public class PollOption
    {
        public int ID { get; set; }
        [Display(Name = "Poll Option")]
        [Required(ErrorMessage = "You must enter a poll option.")]
        [StringLength(50, ErrorMessage = "Poll options cannot be more than 100 characters long.")]
        public string OptionText { get; set; }
        [Display(Name = "Votes")]
        public int Votes { get; set; }
        [Display(Name = "PollID")]
        [Required(ErrorMessage = "You must select a Poll.")]
        public int PollID { get; set; }

        [Display(Name = "Poll")]
        public Poll Poll { get; set; }
    }
}
