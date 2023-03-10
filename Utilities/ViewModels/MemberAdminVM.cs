using Microsoft.AspNetCore.Mvc;
using NiagaraCollegeProject.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NiagaraCollegeProject.ViewModels
{
    /// <summary>
    /// Add back in any Restricted Properties and list of UserRoles
    /// </summary>
    [ModelMetadataType(typeof(MemberMetaData))]
    public class MemberAdminVM : MemberVM
    {
        //public string Email { get; set; }
        //public bool MemberStatus { get; set; }

        [Display(Name = "Roles")]
        public List<string> UserRoles { get; set; } = new List<string>();
    }
}
