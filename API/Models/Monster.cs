using System.ComponentModel.DataAnnotations;

namespace API_Pokemon.Models
{
    public class Monster
    {
        [Key]
        public int idMonster { get; set; }
        public int pokemonId { get; set; }
        [MaxLength(45)]
        public string nom {  get; set; } = string.Empty;
        public int pointVieBase { get; set; }
        public int forceBase { get; set; }
        public int defenseBase { get; set; }
        public int experienceBase { get; set; }
        [MaxLength(45)]
        public string spriteURL { get; set; } = string.Empty;
        [MaxLength(45)]
        public string type1 { get; set; } = string.Empty;
        [MaxLength(45)]
        public string type2 { get; set; } = string.Empty;
    }
}
