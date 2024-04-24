using System.Collections.Concurrent;
using CopticChanter.WebApi.ContentSources;
using CopticChanter.WebApi.Core;
using CopticChanter.WebApi.Core.Requests;
using CopticChanter.WebApi.Core.Responses;
using CoptLib;
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
    [HttpPost]
    public async Task<IActionResult> GetLayout(string type, string id, [FromServices] Session session)
    {
        var request = await Helpers.TryDeserializeJson<LayoutRequest>(Request.Body);
        
        var context = session.Context;

        if (request?.Date is not null)
            session.Context.SetDate(LocalDateTime.FromDateTime(request.Date.Value));

        var excludedLanguages = request?.GetValidExcludedLanguages();
        var transliterations = request?.GetValidTransliterations();
        DocLayoutOptions layoutOptions = new(excludedLanguages: excludedLanguages, transliterations: transliterations);
        
        List<List<IDefinition>> table;
        string title;

        type = type.ToUpperInvariant();
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
                    .Select(setId => AvailableContent.OpenSetFolder(setId, _env))
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

        // Add Coptic date to layout
        LanguageInfo dateLanguage = new(KnownLanguage.Coptic);
        Comment dateComment = new(null)
        {
            Language = dateLanguage,
            SourceText = context.CurrentDate.Format(dateLanguage)
        };
        table.Insert(0, [dateComment]);
        
        Layout layout = new(session.Key, title, table);
        var xLayout = layout.ToXml();
        var stream = await xLayout.ToStringAsync();

        session.UpdateLastModified();
        return File(stream, ContentTypes.MIMETYPE_XML);
    }
}