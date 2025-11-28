using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Pokemon.Models
{
    public enum TypeTuile
    {
        HERBE,
        EAU,
        MONTAGNE,
        FORET,
        VILLE,
        ROUTE,
    }

    public class Tuile
    {
        [Key, Column(Order = 0)]
        public int PositionX { get; set; }

        [Key, Column(Order = 1)]
        public int PositionY { get; set; }

        public TypeTuile Type { get; set; }

        public bool EstTraversable { get; set; }

        public string ImageUrl { get; set; } = string.Empty;
    }
}
