using Microsoft.EntityFrameworkCore;
using Autonoma_Pokedex.Models;

namespace Autonoma_Pokedex.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Pokemon> Pokemons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // El ID viene directamente de PokéAPI
            // y no debe ser generado automáticamente por SQL Server.
            modelBuilder.Entity<Pokemon>()
                .Property(p => p.Id)
                .ValueGeneratedNever();
        }
    }
}