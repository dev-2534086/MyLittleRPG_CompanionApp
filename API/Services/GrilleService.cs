using API_Pokemon.Data.Config;
using static API_Pokemon.Models.DTO;

public class GrilleService
{
    private readonly TuileService _tuileService;

    public GrilleService(TuileService tuileService)
    {
        _tuileService = tuileService;
    }

    public GrilleJeuDto GenererGrilleAutour(int posX, int posY)
    {
        var tuilesDto = new List<TuileAvecInfosDto>();

        for (int x = posX - 1; x <= posX + 1; x++)
        {
            for (int y = posY - 1; y <= posY + 1; y++)
            {
                // En dehors des limites de la carte
                if (x < 0 || x >= GameConfig.MAP_SIZE || y < 0 || y >= GameConfig.MAP_SIZE)
                {
                    tuilesDto.Add(new TuileAvecInfosDto
                    {
                        X = x,
                        Y = y,
                        TypeTuile = "INCONNU",
                        Monstre = null,
                        EstAccessible = false
                    });
                    continue;
                }

                // Récupère (ou crée) la tuile à cette position
                var tuile = _tuileService.GetOrCreateTuile(x, y);

                // Ajoute la tuile (sans monstre)
                tuilesDto.Add(new TuileAvecInfosDto
                {
                    X = x,
                    Y = y,
                    TypeTuile = tuile.Type.ToString(),
                    Monstre = null, // aucun monstre ici
                    EstAccessible = tuile.EstTraversable
                });
            }
        }

        return new GrilleJeuDto { Tuiles = tuilesDto };
    }
}
