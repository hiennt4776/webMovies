using AdminService.Service;
using helperMovies.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovieController : Controller
    {


        private readonly IMovieService _movieService;
        private readonly JwtAuthService _jwtAuthService;



        public MovieController(IMovieService movieService, JwtAuthService jwtAuthService)
        {
            _movieService = movieService;
            _jwtAuthService = jwtAuthService;
        }


        [HttpGet("getPageSearchMovies")]
       
        public async Task<ActionResult<PagedResult<MovieDTO>>> Get([FromQuery] MovieSearchDTO movieSearch)
        {
            var res = await _movieService.GetMoviesAsync(movieSearch.Keyword, movieSearch.PageNumber, movieSearch.PageSize);
            return Ok(res);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MovieDTO>> GetById(int id)
        {
            var m = await _movieService.GetMovieByIdAsync(id);
            if (m == null) return NotFound();
            return Ok(m);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MovieDTO dto)
        {
            await _movieService.AddMovieAsync(dto);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MovieDTO dto)
        {
            dto.Id = id;
            await _movieService.UpdateMovieAsync(dto);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _movieService.DeleteMovieAsync(id);
            return Ok();
        }
    }
}
