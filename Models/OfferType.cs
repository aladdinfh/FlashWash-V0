#pragma warning disable CS8618

using System.ComponentModel.DataAnnotations;

namespace FlashWash.Models
{
    public class OfferType
    {
        [Key]
        public int OfferTypeId { get; set; }
        [Required(ErrorMessage ="Please enter a the offer type ")]
        public string Title { get; set; }



    }
}
