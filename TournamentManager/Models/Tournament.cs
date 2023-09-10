using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Models
{
    public class Tournament
    {
        public int Id { get; set; }
        public string? AdminUserId { get; set; }

        [Required]
        [Display(Name = "Tournament Name")]
        public string? Name { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Event Start Time")]
        public DateTime StartTime { get; set; }

        [Display(Name = "Number of Teams")]
        public int TeamsSignedUp { get; set; }
        // Virtuals
        public virtual AppUser? CreatedByAdmin { get; set; }
        public virtual ICollection<Team>? Teams { get; set; } = new HashSet<Team>();
        public virtual ICollection<Match>? Matches { get; set; } = new HashSet<Match>();
    }
}
