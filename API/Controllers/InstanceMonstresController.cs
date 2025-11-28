using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_Pokemon.Data.Context;
using API_Pokemon.Models;

namespace API_Pokemon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstanceMonstresController : ControllerBase
    {
        private readonly MonsterContext _context;

        public InstanceMonstresController(MonsterContext context)
        {
            _context = context;
        }

        // GET: api/InstanceMonstres/10/10
        [HttpGet("{posX}/{posY}")]
        public async Task<ActionResult<InstanceMonstre>> GetInstanceMonstre(int posX, int posY)
        {
            var instanceMonstre = await _context.InstanceMonstres
                .FirstOrDefaultAsync(m => m.PositionX == posX && m.PositionY == posY);

            if (instanceMonstre == null)
                return NotFound();

            return instanceMonstre;
        }
    }
}
