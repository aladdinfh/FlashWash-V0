using System.ComponentModel.DataAnnotations;

namespace FlashWash.Models
{
    public class EmailUpdateViewModel
    {
        [Required(ErrorMessage = "Please enter the new email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string NewEmail { get; set; }
    }
}
