using AdminService.Service;
using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;


namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovieFileController : ControllerBase
    {
        private readonly IMovieFileService _movieFileService;

        public MovieFileController(IMovieFileService movieFileService)
        {
            _movieFileService = movieFileService;
        }

        [HttpGet("{movieId}")]
        public async Task<IActionResult> GetFiles(int movieId)
        {
            var files = await _movieFileService.GetFilesAsync(movieId);
            return Ok(files);
        }

        // POST api/moviefiles/upload/{movieId}
        [HttpPost("upload/{movieId}")]
        [RequestSizeLimit(1024 * 1024 * 500)]  // 500MB
        [RequestFormLimits(MultipartBodyLengthLimit = 1024 * 1024 * 500)]
        public async Task<IActionResult> Upload(int movieId, [FromForm] IFormFile file, [FromForm] string fileType)
        {
            if (file == null) return BadRequest("No file provided");
            var dto =  await _movieFileService.UploadFileAsync(movieId, file, fileType);
            return Ok(dto);
        }

        // DELETE api/moviefiles/{fileId}
        [HttpDelete("{fileId}")]
        public async Task<IActionResult> Delete(int fileId)
        {
            await _movieFileService.DownloadFileAsync(fileId);
            return NoContent();
        }
    }
}
