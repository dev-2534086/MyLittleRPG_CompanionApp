using API_Pokemon.Data.Context;
using API_Pokemon.Services;
using Microsoft.EntityFrameworkCore;

namespace API_Pokemon
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configuration CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowAnyOrigin();
                });
            });

            // Configuration du contexte de base de données
            builder.Services.AddDbContext<MonsterContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("Default");
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });

            // Injection des services
            builder.Services.AddScoped<TuileService>();
            builder.Services.AddScoped<GrilleService>();
            builder.Services.AddScoped<IMonstreService, MonstreService>();
            builder.Services.AddScoped<ICombatService, CombatService>();

            // Configuration JSON
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition =
                        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.ReferenceHandler =
                        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // --- INITIALISATION DE LA MAP ET DES MONSTRES ---
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MonsterContext>();
                var monstreService = scope.ServiceProvider.GetRequiredService<IMonstreService>();

                Console.WriteLine("[INFO] --- Initialisation de la carte et des monstres ---");

                // Suppression sécurisée de tous les anciens éléments
                context.InstanceMonstres.RemoveRange(context.InstanceMonstres);
                context.Tuiles.RemoveRange(context.Tuiles);
                context.SaveChanges();

                Console.WriteLine("[INFO] Carte nettoyée. Génération de nouveaux monstres...");

                // Génération initiale de 300 monstres
                monstreService.InitialiserMonstres(300);

                Console.WriteLine("[INFO] Génération complète : 300 monstres créés.");
            }

            // --- Configuration du pipeline HTTP ---
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("CorsPolicy");
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
