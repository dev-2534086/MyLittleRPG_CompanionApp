using System.ComponentModel.DataAnnotations;

namespace API_Pokemon.Models.Quest
{
    public class QuestMonster
    {
        [Key]
        public int QuestMonsterId { get; set; }
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(150)]
        public string Description { get; set; } = string.Empty;
        [MaxLength(50)]
        public string MonsterType { get; set; } = string.Empty;
        public int NbMonsterKilled { get; set; } 
        public int GoalMonster {  get; set; }
        public bool IsActive { get; set; }
        public bool IsCompleted { get; set; }
        public int CharacterId { get; set; }


        public QuestMonster() { }

        public QuestMonster(int characterId)
        {
            Title = string.Empty;
            Description = string.Empty;
            MonsterType = string.Empty;
            NbMonsterKilled = 0;
            GoalMonster = 0;
            IsActive = true;
            IsCompleted = false;
            CharacterId = characterId;

        }
    }
}
