#pragma warning disable CS8618

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashWash.Models
{
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            DateTime dateTime = (DateTime)value;
            return dateTime > DateTime.Now;
        }
    }

    public class Request
    {
        [Key]
        public int RequestId { get; set; }

        public int? UserId { get; set; }

        public User? User { get; set; }

        public int OfferId { get; set; }

        public Offer? Offer { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm}")]
        public DateTime StartTime { get; set; }

        [Required]
        public RequestStatus Status { get; set; }
    }
}
