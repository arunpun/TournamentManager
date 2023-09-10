using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Models
{
    public class Match
    {
        public int Id { get; set; }
        public string? AdminUserId { get; set; }
        public int TournamentId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Scheduled Time")]
        public DateTime ScheduledTime { get; set; }

        [Required]
        [Display(Name = "Round")]
        public int Round { get; set; }

        [Required]
        [Display(Name = "Team A")]
        public int TeamAId { get; set; }
        public string? TeamAName { get; set; }

        [Required]
        [Display(Name = "Team B")]
        public int TeamBId { get; set; }
        public string? TeamBName { get; set; }

        [Display(Name = "Match Completed")]
        public bool IsCompleted { get; set; }
        public int? WinnerId { get; set; }
        public string? WinnerName { get; set; }
        public string? Score { get; set; }

        // Virtuals
        public virtual AppUser? CreatedByAdmin { get; set; }
        public virtual Tournament? Tournament { get; set; }

        [Display(Name = "Team A")]
        public virtual Team? TeamA { get; set; }

        [Display(Name = "Team B")]
        public virtual Team? TeamB { get; set; }
        public virtual Team? Winner { get; set; }
    }
}
