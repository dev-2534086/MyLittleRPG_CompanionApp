namespace API_Pokemon.Models
{
    public class HuntedMonster
    {
        public long Id { get; set; }
        public string? PlayerEmail { get; set; }
        public int MonsterId { get; set; }
        public DateTime HuntedAt { get; set; } = DateTime.UtcNow;
    }
}
