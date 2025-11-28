namespace API_Pokemon.Models
{
    public class DTO
    {
        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class RegisterRequest 
        {
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;

        }

        public class LogoutRequest 
        {
            public string Email { get; set; } = string.Empty;
        }

        public class SimulateCombatRequest
        {
            public string Email { get; set; } = string.Empty;
            public int MonsterX { get; set; }
            public int MonsterY { get; set; }
        }

        public class InstanceMonstreDto
        {
            public int Id { get; set; }
            public int MonstreId { get; set; }
            public string SpriteUrl { get; set; }
            public int Niveau { get; set; }

            // Statistiques calculées du monstre
            public int PointsVieActuels { get; set; }
            public int PointsVieMax { get; set; }
            public int Attaque { get; set; }
        }

        // DTO pour une tuile avec informations complètes (incluant monstre s'il y en a un)
        public class TuileAvecInfosDto
        {
            public int X { get; set; }
            public int Y { get; set; }
            public string TypeTuile { get; set; }

            // Monstre présent sur la tuile (null si aucun)
            public InstanceMonstreDto Monstre { get; set; }

            // Indique si le joueur peut se déplacer sur cette tuile
            public bool EstAccessible { get; set; }
        }

        // DTO pour une grille 3x3 autour du joueur (comme demandé précédemment)
        public class GrilleJeuDto
        {
            public List<TuileAvecInfosDto> Tuiles { get; set; }
        }
    }
}
