using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace AirwayAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MassMailerFileUploadController : ControllerBase
    {
        [HttpGet("{username}")]
        public IActionResult ClearFolder(string username)
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", "MassMailerAttachment", username.Trim().ToLower());
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                DirectoryInfo di = new(path);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                return NoContent();
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest();
            }
        }

        [HttpPost, DisableRequestSizeLimit]
        public IActionResult Upload()
        {
            try
            {
                var files = Request.Form.Files;
                List<string> fileNames = new();
                var folderName = Path.Combine("Files", "MassMailerAttachment", Request.Form["username"].ToString().Trim().ToLower());
                var path = Path.Combine(Directory.GetCurrentDirectory(), folderName);
               
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                for (int i = 0; i < files.Count; ++i)
                {
                    if (files[i].Length > 0)
                    {
                        var contentDisposition = ContentDispositionHeaderValue.Parse(files[i].ContentDisposition);
                        var fileName = contentDisposition.FileName?.Trim('"');

                        if (string.IsNullOrEmpty(fileName))
                        {
                            return BadRequest("File name is invalid.");
                        }

                        var fullPath = Path.Combine(path, fileName);
                        fileNames.Add(fileName);

                        using var stream = new FileStream(fullPath, FileMode.Create);
                        files[i].CopyTo(stream);
                    }
                    else
                    {
                        return BadRequest("File is empty.");
                    }

                }
                return Ok(fileNames.ToArray());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
    }
}