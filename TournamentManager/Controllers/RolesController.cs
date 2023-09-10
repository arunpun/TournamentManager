using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.Models;

namespace TournamentManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            this.roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create() 
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(ProjectRole role)
        {
            var roleExists = await roleManager.RoleExistsAsync(role.RoleName);

            if (!roleExists)
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role.RoleName));
            }

            return View();
        }
    }
}
