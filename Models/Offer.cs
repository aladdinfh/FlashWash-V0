using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashWash.Models
{
    public class Offer
    {
        [Key]
        public int OfferId { get; set; }

        [Required(ErrorMessage = "The Title field is required.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "The Description field is required.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "The Duration (minutes) field is required.")]
        [Display(Name = "Duration (minutes)")]
        [Range(0, 59, ErrorMessage = "Duration minutes must be between 0 and 59.")]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "The Price field is required.")]
        public int Price { get; set; }

        [Required(ErrorMessage = "The Image URL field is required.")]
        [DataType(DataType.ImageUrl)]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "The OfferTypeId field is required.")]
        public int OfferTypeId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public int WashStationId { get; set; }

        public OfferType? OfferType { get; set; }
        public WashStation? WashStation { get; set; }

        public List<Request> Requests { get; set; } = new List<Request>();
    }


}

