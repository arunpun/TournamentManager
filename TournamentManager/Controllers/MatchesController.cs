using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TournamentManager.Data;
using TournamentManager.Models;

namespace TournamentManager.Controllers
{
    public class MatchesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MatchesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Matches
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Matches.Include(m => m.TeamA).Include(m => m.TeamB).Include(m => m.Winner);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Matches/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Matches == null)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.Winner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }

        // GET: Matches/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            // Fetch the tournaments and create SelectListItem objects
            var tournaments = _context.Tournaments.ToList();
            var tournamentItems = tournaments.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name
            }).ToList();

            // Add a "Please Select" option at the beginning
            tournamentItems.Insert(0, new SelectListItem
            {
                Value = "0",
                Text = "Please Select"
            });

            ViewData["TournamentId"] = new SelectList(tournamentItems, "Value", "Text");

            // Define the number of rounds to support
            int numberOfRounds = 4; // Change this value as needed

            // Fetch the rounds and create SelectListItem objects
            var rounds = new List<SelectListItem>();

            for (int i = 1; i <= numberOfRounds; i++)
            {
                rounds.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = $"Round {i}"
                });
            }

            // Add a "Please Select" option at the beginning
            rounds.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "Please Select"
            });

            ViewData["Round"] = new SelectList(rounds, "Value", "Text");

            // Add a "Please Select" option for the "Team A" and "Team B" dropdowns
            var teamsWithPleaseSelect = _context.Teams.ToList();
            teamsWithPleaseSelect.Insert(0, new Team { Id = 0, Name = "Please Select" });

            ViewData["TeamAId"] = new SelectList(teamsWithPleaseSelect, "Id", "Name");
            ViewData["TeamBId"] = new SelectList(teamsWithPleaseSelect, "Id", "Name");

            // Add a default option for the "Winner" dropdown
            var teamsForWinner = _context.Teams.ToList();
            teamsForWinner.Insert(0, new Team { Id = 0, Name = "Not Determined" });

            ViewData["WinnerId"] = new SelectList(teamsForWinner, "Id", "Name");

            return View();
        }

        // POST: Matches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AdminUserId,ScheduledTime,TeamAId,TeamBId,IsCompleted,WinnerId,Score,TournamentId,TeamAName,TeamBName,Round")] Match match, int tournamentId)
        {
            if (ModelState.IsValid)
            {
                // Set the TournamentId based on the selected tournament
                match.TournamentId = tournamentId;
                match.WinnerId = null;
                match.ScheduledTime = match.ScheduledTime.ToUniversalTime();
                _context.Add(match);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Populate the ViewBag with the list of tournaments for selection
            ViewBag.TournamentId = new SelectList(_context.Tournaments, "Id", "Name", tournamentId);
            // Populate the ViewBag with the list of teams for selection
            ViewBag.TeamAId = new SelectList(_context.Teams, "Id", "Name", match.TeamAId);
            ViewBag.TeamBId = new SelectList(_context.Teams, "Id", "Name", match.TeamBId);
            ViewBag.WinnerId = new SelectList(_context.Teams, "Id", "Name", match.WinnerId);
            return View(match);
        }


        // GET: Matches/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
            {
                return NotFound();
            }

            // Prepare the select list for WinnerId
            ViewData["WinnerId"] = new SelectList(_context.Teams, "Id", "Name", match.WinnerId);

            return View(match);
        }

        // POST: Matches/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AdminUserId,ScheduledTime,TeamAId,TeamBId,IsCompleted,WinnerId,Score,Round,WinnerName,TeamAName,TeamBName,TournamentId")] Match match)
        {
            if (id != match.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Populate the complex properties based on the selected TeamAId, TeamBId, and WinnerId.
                match.TeamA = await _context.Teams.FindAsync(match.TeamAId);
                match.TeamB = await _context.Teams.FindAsync(match.TeamBId);

                if (match.WinnerId != null)
                {
                    match.Winner = await _context.Teams.FindAsync(match.WinnerId);
                }

                match.ScheduledTime = match.ScheduledTime.ToUniversalTime().AddHours(1);

                try
                {
                    _context.Update(match);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MatchExists(match.Id))
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

            // Repopulate the dropdowns.
            ViewData["WinnerId"] = new SelectList(_context.Teams, "Id", "Name", match.WinnerId);

            return View(match);
        }

        // GET: Matches/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Matches == null)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.Winner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }

        // POST: Matches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Matches == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Matches'  is null.");
            }
            var match = await _context.Matches.FindAsync(id);
            if (match != null)
            {
                _context.Matches.Remove(match);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MatchExists(int id)
        {
          return (_context.Matches?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
