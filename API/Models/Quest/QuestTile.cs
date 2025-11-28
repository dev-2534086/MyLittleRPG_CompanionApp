using System.ComponentModel.DataAnnotations;

namespace API_Pokemon.Models.Quest
{
    public class QuestTile
    {
        [Key]
        public int QuestTileId { get; set; }
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(150)]
        public string Description { get; set; } = string.Empty;
        public int TilePositionX { get; set; }
        public int TilePositionY { get; set; }
        public bool IsActive { get; set; }
        public bool IsCompleted { get; set; }
        public int CharacterId { get; set; }


        public QuestTile() { }

        public QuestTile(int characterId) 
        {
            Title = string.Empty;
            Description = string.Empty;
            TilePositionX = 0;
            TilePositionY = 0;
            IsActive = true;
            IsCompleted = false;
            CharacterId = characterId;

        }
    }
}
