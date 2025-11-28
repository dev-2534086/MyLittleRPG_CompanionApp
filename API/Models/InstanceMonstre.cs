using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Pokemon.Models
{
    public class InstanceMonstre
    {
        [Key, Column(Order = 0)]
        public int PositionX { get; set; }

        [Key, Column(Order = 1)]
        public int PositionY { get; set; }
        public int MonstreId { get; set; }
        public int Niveau { get; set; }
        public int PointsVieActuels { get; set; }
        public int PointsVieMax { get; set; }
        public int Attaque { get; set; }
        public int Defense { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("MonstreId")]
        public virtual Monster? Monstre { get; set; }

        // Méthodes de calcul des statistiques
        public int CalculerAttaque()
        {
            return Monstre?.forceBase + Niveau ?? 0;
        }

        public int CalculerDefense()
        {
            return Monstre?.defenseBase + Niveau ?? 0;
        }

        public int CalculerExperienceDonnee()
        {
            return Monstre?.experienceBase + (Niveau * 10) ?? 0;
        }
    }
}
