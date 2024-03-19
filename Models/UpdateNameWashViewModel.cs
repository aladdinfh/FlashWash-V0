using System.ComponentModel.DataAnnotations;

namespace FlashWash.Models
{
    public class UpdateNameWashViewModel
    {
        [Required]
        public string NewName { get; set; }
    }
}
