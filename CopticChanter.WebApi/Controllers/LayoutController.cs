using System.Collections.Concurrent;
using System.Xml.Linq;
using CopticChanter.WebApi.ContentSources;
using CopticChanter.WebApi.Core;
using CoptLib.IO;
using CoptLib.Models;
using CoptLib.Models.Sequences;
using CoptLib.ViewModels;
using CoptLib.Writing;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace CopticChanter.WebApi.Controllers;

[Route("/layout")]
public class LayoutController : Controller
{
    private static readonly ConcurrentDictionary<string, ILoadContext> _sessions = new();
    private readonly IWebHostEnvironment _env;

    public LayoutController(IWebHostEnvironment env)
    {
        _env = env;
    }
    
    [Route("{type}/{id}")]
    [HttpGet]
    public async Task<IActionResult> GetLayout(string type, string id, [FromServices] Session session,
        [FromQuery] DateTime? date, [FromQuery(Name = "exclude")] List<string>? excludedLanguageTags)
    {
        var sessionKey = (string)Request.HttpContext.Items["sessionKey"]!;
        var context = session.Context;

        if (date is not null)
            session.Context.SetDate(LocalDateTime.FromDateTime(date.Value));
        var excludedLanguages = excludedLanguageTags?.Select(LanguageInfo.Parse).ToList() ?? [];

        type = type.ToUpperInvariant();

        IActionResult TryGetStream()
        {
            try
            {
                return File(AvailableContent.Open(type, id, _env), "");
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"No {type} with ID '{id}' was found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to load {type} with ID '{id}':\r\n{ex}");
            }
        }

        List<List<IDefinition>> table;
        DocLayoutOptions layoutOptions = new(excludedLanguages: excludedLanguages);
        if (type == "DOC")
        {
            var doc = await session.MergedContent.TryGetDocAsync(id);
            if (doc is null)
            {
                var result = TryGetStream();
                if (result is not FileStreamResult streamResult)
                    return result;
                var stream = streamResult.FileStream;

                doc = context.LoadDoc(stream);
                await session.Content.AddAsync(doc);
            }

            doc.ApplyTransforms();
            var docLayout = new DocLayout(doc, layoutOptions);
            table = docLayout.CreateTable();
        }
        else if (type == "SET")
        {
            var set = await session.MergedContent.TryGetSetAsync(id);
            if (set is null)
            {
                var result = TryGetStream();
                if (result is not FileStreamResult streamResult)
                    return result;
                var stream = streamResult.FileStream;

                using var setArchive = SharpCompress.Archives.Zip.ZipArchive.Open(stream);
                using var setFolder = new OwlCore.Storage.SharpCompress.ReadOnlyArchiveFolder(setArchive, id, id);
                DocSetReader setReader = new(setFolder, context);
                await setReader.ReadDocs();

                set = setReader.Set;
                await session.Content.AddAsync(set);
            }

            DocSetViewModel setVm = new(set)
            {
                LayoutOptions = layoutOptions
            };

            await setVm.CreateTablesAync();
            table = setVm.Tables.SelectMany(t => t).ToList();
        }
        else if (type == "SEQ")
        {
            var seq = await session.MergedContent.TryGetSequenceAsync(id);
            if (seq is null)
            {
                var result = TryGetStream();
                if (result is not FileStreamResult streamResult)
                    return result;
                var stream = streamResult.FileStream;

                var seqXml = await XDocument.LoadAsync(stream, LoadOptions.None, default);
                seq = SequenceReader.ParseSequenceXml(seqXml, context);
                await session.Content.AddAsync(seq);
            }

            var docResolver = SequenceEx.LazyLoadedDocResolverFactory(context,
                AvailableContent.Sets.Keys
                    .Select(setId =>
                    {
                        var setStream = AvailableContent.Open("SET", setId, _env);
                        var setArchive = SharpCompress.Archives.Zip.ZipArchive.Open(setStream);
                        return new OwlCore.Storage.SharpCompress.ReadOnlyArchiveFolder(setArchive, setId, setId);
                    })
                    .ToAsyncEnumerable()
            );
            
            var docs = await seq.EnumerateDocs(docResolver).ToListAsync();
            DocSetViewModel seqVm = new(seq.Name, docs)
            {
                LayoutOptions = layoutOptions
            };

            await seqVm.CreateTablesAync();
            table = seqVm.Tables.SelectMany(t => t).ToList();
        }
        else
        {
            return BadRequest($"Invalid type '{type}'");
        }

        Layout layout = new(sessionKey, table);
        var layoutXml = await layout.ToXmlStringAsync();
        
        return File(layoutXml, "application/xml");
    }
}