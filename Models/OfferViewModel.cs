
namespace FlashWash.Models

{
    public class OfferViewModel
    {
        public Offer Offer { get; set; }
        public OfferType OfferType { get; set; }

        public List<OfferType> AllOfferTypes { get; set; }
    }
}
