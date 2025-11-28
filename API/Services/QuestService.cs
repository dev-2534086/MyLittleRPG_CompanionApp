using API_Pokemon.Data.Config;
using API_Pokemon.Data.Context;
using API_Pokemon.Models;
using API_Pokemon.Models.Quest;
using Microsoft.EntityFrameworkCore;

namespace API_Pokemon.Services
{
    public class QuestProgressResult
    {
        public bool HasProgress { get; set; }
        public bool HasCompletion { get; set; }
        public List<string> ProgressMessages { get; set; } = new List<string>();
        public List<string> CompletionMessages { get; set; } = new List<string>();
    }

    public class QuestService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Random _random = new();
        private Timer? _questGenerationTimer;

        public QuestService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            // Démarrer le timer pour générer des quêtes toutes les 10 minutes
            _questGenerationTimer = new Timer(GenerateQuestsForAllCharacters, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
        }

        private async void GenerateQuestsForAllCharacters(object? state)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<MonsterContext>();
                
                var characters = await context.Characters.ToListAsync();
                
                foreach (var character in characters)
                {
                    await EnsureCharacterHasEnoughQuests(character.CharacterId, context);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erreur lors de la génération de quêtes: {ex.Message}");
            }
        }

        public async Task EnsureCharacterHasEnoughQuests(int characterId, MonsterContext? context = null)
        {
            using var scope = context == null ? _serviceScopeFactory.CreateScope() : null;
            var dbContext = context ?? scope!.ServiceProvider.GetRequiredService<MonsterContext>();
            
            var activeQuestsCount = await GetActiveQuestsCount(characterId, dbContext);
            
            if (activeQuestsCount < 3)
            {
                await GenerateRandomQuest(characterId, dbContext);
            }
        }

        private async Task<int> GetActiveQuestsCount(int characterId, MonsterContext context)
        {
            var tileQuests = await context.QuestTiles
                .CountAsync(q => q.CharacterId == characterId && q.IsActive && !q.IsCompleted);
            
            var monsterQuests = await context.QuestMonster
                .CountAsync(q => q.CharacterId == characterId && q.IsActive && !q.IsCompleted);
            
            var levelQuests = await context.QuestLevel
                .CountAsync(q => q.CharacterId == characterId && q.IsActive && !q.IsCompleted);

            return tileQuests + monsterQuests + levelQuests;
        }

        private async Task GenerateRandomQuest(int characterId, MonsterContext context)
        {
            var questType = _random.Next(3); // 0 = Tile, 1 = Monster, 2 = Level
            
            switch (questType)
            {
                case 0:
                    await GenerateTileQuest(characterId, context);
                    break;
                case 1:
                    await GenerateMonsterQuest(characterId, context);
                    break;
                case 2:
                    await GenerateLevelQuest(characterId, context);
                    break;
            }
        }

        private async Task GenerateTileQuest(int characterId, MonsterContext context)
        {
            // Générer une position de tuile aléatoire existante ou accessible
            int x, y;
            int attempts = 0;
            do
            {
                x = _random.Next(0, GameConfig.MAP_SIZE);
                y = _random.Next(0, GameConfig.MAP_SIZE);
                attempts++;
            } while ((x == GameConfig.CENTER_X && y == GameConfig.CENTER_Y) && attempts < 100);

            var quest = new QuestTile(characterId)
            {
                Title = $"Atteindre la position ({x}, {y})",
                Description = $"Voyagez jusqu'à la position ({x}, {y}) sur la carte.",
                TilePositionX = x,
                TilePositionY = y
            };

            await context.QuestTiles.AddAsync(quest);
            context.SaveChanges();
        }

        private async Task GenerateMonsterQuest(int characterId, MonsterContext context)
        {
            // Récupérer un type de monstre aléatoire
            var monsters = await context.Monster.ToListAsync();
            if (!monsters.Any())
                return;

            var randomMonster = monsters[_random.Next(monsters.Count)];
            var monsterType = randomMonster.type1; // Utiliser le type1 comme type de monstre
            
            // Générer un objectif raisonnable (entre 3 et 10 monstres)
            var goal = _random.Next(3, 11);

            var quest = new QuestMonster(characterId)
            {
                Title = $"Vaincre {goal} {monsterType}",
                Description = $"Défaites {goal} monstres de type {monsterType}.",
                MonsterType = monsterType,
                GoalMonster = goal
            };

            await context.QuestMonster.AddAsync(quest);
            context.SaveChanges();
        }

        private async Task GenerateLevelQuest(int characterId, MonsterContext context)
        {
            var character = await context.Characters.FindAsync(characterId);
            if (character == null)
                return;

            // Générer un objectif de niveau raisonnable (niveau actuel + 1 à +5)
            var goalLevel = character.Level + _random.Next(1, 6);

            var quest = new QuestLevel(characterId)
            {
                Title = $"Atteindre le niveau {goalLevel}",
                Description = $"Gagnez de l'expérience jusqu'à atteindre le niveau {goalLevel}.",
                GoalLevel = goalLevel
            };

            await context.QuestLevel.AddAsync(quest);
            context.SaveChanges();
        }

        public async Task<QuestProgressResult> CheckTileReached(int characterId, int x, int y, MonsterContext? context = null)
        {
            using var scope = context == null ? _serviceScopeFactory.CreateScope() : null;
            var dbContext = context ?? scope!.ServiceProvider.GetRequiredService<MonsterContext>();
            
            var result = new QuestProgressResult();
            
            var activeQuests = await dbContext.QuestTiles
                .Where(q => q.CharacterId == characterId && q.IsActive && !q.IsCompleted)
                .ToListAsync();

            foreach (var quest in activeQuests)
            {
                if (quest.TilePositionX == x && quest.TilePositionY == y)
                {
                    quest.IsCompleted = true;
                    quest.IsActive = false;
                    result.HasCompletion = true;
                    result.CompletionMessages.Add($"Quête complétée: {quest.Title}");
                }
            }

            if (result.HasCompletion)
            {
                dbContext.SaveChanges();
            }

            return result;
        }

        public async Task<QuestProgressResult> CheckMonsterKilled(int characterId, Monster monster, MonsterContext? context = null)
        {
            using var scope = context == null ? _serviceScopeFactory.CreateScope() : null;
            var dbContext = context ?? scope!.ServiceProvider.GetRequiredService<MonsterContext>();
            
            var result = new QuestProgressResult();
            
            var activeQuests = await dbContext.QuestMonster
                .Where(q => q.CharacterId == characterId && q.IsActive && !q.IsCompleted)
                .ToListAsync();

            foreach (var quest in activeQuests)
            {
                if (quest.MonsterType == monster.type1 || quest.MonsterType == monster.type2)
                {
                    quest.NbMonsterKilled++;
                    result.HasProgress = true;
                    
                    if (quest.NbMonsterKilled >= quest.GoalMonster)
                    {
                        quest.IsCompleted = true;
                        quest.IsActive = false;
                        result.HasCompletion = true;
                        result.CompletionMessages.Add($"Quête complétée: {quest.Title}");
                    }
                    else
                    {
                        result.ProgressMessages.Add($"{quest.Title}: {quest.NbMonsterKilled}/{quest.GoalMonster}");
                    }
                }
            }

            if (result.HasProgress || result.HasCompletion)
            {
                dbContext.SaveChanges();
            }

            return result;
        }

        public async Task<QuestProgressResult> CheckLevelReached(int characterId, int currentLevel, MonsterContext? context = null)
        {
            using var scope = context == null ? _serviceScopeFactory.CreateScope() : null;
            var dbContext = context ?? scope!.ServiceProvider.GetRequiredService<MonsterContext>();
            
            var result = new QuestProgressResult();
            
            var activeQuests = await dbContext.QuestLevel
                .Where(q => q.CharacterId == characterId && q.IsActive && !q.IsCompleted)
                .ToListAsync();

            foreach (var quest in activeQuests)
            {
                if (currentLevel >= quest.GoalLevel)
                {
                    quest.IsCompleted = true;
                    quest.IsActive = false;
                    result.HasCompletion = true;
                    result.CompletionMessages.Add($"Quête complétée: {quest.Title}");
                }
            }

            if (result.HasCompletion)
            {
                dbContext.SaveChanges();
            }

            return result;
        }

        public async Task<DTO.Quests> GetActiveQuestsForCharacter(int characterId, MonsterContext? context = null)
        {
            using var scope = context == null ? _serviceScopeFactory.CreateScope() : null;
            var dbContext = context ?? scope!.ServiceProvider.GetRequiredService<MonsterContext>();
            
            var quests = new DTO.Quests
            {
                questTiles = await dbContext.QuestTiles
                    .Where(q => q.CharacterId == characterId && q.IsActive && !q.IsCompleted)
                    .ToListAsync(),
                questMonsters = await dbContext.QuestMonster
                    .Where(q => q.CharacterId == characterId && q.IsActive && !q.IsCompleted)
                    .ToListAsync(),
                questLevels = await dbContext.QuestLevel
                    .Where(q => q.CharacterId == characterId && q.IsActive && !q.IsCompleted)
                    .ToListAsync()
            };

            return quests;
        }

        public void Dispose()
        {
            _questGenerationTimer?.Dispose();
        }
    }
}

