using System.ComponentModel.DataAnnotations;

namespace Web_TracNghiem_HTSV.Models
{
    public class RoleViewModel
    {
        public string RoleId { get; set; }

        [Required(ErrorMessage = "Role Name is required.")]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; }
    }
}
