using System.ComponentModel.DataAnnotations;

namespace NiagaraCollegeProject.Models
{
    public class Poll
    {
        public int ID { get; set; }
      

        [Display(Name = "Question")]
        [Required(ErrorMessage = "You must enter a poll question.")]
        [StringLength(50, ErrorMessage = "Poll questions cannot be more than 50 characters long.")]
        public string Question { get; set; }
        public bool isActive { get; set; }

        [Display(Name = "Expiry Date")]
        [Required(ErrorMessage = "You must select a future expiry date.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy h:mm tt}")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "PAC")]
        [Required(ErrorMessage = "Please select a PAC.")]
        public int PacID { get; set; }
        [Display(Name = "PAC")]
        public PAC PAC { get; set; }

    }
}
