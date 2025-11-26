using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
using CustomerService.Service;


namespace CustomerService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   
    public class MovieController : ControllerBase
    {
        private readonly IMovieService _movieService;
        public MovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }



        [HttpGet("random-top5")]
        public async Task<ActionResult<List<MovieDTO>>> GetRandomTop5()
        {
            //var baseUrl = $"{Request.Scheme}://{Request.Host}";
            return Ok(await _movieService.GetRandomTop5FilmsAsync());
        }

        [HttpGet("test-file")]
        public IActionResult TestFile()
        {
            string path = @"E:\File\movies\M5\poster5.png";
            if (!System.IO.File.Exists(path))
                return NotFound("File not found");

            var bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "image/png");
        }

        [HttpGet]
        public async Task<IActionResult> GetMovies()
        {
            var movies = await _movieService.GetMoviesAsync();
            return Ok(movies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovie(int id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null) return NotFound();
            return Ok(movie);
        }

        [HttpGet("{id}/file/{fileType}")]
        public async Task<IActionResult> GetMovieFile(int id, string fileType)
        {
            var bytes = await _movieService.GetMovieFileAsync(id, fileType);
            if (bytes == null) return NotFound();

            var contentType = fileType.ToLower() switch
            {
                "poster" => "image/png",
                "trailer" => "video/mp4",
                "movies" => "video/mp4",
                "subtitle" => "text/vtt",
                _ => "application/octet-stream"
            };

            return File(bytes, contentType);
        }
    }
}
