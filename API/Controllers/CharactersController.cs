using API_Pokemon.Data.Config;
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
    public class CharactersController : ControllerBase
    {
        private readonly MonsterContext _context;
        private readonly ICombatService _combatService;
        private readonly TuileService _tuileService;
        private readonly QuestService _questService;

        public CharactersController(MonsterContext context, ICombatService combatService, TuileService tuileService, QuestService questService)
        {
            _context = context;
            _combatService = combatService;
            _tuileService = tuileService;
            _questService = questService;
        }

        // Récupérer le personnage associé à un email
        [HttpGet("{email}")]
        public async Task<IActionResult> GetCharacter(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email is required." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var character = await _context.Characters.FirstOrDefaultAsync(c => c.UserId == user.UserId);
            if (character == null)
                return NotFound(new { message = "Character not found for this user." });

            return Ok(new { message = "Character retrieved successfully.", character });
        }

        // Déplacer un personnage
        [HttpPost("move/{x:int}/{y:int}")]
        public async Task<IActionResult> MoveCharacter(int x, int y, [FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required.");

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                    return NotFound("User not found.");

                var character = await _context.Characters.FirstOrDefaultAsync(c => c.UserId == user.UserId);
                if (character == null)
                    return NotFound("Character not found.");

                // Validation des coordonnées
                if (x < 0 || x >= GameConfig.MAP_SIZE || y < 0 || y >= GameConfig.MAP_SIZE)
                    return BadRequest("Destination is outside map boundaries.");

                // Si c'est la ville (10,10) → déplacement sans combat
                if (x == GameConfig.CENTER_X && y == GameConfig.CENTER_Y)
                {
                    character.PositionX = x;
                    character.PositionY = y;
                    await _context.SaveChangesAsync();
                    
                    // Vérifier les quêtes de tuile
                    var centerTileQuestResult = await _questService.CheckTileReached(character.CharacterId, x, y, _context);
                    
                    if (centerTileQuestResult.HasProgress || centerTileQuestResult.HasCompletion)
                    {
                        return Ok(new 
                        { 
                            message = "Character moved successfully.", 
                            character,
                            questProgress = new DTO.QuestProgressInfo
                            {
                                HasProgress = centerTileQuestResult.HasProgress,
                                HasCompletion = centerTileQuestResult.HasCompletion,
                                ProgressMessages = centerTileQuestResult.ProgressMessages,
                                CompletionMessages = centerTileQuestResult.CompletionMessages
                            }
                        });
                    }
                    
                    return Ok(new { message = "Character moved successfully.", character });
                }

                // Récupérer ou créer la tuile de destination
                var destinationTile = _tuileService.GetOrCreateTuile(x, y);
                if (!destinationTile.EstTraversable)
                    return BadRequest("Destination tile is not traversable.");

                // Définir la ville domicile si la tuile est une ville
                if (destinationTile.Type == TypeTuile.VILLE)
                    character.DefinirVilleDomicile(x, y);

                // Vérifier s'il y a un monstre
                var monstre = await _context.InstanceMonstres
                    .Include(m => m.Monstre)
                    .FirstOrDefaultAsync(m => m.PositionX == x && m.PositionY == y);

                if (monstre == null)
                {
                    character.PositionX = x;
                    character.PositionY = y;
                    await _context.SaveChangesAsync();
                    
                    // Vérifier les quêtes de tuile
                    var noMonsterTileQuestResult = await _questService.CheckTileReached(character.CharacterId, x, y, _context);
                    
                    if (noMonsterTileQuestResult.HasProgress || noMonsterTileQuestResult.HasCompletion)
                    {
                        return Ok(new 
                        { 
                            message = "Character moved successfully.", 
                            character,
                            questProgress = new DTO.QuestProgressInfo
                            {
                                HasProgress = noMonsterTileQuestResult.HasProgress,
                                HasCompletion = noMonsterTileQuestResult.HasCompletion,
                                ProgressMessages = noMonsterTileQuestResult.ProgressMessages,
                                CompletionMessages = noMonsterTileQuestResult.CompletionMessages
                            }
                        });
                    }
                    
                    return Ok(new { message = "Character moved successfully.", character });
                }

                // Exécuter le combat
                int levelBefore = character.Level;
                var resultat = _combatService.ExecuterCombat(character, monstre);
                
                // Si le joueur a gagné, le déplacer sur la tuile
                if (resultat.VictoireJoueur)
                {
                    character.PositionX = x;
                    character.PositionY = y;
                }
                
                await _context.SaveChangesAsync();

                // Vérifier les quêtes de monstre si le joueur a gagné
                QuestProgressResult? monsterQuestResult = null;
                if (resultat.VictoireJoueur && monstre.Monstre != null)
                {
                    monsterQuestResult = await _questService.CheckMonsterKilled(character.CharacterId, monstre.Monstre, _context);
                }
                
                // Vérifier les quêtes de tuile après le déplacement (si victoire)
                QuestProgressResult? tileQuestResult = null;
                if (resultat.VictoireJoueur)
                {
                    tileQuestResult = await _questService.CheckTileReached(character.CharacterId, character.PositionX, character.PositionY, _context);
                }
                
                // Vérifier les quêtes de niveau si le niveau a changé
                QuestProgressResult? levelQuestResult = null;
                if (character.Level > levelBefore)
                {
                    levelQuestResult = await _questService.CheckLevelReached(character.CharacterId, character.Level, _context);
                }
                
                // Combiner les résultats de quêtes
                var combinedQuestResult = new QuestProgressResult();
                if (monsterQuestResult != null)
                {
                    combinedQuestResult.HasProgress = combinedQuestResult.HasProgress || monsterQuestResult.HasProgress;
                    combinedQuestResult.HasCompletion = combinedQuestResult.HasCompletion || monsterQuestResult.HasCompletion;
                    combinedQuestResult.ProgressMessages.AddRange(monsterQuestResult.ProgressMessages);
                    combinedQuestResult.CompletionMessages.AddRange(monsterQuestResult.CompletionMessages);
                }
                if (tileQuestResult != null)
                {
                    combinedQuestResult.HasProgress = combinedQuestResult.HasProgress || tileQuestResult.HasProgress;
                    combinedQuestResult.HasCompletion = combinedQuestResult.HasCompletion || tileQuestResult.HasCompletion;
                    combinedQuestResult.ProgressMessages.AddRange(tileQuestResult.ProgressMessages);
                    combinedQuestResult.CompletionMessages.AddRange(tileQuestResult.CompletionMessages);
                }
                if (levelQuestResult != null)
                {
                    combinedQuestResult.HasProgress = combinedQuestResult.HasProgress || levelQuestResult.HasProgress;
                    combinedQuestResult.HasCompletion = combinedQuestResult.HasCompletion || levelQuestResult.HasCompletion;
                    combinedQuestResult.ProgressMessages.AddRange(levelQuestResult.ProgressMessages);
                    combinedQuestResult.CompletionMessages.AddRange(levelQuestResult.CompletionMessages);
                }
                
                if (combinedQuestResult.HasProgress || combinedQuestResult.HasCompletion)
                {
                    return Ok(new
                    {
                        message = resultat.Message,
                        character,
                        combatResult = resultat,
                        questProgress = new DTO.QuestProgressInfo
                        {
                            HasProgress = combinedQuestResult.HasProgress,
                            HasCompletion = combinedQuestResult.HasCompletion,
                            ProgressMessages = combinedQuestResult.ProgressMessages,
                            CompletionMessages = combinedQuestResult.CompletionMessages
                        }
                    });
                }

                return Ok(new
                {
                    message = resultat.Message,
                    character,
                    combatResult = resultat
                });
            }
            catch (Exception ex)
            {
                // Gestion robuste des erreurs
                return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
            }
        }

        // Créer un personnage
        [HttpPost("create/{email}")]
        public async Task<IActionResult> CreateCharacter(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email is required." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound(new { message = "No user found with this email." });

            if (await _context.Characters.AnyAsync(c => c.UserId == user.UserId))
                return BadRequest(new { message = "This user already has a character." });

            var character = new Character(user.UserName, user.UserId);
            await _context.Characters.AddAsync(character);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCharacter), new { email = user.Email }, new { message = "Character created successfully.", character });
        }

        // Ramener le personnage au centre
        [HttpPost("center/{email}")]
        public async Task<IActionResult> CenterCharacter(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email is required." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var character = await _context.Characters.FirstOrDefaultAsync(c => c.UserId == user.UserId);
            if (character == null)
                return NotFound(new { message = "Character not found." });

            // Déplacer le joueur au centre
            character.PositionX = GameConfig.CENTER_X;
            character.PositionY = GameConfig.CENTER_Y;

            // Supprimer un éventuel monstre au centre
            var starterMonster = await _context.InstanceMonstres
                .FirstOrDefaultAsync(m => m.PositionX == GameConfig.CENTER_X && m.PositionY == GameConfig.CENTER_Y);

            if (starterMonster != null)
                _context.InstanceMonstres.Remove(starterMonster);

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Character centered to ({GameConfig.CENTER_X},{GameConfig.CENTER_Y}).", character });
        }

        // Simulation de combat
        [HttpPost("simulate-combat")]
        public async Task<IActionResult> SimulateCombat([FromBody] SimulateCombatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email is required.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return NotFound("User not found.");

            var character = await _context.Characters.FirstOrDefaultAsync(c => c.UserId == user.UserId);
            if (character == null)
                return NotFound("Character not found.");

            var monstre = await _context.InstanceMonstres
                .Include(m => m.Monstre)
                .FirstOrDefaultAsync(m => m.PositionX == request.MonsterX && m.PositionY == request.MonsterY);

            if (monstre == null)
                return NotFound("Monster not found.");

            var resultat = _combatService.SimulerCombat(character, monstre);
            return Ok(resultat);
        }

        [HttpGet("Leaderboard")]
        public async Task<IActionResult> Leaderboard(
        [FromQuery] string category = "level",
        [FromQuery] int limit = 10)
        {
            var characters = await _context.Characters
                .Join(_context.Users,
                      c => c.UserId,
                      u => u.UserId,
                      (c, u) => new { Character = c, Email = u.Email })
                .ToListAsync();

            var query = characters.Select(c => new
            {
                c.Character.CharacterId,
                c.Character.Name,
                c.Character.Level,
                c.Character.Xp,
                c.Character.MaxHp,
                c.Character.Attack,
                c.Character.Defense,
                MonstersHunted = _context.HuntedMonsters.Count(h => h.PlayerEmail == c.Email)
            });

            query = category.ToLower() switch
            {
                "level" => query.OrderByDescending(c => c.Level),
                "maxhp" => query.OrderByDescending(c => c.MaxHp),
                "attack" => query.OrderByDescending(c => c.Attack),
                "defense" => query.OrderByDescending(c => c.Defense),
                _ => query.OrderByDescending(c => c.Level)
            };

            var leaderboard = query
                .Take(limit)
                .ToList();

            return Ok(leaderboard);
        }

    }
}