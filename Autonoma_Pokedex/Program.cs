using Autonoma_Pokedex.Data;
using Autonoma_Pokedex.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Agregar MVC
builder.Services.AddControllersWithViews();

// Conexión con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Registrar PokemonService
builder.Services.AddHttpClient<PokemonService>();

var app = builder.Build();

// Configuración del entorno
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// Cargar los Pokémon automáticamente si la base de datos está vacía
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    var pokemonService = scope.ServiceProvider
        .GetRequiredService<PokemonService>();

    if (!context.Pokemons.Any())
    {
        await pokemonService.CargarPokemons();
    }
}

// Página inicial: Pokémon
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pokemon}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();