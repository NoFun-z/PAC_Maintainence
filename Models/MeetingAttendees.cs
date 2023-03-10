using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Xml.Linq;

namespace NiagaraCollegeProject.Models
{
    public class MeetingAttendees
    {
        public int ID { get; set; }

        public int MeetingID { get; set; }
        public Meeting Meeting { get; set; }

        public int MemberID { get; set; }
        public Member Member { get; set; }
    }
}
