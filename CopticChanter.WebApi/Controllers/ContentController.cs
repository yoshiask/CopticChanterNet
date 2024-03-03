using CopticChanter.WebApi.Core;
using CopticChanter.WebApi.Core.Responses;
using CoptLib.IO;
using CoptLib.Models;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace CopticChanter.WebApi.Controllers;

[Route("/content")]
public class ContentController : Controller
{
    [HttpGet]
    public IActionResult AvailableContent([FromServices] Core.AvailableContent content)
    {
        return Ok(content);
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
        IContextualLoad loadedContent;
        if (type == "DOC")
        {
            var doc = context.LoadDoc(stream);
            loadedContent = doc;
            await session.Content.AddAsync(doc);
        }
        else if (type == "SET")
        {
            using var setArchive = SharpCompress.Archives.Zip.ZipArchive.Open(stream);
            using var setFolder = new OwlCore.Storage.SharpCompress.ReadOnlyArchiveFolder(setArchive, file.FileName, file.Name);
            DocSetReader setReader = new(setFolder, context);
            await setReader.ReadDocs();

            loadedContent = setReader.Set;
            await session.Content.AddAsync(setReader.Set);
        }
        else if (type == "SEQ")
        {
            var seqXml = await XDocument.LoadAsync(stream, LoadOptions.None, default);
            var seq = SequenceReader.ParseSequenceXml(seqXml, session.Context);

            loadedContent = seq;
            await session.Content.AddAsync(seq);
        }
        else
        {
            return BadRequest($"Invalid type '{type}'");
        }

        CustomContentResponse response = new(session.Key, loadedContent.Key!);
        return Ok(response);
    }
}