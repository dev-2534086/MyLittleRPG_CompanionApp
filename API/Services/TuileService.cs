using API_Pokemon.Data.Context;
using API_Pokemon.Models;

public class TuileService
{
    private Random _random = new Random();
    private const string dir = "Assets/TypesTuiles/";

    public static readonly Dictionary<TypeTuile, string> TypeTuileImageUrls = new()
    {
        { TypeTuile.HERBE,     $"{dir}Plains.png" },
        { TypeTuile.EAU,       $"{dir}River.png" },
        { TypeTuile.MONTAGNE,  $"{dir}Mountain.png" },
        { TypeTuile.FORET,     $"{dir}Forest.png" },
        { TypeTuile.VILLE,     $"{dir}Town.png" },
        { TypeTuile.ROUTE,     $"{dir}Road.png" }
    };

    private readonly MonsterContext _db;

    public TuileService(MonsterContext db)
    {
        _db = db;
    }

    public Tuile GetOrCreateTuile(int x, int y)
    {
        if (x < 0 || x >= 50 || y < 0 || y >= 50)
            throw new ArgumentOutOfRangeException("Coordonnées hors du monde 50x50");

        var tuile = _db.Tuiles.Find(x, y);
        if (tuile != null)
            return tuile;

        tuile = GenererTuile(x, y);
        _db.Tuiles.Add(tuile);
        _db.SaveChanges();

        if (!(x == 10 && y == 10) && tuile.EstTraversable && tuile.Type != TypeTuile.VILLE && _random.Next(100) < 10)
        {
            GenererMonstreSurTuile(x, y);
        }

        return tuile;
    }

    private Tuile GenererTuile(int x, int y)
    {
        var proba = new List<(TypeTuile type, int proba, bool traversable)>
        {
            (TypeTuile.HERBE, 20, true),
            (TypeTuile.EAU, 10, false),
            (TypeTuile.MONTAGNE, 15, false),
            (TypeTuile.FORET, 15, true),
            (TypeTuile.VILLE, 5, true),
            (TypeTuile.ROUTE, 35, true)
        };

        var adjacents = GetTypesAdjacents(x, y);
        foreach (var i in Enumerable.Range(0, proba.Count))
        {
            if (adjacents.Contains(proba[i].type))
                proba[i] = (proba[i].type, proba[i].proba + 15, proba[i].traversable);
        }

        int total = proba.Sum(p => p.proba);
        int roll = _random.Next(total);
        int cumulative = 0;

        foreach (var (type, chance, traversable) in proba)
        {
            cumulative += chance;
            if (roll < cumulative)
            {
                return new Tuile
                {
                    PositionX = x,
                    PositionY = y,
                    Type = type,
                    EstTraversable = traversable,
                    ImageUrl = TypeTuileImageUrls[type]
                };
            }
        }

        return new Tuile
        {
            PositionX = x,
            PositionY = y,
            Type = TypeTuile.HERBE,
            EstTraversable = true,
            ImageUrl = TypeTuileImageUrls[TypeTuile.HERBE]
        };
    }

    private void GenererMonstreSurTuile(int x, int y)
    {
        // Safety: block spawning on starting tile
        if (x == 10 && y == 10) return;

        // Vérifier qu'il n'y a pas déjà un monstre sur cette tuile
        if (_db.InstanceMonstres.Any(m => m.PositionX == x && m.PositionY == y))
            return;

        // Récupérer un monstre aléatoire de la base de données
        var monsters = _db.Monster.ToList();
        if (!monsters.Any()) return;

        var monstreBase = monsters[_random.Next(monsters.Count)];
        
        // Calculer le niveau basé sur la distance à la ville la plus proche
        int distanceVille = CalculerDistanceVilleLaPlusProche(x, y);
        int niveau = Math.Max(1, distanceVille);

        var instance = new InstanceMonstre
        {
            PositionX = x,
            PositionY = y,
            MonstreId = monstreBase.idMonster,
            Niveau = niveau,
            PointsVieMax = monstreBase.pointVieBase + niveau,
            PointsVieActuels = monstreBase.pointVieBase + niveau,
            Attaque = monstreBase.forceBase + niveau,
            Defense = monstreBase.defenseBase + niveau
        };

        _db.InstanceMonstres.Add(instance);
        _db.SaveChanges();
        
    }

    private int CalculerDistanceVilleLaPlusProche(int x, int y)
    {
        var villes = _db.Tuiles
            .Where(t => t.Type == TypeTuile.VILLE)
            .Select(t => new { t.PositionX, t.PositionY })
            .ToList();

        if (!villes.Any())
            return 1;

        int distance = villes
            .Select(v => Math.Abs(v.PositionX - x) + Math.Abs(v.PositionY - y))
            .Min();

        return distance;
    }

    private List<TypeTuile> GetTypesAdjacents(int x, int y)
    {
        var voisins = new[]
        {
            (x-1, y), (x+1, y), (x, y-1), (x, y+1)
        };

        return voisins
            .Where(v => v.Item1 >= 0 && v.Item1 < 50 && v.Item2 >= 0 && v.Item2 < 50)
            .Select(v => _db.Tuiles.Find(v.Item1, v.Item2))
            .Where(t => t != null)
            .Select(t => t.Type)
            .ToList();
    }
}
