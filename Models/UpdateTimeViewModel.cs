using System;
using System.ComponentModel.DataAnnotations;

namespace FlashWash.Models
{
    public class UpdateTimeViewModel
    {
        [Required]
        public TimeSpan MorningStartTime { get; set; }

        [Required]
        public TimeSpan MorningEndTime { get; set; }

        [Required]
        public TimeSpan EveningStartTime { get; set; }

        [Required]
        public TimeSpan EveningEndTime { get; set; }
    }
}
