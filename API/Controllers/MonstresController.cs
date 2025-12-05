using API_Pokemon.Data.Context;
using API_Pokemon.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static API_Pokemon.Models.DTO;

namespace API_Pokemon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonstresController : ControllerBase
    {
        private readonly MonsterContext _context;
        private const int DEFAULT_LIMIT = 20;
        private const int MAX_LIMIT = 50;

        public MonstresController(MonsterContext context)
        {
            _context = context;
        }

        [HttpGet("Pokedex")]
        public async Task<IActionResult> GetPokedex(
    [FromQuery] string? email,        // <-- changement ici
    [FromQuery] string? name,
    [FromQuery] string? types,
    [FromQuery] int offset = 0,
    [FromQuery] int limit = DEFAULT_LIMIT,
    CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required.");

            if (limit <= 0) limit = DEFAULT_LIMIT;
            if (limit > MAX_LIMIT) limit = MAX_LIMIT;
            if (offset < 0) offset = 0;

            // Trouver l'utilisateur
            var joueur = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
            if (joueur == null)
                return NotFound("No user found with this email.");

            // Préparation du filtrage par types
            string[]? typeList = null;
            if (!string.IsNullOrWhiteSpace(types))
            {
                typeList = types.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => t.Trim())
                                .Where(t => t.Length > 0)
                                .ToArray();
            }

            // Base: sélection des monstres
            var query = _context.Monster.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var nameLower = name.Trim().ToLower();
                query = query.Where(m => m.nom.ToLower().Contains(nameLower));
            }

            if (typeList != null && typeList.Length > 0)
            {
                query = query.Where(m => typeList.Contains(m.type1) || typeList.Contains(m.type2));
            }

            var total = await query.CountAsync(ct);

            var monsters = await query
                .OrderBy(m => m.nom)
                .Skip(offset)
                .Take(limit)
                .ToListAsync(ct);

            var monsterIds = monsters.Select(m => m.idMonster).ToArray();

            // Vérification des monstres chassés par email
            var huntedSet = await _context.HuntedMonsters
                .Where(h => h.PlayerEmail == email && monsterIds.Contains(h.MonsterId))
                .Select(h => h.MonsterId)
                .ToListAsync(ct);

            var items = monsters.Select(m => new MonsterPokedexDto
            {
                Id = m.idMonster,
                Name = m.nom,
                SpriteUrl = m.spriteURL,
                Type1 = m.type1,
                Type2 = m.type2,
                IsHunted = huntedSet.Contains(m.idMonster)
            }).ToList();

            var result = new PagedResult<MonsterPokedexDto>
            {
                Items = items,
                Total = total,
                Offset = offset,
                Limit = limit
            };

            return Ok(result);
        }
    }
}
