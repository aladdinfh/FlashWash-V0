using System.ComponentModel.DataAnnotations;

namespace FlashWash.Models
{
    public class UpdateTelephoneWashViewModel
    {
        [Required]
        public string Telephone { get; set; }
    }
}
