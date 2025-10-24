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
    }
}
