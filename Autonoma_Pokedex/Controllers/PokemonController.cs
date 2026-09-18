using Autonoma_Pokedex.Services;
using Microsoft.AspNetCore.Mvc;

namespace Autonoma_Pokedex.Controllers
{
    public class PokemonController : Controller
    {
        private readonly PokemonService _pokemonService;

        public PokemonController(PokemonService pokemonService)
        {
            _pokemonService = pokemonService;
        }

        // Lista de Pokémon con paginación
        public async Task<IActionResult> Index(int page = 1)
        {
            int cantidadPorPagina = 20;

            var pokemons = await _pokemonService.GetPokemons();

            int totalPokemons = pokemons.Count;

            var pokemonsPagina = pokemons
                .Skip((page - 1) * cantidadPorPagina)
                .Take(cantidadPorPagina)
                .ToList();

            ViewBag.PaginaActual = page;
            ViewBag.TotalPaginas =
                (int)Math.Ceiling((double)totalPokemons / cantidadPorPagina);

            return View(pokemonsPagina);
        }

        // Cargar los Pokémon desde la API
        public async Task<IActionResult> Cargar()
        {
            await _pokemonService.CargarPokemons();

            return RedirectToAction("Index");
        }

        // Detalles de un Pokémon
        public async Task<IActionResult> Details(int id)
        {
            var pokemons = await _pokemonService.GetPokemons();

            var pokemon = pokemons.FirstOrDefault(p => p.Id == id);

            return View(pokemon);
        }
    }
}