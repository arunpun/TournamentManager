using System.Text.Json;
using TournamentManager.Models;

namespace TournamentManager.ViewModels
{
    public class TournamentDetailsViewModel
    {
        public Tournament Tournament { get; set; }
        public List<Match> Matches { get; set; }
        public List<List<List<Bracket>>> BracketData { get; set; }
    }
}
