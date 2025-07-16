using System.ComponentModel.DataAnnotations;

namespace Xpress_backend_V2.Models.DTO
{
    public class ManagerApprovalDTO
    {
        public int ApprovingUserId { get; set; } // The ID of the manager approving
        public string? Comments { get; set; }
        [Required(ErrorMessage = "Please specify if the travel is billable.")]
        public bool IsBillable { get; set; }
    }
}
