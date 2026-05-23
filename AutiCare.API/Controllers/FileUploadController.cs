using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutiCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FileUploadController : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file provided.");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("File size exceeds the 5MB limit.");

            var ext = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".pdf" };

            if (!allowedExtensions.Contains(ext))
                return BadRequest("Invalid file type. Only PNG, JPG, and PDF are allowed.");

            var fileName = Guid.NewGuid() + ext;
            var path = Path.Combine("wwwroot/uploads", fileName);

            Directory.CreateDirectory("wwwroot/uploads");

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/{fileName}";

            return Ok(new { url });
        }
    }
}
