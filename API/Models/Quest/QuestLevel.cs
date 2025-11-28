using System.ComponentModel.DataAnnotations;

namespace API_Pokemon.Models.Quest
{
    public class QuestLevel
    {
        [Key]
        public int QuestLeveId { get; set; }
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(150)]
        public string Description { get; set; } = string.Empty;
        public int GoalLevel { get; set; }
        public bool IsActive { get; set; }
        public bool IsCompleted { get; set; }
        public int CharacterId { get; set; }


        public QuestLevel() { }

        public QuestLevel(int characterId)
        {
            Title = string.Empty;
            Description = string.Empty;
            GoalLevel = 0;
            IsActive = true;
            IsCompleted = false;
            CharacterId = characterId;

        }
    }
}
