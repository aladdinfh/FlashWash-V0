using System.ComponentModel.DataAnnotations;

namespace FlashWash.Models
{
    public class TelephoneUpdateViewModel
    {
        [Required(ErrorMessage = "Please enter your telephone number.")]
        public string Telephone { get; set; }
    }

}
