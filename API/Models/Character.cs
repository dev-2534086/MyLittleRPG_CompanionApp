using System.ComponentModel.DataAnnotations;

namespace API_Pokemon.Models
{
    public class Character
    {
        [Key]
        public int CharacterId {  get; set; }
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Xp { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int UserId { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public int VilleDomicileX { get; set; } = 10;
        public int VilleDomicileY { get; set; } = 10;

        public Character()
        {

        }

        public Character(string name, int userId)
        {
            Name = name;
            Level = 1;
            Xp = 0;
            Hp = 100;
            MaxHp = 100;
            Attack = 10;
            Defense = 5;
            PositionX = 10;
            PositionY = 10;
            UserId = userId; 
            CreationDate = DateTime.Now;
            VilleDomicileX = 10;
            VilleDomicileY = 10;
        }

        // Méthodes pour le système de combat et de niveaux
        public void GagnerExperience(int experience)
        {
            Xp += experience;
            VerifierNiveau();
        }

        private void VerifierNiveau()
        {
            int xpRequis = Level * 100;
            if (Xp >= xpRequis)
            {
                MonterNiveau();
            }
        }

        private void MonterNiveau()
        {
            Level++;
            Attack++;
            Defense++;
            MaxHp += 10;
            Hp = MaxHp;
            Xp = 0;
        }

        public void RestaurerVie()
        {
            Hp = MaxHp;
        }

        public void TeleporterAVilleDomicile()
        {
            PositionX = VilleDomicileX;
            PositionY = VilleDomicileY;
            RestaurerVie();
        }

        public void DefinirVilleDomicile(int x, int y)
        {
            VilleDomicileX = x;
            VilleDomicileY = y;
        }
    }
}
