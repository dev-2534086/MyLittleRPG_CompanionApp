using API_Pokemon.Data.Config;
using API_Pokemon.Data.Context;
using API_Pokemon.Models;

public interface IMonstreService
{
    List<InstanceMonstre> GetAll();
    InstanceMonstre GetByPosition(int x, int y);
    void SupprimerMonstre(int id);
    void InitialiserMonstres(int nombre);
}

public class MonstreService : IMonstreService
{
    private readonly MonsterContext _context;
    private readonly List<InstanceMonstre> _instances = new();
    private readonly Random _random = new();

    private int _mortCounter = 0;
    private bool _initialized = false; // ✅ évite de régénérer à chaque appel

    public MonstreService(MonsterContext context)
    {
        _context = context;
    }

    public List<InstanceMonstre> GetAll()
    {
        // ✅ Génère 300 monstres au premier appel si ce n’est pas déjà fait
        if (!_initialized)
        {
            InitialiserMonstres(GameConfig.MAX_MONSTRES);
            _initialized = true;
        }

        return _instances;
    }

    public InstanceMonstre GetByPosition(int x, int y)
        => _instances.FirstOrDefault(m => m.PositionX == x && m.PositionY == y);

    public void SupprimerMonstre(int id)
    {
        var monstre = _context.InstanceMonstres.FirstOrDefault(m => m.MonstreId == id);
        if (monstre == null) return;

        _context.InstanceMonstres.Remove(monstre);
        _instances.Remove(monstre);
        _context.SaveChanges();

        _mortCounter++;

        // ✅ Quand 10 monstres meurent, on en régénère 10
        if (_mortCounter >= GameConfig.MONSTRES_REGEN_TAUX)
        {
            InitialiserMonstres(GameConfig.MONSTRES_REGEN_TAUX);
            _mortCounter = 0;
        }
    }

    public void InitialiserMonstres(int nombre)
    {
        var monsters = _context.Monster.ToList();
        if (!monsters.Any())
        {
            Console.WriteLine("[WARN] Aucun monstre disponible dans la DB.");
            return;
        }

        int existingCount = _context.InstanceMonstres.Count();
        if (existingCount >= GameConfig.MAX_MONSTRES)
        {
            Console.WriteLine("[INFO] Nombre maximum de monstres déjà atteint.");
            return;
        }

        int toGenerate = Math.Min(nombre, GameConfig.MAX_MONSTRES - existingCount);

        var usedPositions = _context.InstanceMonstres
            .Select(m => new { m.PositionX, m.PositionY })
            .ToList()
            .Select(p => (p.PositionX, p.PositionY))
            .ToHashSet();

        int generated = 0;
        var newTuiles = new List<Tuile>();
        var newInstances = new List<InstanceMonstre>();

        while (generated < toGenerate)
        {
            int x = _random.Next(0, GameConfig.MAP_SIZE);
            int y = _random.Next(0, GameConfig.MAP_SIZE);

            // Évite la ville principale
            if (x == GameConfig.CENTER_X && y == GameConfig.CENTER_Y)
                continue;

            if (!usedPositions.Add((x, y)))
                continue;

            var tuile = _context.Tuiles.FirstOrDefault(t => t.PositionX == x && t.PositionY == y);
            if (tuile == null)
            {
                var typesTraversables = new[] { TypeTuile.HERBE, TypeTuile.FORET, TypeTuile.ROUTE };
                var typeChoisi = typesTraversables[_random.Next(typesTraversables.Length)];

                tuile = new Tuile
                {
                    PositionX = x,
                    PositionY = y,
                    EstTraversable = true,
                    Type = typeChoisi,
                    ImageUrl = TuileService.TypeTuileImageUrls[typeChoisi]
                };
                newTuiles.Add(tuile);
            }

            var monstreBase = monsters[_random.Next(monsters.Count)];

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

            newInstances.Add(instance);
            _instances.Add(instance);
            generated++;
        }

        if (generated > 0)
        {
            _context.Tuiles.AddRange(newTuiles);
            _context.InstanceMonstres.AddRange(newInstances);
            _context.SaveChanges();

            Console.WriteLine($"[INFO] Génération de {generated} nouveaux monstres.");
        }
        else
        {
            Console.WriteLine("[INFO] Aucun monstre généré (limite atteinte).");
        }
    }

    private int CalculerDistanceVilleLaPlusProche(int x, int y)
    {
        var villes = _context.Tuiles
            .Where(t => t.ImageUrl == "Assets/TypesTuiles/Town.png")
            .Select(t => new { t.PositionX, t.PositionY })
            .ToList();

        if (!villes.Any())
            return 10;

        int distance = villes
            .Select(v => Math.Abs(v.PositionX - x) + Math.Abs(v.PositionY - y))
            .Min();

        return distance;
    }
}
