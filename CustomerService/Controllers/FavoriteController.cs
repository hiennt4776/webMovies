using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
using CustomerService.Service;


namespace CustomerService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]

    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteMovieService _favoriteMovieService;
        private readonly JwtAuthService _jwtAuthService;

        public FavoriteController(IFavoriteMovieService favoriteMovieService, JwtAuthService jwtAuthService)
        {
            _favoriteMovieService = favoriteMovieService;
            _jwtAuthService = jwtAuthService;
        }

        [HttpPost("{movieId}")]
        public async Task<IActionResult> ToggleFavorite(int movieId)
        {
            var isFavorite = await _favoriteMovieService.ToggleFavoriteAsync(movieId);

            return Ok(isFavorite);
        }

        [HttpGet("{movieId}")]
        public async Task<IActionResult> IsFavorite(int movieId)
        {
            var result = await _favoriteMovieService.IsFavoriteAsync(movieId);
            return Ok(result);
        }

    }


}
