using Autonoma_Pokedex.Data;
using Autonoma_Pokedex.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Autonoma_Pokedex.Services
{
    public class PokemonService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;

        public PokemonService(
            HttpClient httpClient,
            ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        // ==========================================
        // CARGAR POKÉMON DESDE POKÉAPI
        // ==========================================

        public async Task CargarPokemons()
        {
            // Obtener la lista de Pokémon
            var response = await _httpClient.GetStringAsync(
                "https://pokeapi.co/api/v2/pokemon?limit=1025"
            );

            var data = JsonConvert.DeserializeObject<PokemonListResponse>(response);

            if (data == null || data.Results == null)
            {
                return;
            }

            // Obtener los Pokémon que ya existen
            var idsExistentes = await _context.Pokemons
                .Select(p => p.Id)
                .ToListAsync();

            // Procesar de 20 en 20
            foreach (var grupo in data.Results.Chunk(20))
            {
                var tareas = grupo.Select(async item =>
                {
                    try
                    {
                        // Obtener detalles
                        var detalleResponse = await _httpClient.GetStringAsync(
                            item.Url
                        );

                        var detalle =
                            JsonConvert.DeserializeObject<PokemonApiDetail>(
                                detalleResponse
                            );

                        if (detalle == null)
                        {
                            return null;
                        }

                        // No insertar si ya existe
                        if (idsExistentes.Contains(detalle.Id))
                        {
                            return null;
                        }

                        // Crear Pokémon
                        var pokemon = new Pokemon
                        {
                            Id = detalle.Id,

                            Name = detalle.Name ?? "Desconocido",

                            Height = detalle.Height,

                            Weight = detalle.Weight,

                            // Imagen basada directamente en el ID
                            Image = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{detalle.Id}.png",

                            // Tipo
                            Type = detalle.Types != null &&
                                   detalle.Types.Count > 0 &&
                                   detalle.Types[0].Type != null
                                ? detalle.Types[0].Type.Name
                                : "Desconocido",

                            // Habilidad
                            Ability = detalle.Abilities != null &&
                                      detalle.Abilities.Count > 0 &&
                                      detalle.Abilities[0].Ability != null
                                ? detalle.Abilities[0].Ability.Name
                                : "Desconocida"
                        };

                        return pokemon;
                    }
                    catch
                    {
                        // Si falla un Pokémon, continuar con el siguiente
                        return null;
                    }
                });

                // Esperar las consultas del grupo
                var resultados = await Task.WhenAll(tareas);

                // Agregar los Pokémon encontrados
                foreach (var pokemon in resultados)
                {
                    if (pokemon != null)
                    {
                        _context.Pokemons.Add(pokemon);

                        idsExistentes.Add(pokemon.Id);
                    }
                }

                // Guardar el grupo
                await _context.SaveChangesAsync();
            }
        }


        // ==========================================
        // OBTENER POKÉMON DESDE SQL SERVER
        // ==========================================

        public async Task<List<Pokemon>> GetPokemons()
        {
            return await _context.Pokemons
                .OrderBy(p => p.Id)
                .ToListAsync();
        }
    }


    // ==========================================
    // RESPUESTA DE POKÉAPI
    // ==========================================

    public class PokemonListResponse
    {
        public List<PokemonListItem> Results { get; set; }
    }


    public class PokemonListItem
    {
        public string Name { get; set; }

        public string Url { get; set; }
    }


    // ==========================================
    // DETALLE DEL POKÉMON
    // ==========================================

    public class PokemonApiDetail
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Height { get; set; }

        public int Weight { get; set; }

        public PokemonSprites Sprites { get; set; }

        public List<PokemonType> Types { get; set; }

        public List<PokemonAbility> Abilities { get; set; }
    }


    // ==========================================
    // IMÁGENES
    // ==========================================

    public class PokemonSprites
    {
        public string FrontDefault { get; set; }
    }


    // ==========================================
    // TIPOS
    // ==========================================

    public class PokemonType
    {
        public PokemonTypeInfo Type { get; set; }
    }


    public class PokemonTypeInfo
    {
        public string Name { get; set; }
    }


    // ==========================================
    // HABILIDADES
    // ==========================================

    public class PokemonAbility
    {
        public PokemonAbilityInfo Ability { get; set; }
    }


    public class PokemonAbilityInfo
    {
        public string Name { get; set; }
    }
}