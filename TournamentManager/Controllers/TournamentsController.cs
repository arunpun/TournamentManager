using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TournamentManager.Data;
using TournamentManager.Models;
using TournamentManager.ViewModels;

namespace TournamentManager.Controllers
{
    public class TournamentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public TournamentsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Tournaments
        public async Task<IActionResult> Index()
        {
            return _context.Tournaments != null ?
                        View(await _context.Tournaments.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Tournaments'  is null.");
        }

        // GET: Tournaments/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null || _context.Tournaments == null)
        //    {
        //        return NotFound();
        //    }

        //    var tournament = await _context.Tournaments.FirstOrDefaultAsync(m => m.Id == id);
        //    var matches = _context.Matches
        //        .Where(m => m.TournamentId == id)
        //        .OrderBy(m => m.Round)
        //        .ThenBy(m => m.Id)
        //        .ToList();

        //    // Initialize a list of rounds to hold the bracket data
        //    var rounds = new List<List<List<Bracket>>>();

        //    int seedCounter = 1; // Initialize the seed counter

        //    var jsonSerializerOptions = new JsonSerializerOptions
        //    {
        //        PropertyNamingPolicy = new LowercaseNamingPolicy()
        //    };

        //    for (int roundNumber = 1; roundNumber <= 4; roundNumber++) // Assuming a maximum of 4 rounds
        //    {
        //        var roundMatches = matches.Where(m => m.Round == roundNumber).ToList();

        //        var roundData = new List<List<Bracket>>();
        //        foreach (var match in roundMatches)
        //        {
        //            // Create BracketTeam objects for Team A and Team B
        //            var teamA = new Bracket { Name = match.TeamAName, Seed = seedCounter++ };
        //            var teamB = new Bracket { Name = match.TeamBName, Seed = seedCounter++ };

        //            // Add the teams to the current round
        //            roundData.Add(new List<Bracket> { teamA, teamB });
        //        }

        //        rounds.Add(roundData);
        //    }

        //    var viewModel = new TournamentDetailsViewModel
        //    {
        //        Matches = matches,
        //        BracketData = rounds,
        //        Tournament = tournament
        //    };

        //    var jsonData = JsonSerializer.Serialize(viewModel.BracketData, jsonSerializerOptions);

        //    ViewData["BracketData"] = jsonData;

        //    return View(viewModel);
        //}

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tournaments == null)
            {
                return NotFound();
            }

            var tournament = await _context.Tournaments.FirstOrDefaultAsync(m => m.Id == id);
            var matches = _context.Matches
                .Where(m => m.TournamentId == id)
                .OrderBy(m => m.Round)
                .ThenBy(m => m.Id)
                .ToList();

            // Initialize a list of rounds to hold the bracket data
            var rounds = new List<List<List<Bracket>>>();

            int seedCounter = 1; // Initialize the seed counter

            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new LowercaseNamingPolicy()
            };

            for (int roundNumber = 1; roundNumber <= 3; roundNumber++) // Assuming a maximum of 3 rounds
            {
                var roundMatches = matches.Where(m => m.Round == roundNumber).ToList();

                var roundData = new List<List<Bracket>>();
                foreach (var match in roundMatches)
                {
                    // Create BracketTeam objects for Team A and Team B
                    var teamA = new Bracket { Name = match.TeamAName, Seed = seedCounter++ };
                    var teamB = new Bracket { Name = match.TeamBName, Seed = seedCounter++ };

                    // Add the teams to the current round
                    roundData.Add(new List<Bracket> { teamA, teamB });
                }

                rounds.Add(roundData);
            }

            // Check if the final match has concluded
            var finalRoundMatches = matches.Where(m => m.Round == 3).ToList();
            if (finalRoundMatches.All(m => m.IsCompleted) && finalRoundMatches.Count == 1)
            {
                var finalMatch = finalRoundMatches.FirstOrDefault();

                // Determine the winning team and update the BracketData
                var winningTeamName = finalMatch.WinnerId == finalMatch.TeamAId
                    ? finalMatch.TeamAName
                    : finalMatch.TeamBName;

                // Create the winning team's Bracket object
                var winningTeam = new Bracket { Name = winningTeamName, Seed = seedCounter++ };

                // Create the fourth round (displaying the winning team)
                rounds.Add(new List<List<Bracket>> { new List<Bracket> { winningTeam } });
            }

            var viewModel = new TournamentDetailsViewModel
            {
                Matches = matches,
                BracketData = rounds,
                Tournament = tournament
            };

            var jsonData = JsonSerializer.Serialize(viewModel.BracketData, jsonSerializerOptions);

            ViewData["BracketData"] = jsonData;

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        // GET: Tournaments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tournaments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AdminUserId,Name,StartTime")] Tournament tournament)
        {
            if (ModelState.IsValid)
            {
                tournament.StartTime = tournament.StartTime.ToUniversalTime();
                _context.Add(tournament);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tournament);
        }

        // GET: Tournaments/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tournaments == null)
            {
                return NotFound();
            }

            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }
            return View(tournament);
        }

        // POST: Tournaments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AdminUserId,Name,StartTime")] Tournament tournament)
        {
            if (id != tournament.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tournament);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TournamentExists(tournament.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tournament);
        }

        // GET: Tournaments/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tournaments == null)
            {
                return NotFound();
            }

            var tournament = await _context.Tournaments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tournament == null)
            {
                return NotFound();
            }

            return View(tournament);
        }

        // POST: Tournaments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tournaments == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tournaments'  is null.");
            }
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament != null)
            {
                _context.Tournaments.Remove(tournament);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TournamentExists(int id)
        {
            return (_context.Tournaments?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [Authorize]
        public async Task<IActionResult> SignUp(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var tournament = await _context.Tournaments
                .Include(t => t.Teams) // Include the Teams navigation property
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return NotFound();
            }

            if (tournament.Teams.Any(team => team.AppUserId == currentUser.Id))
            {
                // User has already signed up their team for this tournament
                TempData["InfoMessage"] = "Your team has already signed up for this tournament.";
            }
            else
            {
                // Add the user's team to the tournament.
                var userTeam = await _context.Teams.FirstOrDefaultAsync(team => team.AppUserId == currentUser.Id);

                if (userTeam != null)
                {
                    // Associate the team with the tournament
                    tournament.Teams.Add(userTeam);

                    // Increment the number of teams signed up
                    tournament.TeamsSignedUp++;

                    await _context.SaveChangesAsync();

                    // Set a success message in TempData
                    TempData["SuccessMessage"] = "You have successfully joined the tournament!";
                }
                else
                {
                    // Handle the case where the user does not have a team.
                    TempData["ErrorMessage"] = "You need to create a team before joining a tournament.";
                }
            }

            // Redirect to the tournament index page
            return RedirectToAction("Index");
        }

        [Authorize]
        public async Task<IActionResult> Withdraw(int tournamentId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
            {
                return NotFound();
            }

            var userTeam = tournament.Teams?.FirstOrDefault(team => team.AppUserId == currentUser.Id);

            if (userTeam != null)
            {
                // Remove the team from the tournament
                tournament.Teams.Remove(userTeam);

                // Decrement the number of teams signed up
                tournament.TeamsSignedUp--;

                await _context.SaveChangesAsync();

                // Set a success message in TempData
                TempData["SuccessMessage"] = "You have successfully withdrawn from the tournament.";
            }
            else
            {
                // Handle the case where the user's team is not signed up for the tournament.
                TempData["ErrorMessage"] = "Your team is not signed up for this tournament.";
            }

            return RedirectToAction("Index");
        }
    }
}
