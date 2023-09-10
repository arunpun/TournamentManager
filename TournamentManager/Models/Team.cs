using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string? AppUserId { get; set; }

        [Required]
        [Display(Name = "Team Name")]
        public string? Name { get; set;}

        // Virtuals
        public virtual AppUser? CreatedByUser { get; set; }
        public virtual ICollection<Tournament>? Tournaments { get; set; } = new HashSet<Tournament>();
        public virtual ICollection<Match>? Matches { get; set; } = new HashSet<Match>();
    }
}
