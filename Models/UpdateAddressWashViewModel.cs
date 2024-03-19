using System.ComponentModel.DataAnnotations;

namespace FlashWash.Models
{
    public class UpdateAddressWashViewModel
    {
        [Required(ErrorMessage = "Please enter the new street.")]
        public string Street { get; set; }

        [Required(ErrorMessage = "Please enter the new city.")]
        public string City { get; set; }

        [Required(ErrorMessage = "Please enter the new state.")]
        public string State { get; set; }

        [Required(ErrorMessage = "Please enter the new postal code.")]
        public string PostalCode { get; set; }
    }
}
