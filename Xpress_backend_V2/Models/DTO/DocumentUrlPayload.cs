using System.ComponentModel.DataAnnotations;

namespace Xpress_backend_V2.Models.DTO
{
    public class DocumentUrlPayload
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one document URL must be provided.")]
        public List<string> DocumentUrls { get; set; }
    }
}