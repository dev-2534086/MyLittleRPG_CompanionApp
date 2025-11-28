using API_Pokemon.Data.Context;
using API_Pokemon.Models;
using Microsoft.EntityFrameworkCore;
using static API_Pokemon.Models.DTO;
using API_Pokemon.Data.Config;

public class GrilleService
{
    private readonly MonsterContext _context;
    private readonly TuileService _tuileService;

    public GrilleService(MonsterContext context, TuileService tuileService)
    {
        _context = context;
        _tuileService = tuileService;
    }

    public GrilleJeuDto GenererGrilleAutour(int posX, int posY)
    {
        var tuilesDto = new List<TuileAvecInfosDto>();

        for (int x = posX - 1; x <= posX + 1; x++)
        {
            for (int y = posY - 1; y <= posY + 1; y++)
            {
                tuilesDto.Add(CreerTuileAvecInfos(x, y));
            }
        }

        return new GrilleJeuDto { Tuiles = tuilesDto };
    }

    private TuileAvecInfosDto CreerTuileAvecInfos(int x, int y)
    {
        // Hors limites selon GameConfig
        if (x < 0 || x >= GameConfig.MAP_SIZE || y < 0 || y >= GameConfig.MAP_SIZE)
        {
            return new TuileAvecInfosDto
            {
                X = x,
                Y = y,
                TypeTuile = "INCONNU",
                Monstre = null,
                EstAccessible = false
            };
        }

        var tuile = _tuileService.GetOrCreateTuile(x, y);
        var monstreDto = RecupererMonstreDto(x, y);

        return new TuileAvecInfosDto
        {
            X = x,
            Y = y,
            TypeTuile = tuile.Type.ToString(),
            Monstre = monstreDto,
            EstAccessible = tuile.EstTraversable
        };
    }

    private InstanceMonstreDto RecupererMonstreDto(int x, int y)
    {
        var monstreInstance = _context.InstanceMonstres
            .AsNoTracking()
            .Include(m => m.Monstre)
            .FirstOrDefault(m => m.PositionX == x && m.PositionY == y);

        if (monstreInstance == null || monstreInstance.Monstre == null)
            return null;

        return new InstanceMonstreDto
        {
            Id = monstreInstance.MonstreId,
            MonstreId = monstreInstance.MonstreId,
            SpriteUrl = monstreInstance.Monstre.spriteURL,
            Niveau = monstreInstance.Niveau,
            PointsVieMax = monstreInstance.PointsVieMax,
            PointsVieActuels = monstreInstance.PointsVieActuels,
            Attaque = monstreInstance.Attaque,
        };
    }
}
