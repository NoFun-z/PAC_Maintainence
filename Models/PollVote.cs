using System.ComponentModel.DataAnnotations;

namespace NiagaraCollegeProject.Models
{
    public class PollVote
    {
        public int ID { get; set; }
        [Display(Name = "PollID")]
        public int PollID { get; set; }

        [Display(Name = "Poll")]
        public Poll Poll { get; set; }



        [Display(Name = "OptionText")]
        public int OptionTextID { get; set; }

        [Display(Name = "OptionText")]
        public PollOption OptionText { get; set; }
        public int UserID { get; set; }
    }
}
