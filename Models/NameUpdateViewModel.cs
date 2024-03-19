using System.ComponentModel.DataAnnotations;

namespace FlashWash.Models
{
    public class NameUpdateViewModel
    {
        [Required(ErrorMessage = "Please enter the new first name.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter the new last name.")]
        public string LastName { get; set; }
    }
}
