using API_Pokemon.Data.Context;
using API_Pokemon.Models;
using Microsoft.AspNetCore.Mvc;
using static API_Pokemon.Models.DTO;

namespace API_Pokemon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TuilesController : ControllerBase
    {
        private readonly MonsterContext _context;
        private readonly TuileService _tuileService;
        private readonly GrilleService _grilleService;

        public TuilesController(MonsterContext context, TuileService tuileService, GrilleService grilleService)
        {
            _context = context;
            _tuileService = tuileService;
            _grilleService = grilleService;
        }


        // GET: api/Tuiles/5/5
        [HttpGet("{x:int}/{y:int}")]
        public ActionResult<Tuile> GetTuile(int x, int y)
        {
            try
            {
                var tuile = _tuileService.GetOrCreateTuile(x, y);
                return Ok(tuile);
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest("Coordonnées hors limites (0 à 49)");
            }
        }

        // GET api/Tuiles/Grille/10/15
        [HttpGet("Grille/{x}/{y}")]
        public ActionResult<GrilleJeuDto> GetGrilleAutour(int x, int y)
        {
            var grille = _grilleService.GenererGrilleAutour(x, y);
            return Ok(grille);
        }

        // DELETE: api/Tuiles/clear
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearMap()
        {
            _context.Tuiles.RemoveRange(_context.Tuiles);
            _context.InstanceMonstres.RemoveRange(_context.InstanceMonstres);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Map cleared successfully" });
        }
    }
}
