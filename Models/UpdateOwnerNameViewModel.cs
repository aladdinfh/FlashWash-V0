using System.ComponentModel.DataAnnotations;

namespace FlashWash.Models
{
    public class UpdateOwnerNameViewModel
    {
        [Required]
        public string OwnerName { get; set; }
    }
}
