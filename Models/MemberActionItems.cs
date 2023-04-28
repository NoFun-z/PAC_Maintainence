namespace NiagaraCollegeProject.Models
{
    public class MemberActionItems
    {
        public int ID { get; set; }

        public int ActionItemID { get; set; }
        public ActionItem ActionItem { get; set; }

        public int MemberID { get; set; }
        public Member Member { get; set; }
    }
}
