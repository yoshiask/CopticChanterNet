using System.Collections.Concurrent;
using CopticChanter.WebApi.ContentSources;
using CopticChanter.WebApi.Core.Responses;
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
        var context = session.Context;

        if (date is not null)
            session.Context.SetDate(LocalDateTime.FromDateTime(date.Value));

        var excludedLanguages = excludedLanguageTags?
            .Select(p => p.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .SelectMany(p => p)
            .Select(LanguageInfo.Parse)
            .ToList() ?? [];

        type = type.ToUpperInvariant();

        List<List<IDefinition>> table;
        string title;

        DocLayoutOptions layoutOptions = new(excludedLanguages: excludedLanguages);
        if (type == "DOC")
        {
            var doc = await session.MergedContent.TryGetDocAsync(id);
            if (doc is null)
                return NotFound($"No doc with ID '{id}' was found");

            doc.ApplyTransforms();
            var docLayout = new DocLayout(doc, layoutOptions);
            table = docLayout.CreateTable();
            title = doc.Name;
        }
        else if (type == "SET")
        {
            var set = await session.MergedContent.TryGetSetAsync(id);
            if (set is null)
                return NotFound($"No set with ID '{id}' was found");

            DocSetViewModel setVm = new(set)
            {
                LayoutOptions = layoutOptions
            };

            await setVm.CreateTablesAync();
            table = setVm.Tables.SelectMany(t => t).ToList();
            title = set.Name;
        }
        else if (type == "SEQ")
        {
            var seq = await session.MergedContent.TryGetSequenceAsync(id);
            if (seq is null)
                return NotFound($"No sequence with ID '{id}' was found");

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
            title = seq.Name ?? id;
        }
        else
        {
            return BadRequest($"Invalid type '{type}'");
        }

        Layout layout = new(session.Key, title, table);
        var layoutXml = await layout.ToXmlStringAsync();

        session.UpdateLastModified();
        return File(layoutXml, "application/xml");
    }
}