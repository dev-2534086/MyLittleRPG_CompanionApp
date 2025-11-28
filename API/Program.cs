using API_Pokemon.Data.Context;
using API_Pokemon.Services;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddSingleton<QuestService>();

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

    if (!AppDomain.CurrentDomain.FriendlyName.Contains("testhost"))
    {
        context.InstanceMonstres.RemoveRange(context.InstanceMonstres);
        context.Tuiles.RemoveRange(context.Tuiles);
        context.SaveChanges();
    }    

    monstreService.InitialiserMonstres(300);
}

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

// --- Ajout pour WebApplicationFactory ---
public partial class Program { }
