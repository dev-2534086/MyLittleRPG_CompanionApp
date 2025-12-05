using API_Pokemon.Data.Context;
using API_Pokemon.Models;

namespace API_Pokemon.Services
{
    public interface ICombatService
    {
        CombatResult SimulerCombat(Character joueur, InstanceMonstre monstre);
        CombatResult ExecuterCombat(Character joueur, InstanceMonstre monstre);
    }

    public class CombatResult
    {
        public bool VictoireJoueur { get; set; }
        public bool DefaiteJoueur { get; set; }
        public bool CombatIndecis { get; set; }
        public int DegatsJoueurTotal { get; set; }
        public int DegatsMonstreTotal { get; set; }
        public int ExperienceGagnee { get; set; }
        public int NombreTours { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> DetailsTours { get; set; } = new List<string>();
    }

    public class CombatService : ICombatService
    {
        private readonly MonsterContext _context;
        private readonly IMonstreService _monstreService;
        private readonly Random _random = new();

        public CombatService(MonsterContext context, IMonstreService monstreService)
        {
            _context = context;
            _monstreService = monstreService;
        }

        // Calcule les dégâts du joueur et du monstre pour un tour donné
        // Le facteur aléatoire rend les combats moins déterministes
        private (int degatsJoueur, int degatsMonstre, double facteurAleatoire) CalculerDegats(Character joueur, InstanceMonstre monstre)
        {
            double facteurAleatoire = _random.NextDouble() * (1.25 - 0.8) + 0.8; // entre 0.8 et 1.25
            int degatsJoueur = Math.Max(1, (int)((joueur.Attack - monstre.CalculerDefense()) * facteurAleatoire));
            int degatsMonstre = Math.Max(1, (int)((monstre.CalculerAttaque() - joueur.Defense) * facteurAleatoire));
            return (degatsJoueur, degatsMonstre, facteurAleatoire);
        }

        // Simulation du combat (aucune modification dans la base)
        // Sert uniquement à prédire l'issue possible d’un combat
        public CombatResult SimulerCombat(Character joueur, InstanceMonstre monstre)
        {
            var result = new CombatResult();
            int hpJoueur = joueur.Hp;
            int hpMonstre = monstre.PointsVieActuels;
            int tour = 0;

            while (hpJoueur > 0 && hpMonstre > 0)
            {
                tour++;
                var (degatsJoueur, degatsMonstre, _) = CalculerDegats(joueur, monstre);

                // Application des dégâts pour le tour
                hpMonstre = Math.Max(0, hpMonstre - degatsJoueur);
                hpJoueur = Math.Max(0, hpJoueur - degatsMonstre);

                result.DegatsJoueurTotal += degatsJoueur;
                result.DegatsMonstreTotal += degatsMonstre;
                result.DetailsTours.Add($"Tour {tour}: Vous infligez {degatsJoueur} dégâts, le monstre inflige {degatsMonstre} dégâts");
            }

            result.NombreTours = tour;

            // Détermination du vainqueur
            if (hpMonstre <= 0)
            {
                result.VictoireJoueur = true;
                result.ExperienceGagnee = monstre.CalculerExperienceDonnee();
                result.Message = $"Victoire en {tour} tours ! Vous gagnez {result.ExperienceGagnee} XP (simulation).";
            }
            else
            {
                result.DefaiteJoueur = true;
                result.Message = $"Défaite en {tour} tours (simulation).";
            }

            return result;
        }

        // Combat réel : modifie l’état du joueur, du monstre et la base de données
        public CombatResult ExecuterCombat(Character joueur, InstanceMonstre monstre)
        {
            var result = new CombatResult();
            int tour = 0;

            // Récupérer l'utilisateur lié au Character pour son email
            var user = _context.Users.FirstOrDefault(u => u.UserId == joueur.UserId);
            string email = user?.Email ?? string.Empty;

            while (joueur.Hp > 0 && monstre.PointsVieActuels > 0)
            {
                tour++;
                var (degatsJoueur, degatsMonstre, _) = CalculerDegats(joueur, monstre);

                monstre.PointsVieActuels = Math.Max(0, monstre.PointsVieActuels - degatsJoueur);
                joueur.Hp = Math.Max(0, joueur.Hp - degatsMonstre);

                result.DegatsJoueurTotal += degatsJoueur;
                result.DegatsMonstreTotal += degatsMonstre;
                result.DetailsTours.Add($"Tour {tour}: Vous infligez {degatsJoueur} dégâts, le monstre inflige {degatsMonstre} dégâts");
            }

            result.NombreTours = tour;

            if (monstre.PointsVieActuels <= 0)
            {
                result.VictoireJoueur = true;
                result.ExperienceGagnee = monstre.CalculerExperienceDonnee();
                joueur.GagnerExperience(result.ExperienceGagnee);

                // Ajouter au Pokédex via l'email
                try
                {
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        var exists = _context.HuntedMonsters
                            .Any(h => h.PlayerEmail == email && h.MonsterId == monstre.MonstreId);

                        if (!exists)
                        {
                            _context.HuntedMonsters.Add(new HuntedMonster
                            {
                                PlayerEmail = email,
                                MonsterId = monstre.MonstreId,
                                HuntedAt = DateTime.UtcNow
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[WARN] Erreur HuntedMonster: " + ex.Message);
                }

                _monstreService.SupprimerMonstre(monstre.MonstreId);
                result.Message = $"Victoire en {tour} tours ! Vous gagnez {result.ExperienceGagnee} XP.";
            }
            else
            {
                result.DefaiteJoueur = true;
                joueur.TeleporterAVilleDomicile();
                result.Message = $"Défaite en {tour} tours ! Vous êtes téléporté à votre ville domicile.";
            }

            _context.SaveChanges();
            return result;
        }

    }
}
