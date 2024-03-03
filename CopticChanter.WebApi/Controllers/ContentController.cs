using CopticChanter.WebApi.Core;
using CoptLib.IO;
using Microsoft.AspNetCore.Mvc;

namespace CopticChanter.WebApi.Controllers;

[Route("/content")]
public class ContentController : Controller
{
    [HttpGet]
    public IActionResult AvailableContent()
    {
        return Ok(WebApi.AvailableContent.List);
    }

    [HttpPost]
    public async Task<IActionResult> LoadCustomContent([FromForm] IFormFile file, [FromServices] Session session)
    {
        var mime = file.Headers.ContentType.FirstOrDefault();
        if (mime is null)
            return BadRequest("File content type must be specified.");

        if (!ContentTypes.TryMimeToType(mime, out var type))
            return BadRequest($"Invalid content type '{mime}'");

        var context = session.Context;

        var stream = file.OpenReadStream();
        if (type == "DOC")
        {
            _ = context.LoadDoc(stream);
        }
        else if (type == "SET")
        {
            using var setArchive = SharpCompress.Archives.Zip.ZipArchive.Open(stream);
            using var setFolder = new OwlCore.Storage.SharpCompress.ReadOnlyArchiveFolder(setArchive, file.FileName, file.Name);
            DocSetReader setReader = new(setFolder, context);
            await setReader.ReadDocs();
        }
        else if (type == "SEQ")
        {
            return BadRequest("Custom sequences are not supported at this time.");
        }

        return Ok(Request.HttpContext.Items["sessionKey"]);
    }
}