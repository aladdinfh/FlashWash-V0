using System.ComponentModel.DataAnnotations;

namespace FlashWash.Models
{
    public class UpdateEmailWashViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
