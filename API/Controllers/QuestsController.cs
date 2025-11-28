using API_Pokemon.Data.Context;
using API_Pokemon.Models;
using API_Pokemon.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static API_Pokemon.Models.DTO;

namespace API_Pokemon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestsController : ControllerBase
    {
        private readonly MonsterContext _context;
        private readonly QuestService _questService;

        public QuestsController(MonsterContext context, QuestService questService)
        {
            _context = context;
            _questService = questService;
        }

        // GET: api/Quests/{email}
        [HttpGet("{email}")]
        public async Task<IActionResult> GetQuests(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email is required." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var character = await _context.Characters.FirstOrDefaultAsync(c => c.UserId == user.UserId);
            if (character == null)
                return NotFound(new { message = "Character not found." });

            var quests = await _questService.GetActiveQuestsForCharacter(character.CharacterId, _context);

            return Ok(new { message = "Quests retrieved successfully.", quests });
        }
    }
}

