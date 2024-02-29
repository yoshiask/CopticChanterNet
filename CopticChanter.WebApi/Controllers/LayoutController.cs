using System.Collections.Concurrent;
using System.Text;
using System.Xml;
using System.Xml.Linq;
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
    public async Task<IActionResult> GetLayout(string type, string id, [FromQuery] string? sessionKey,
        [FromForm] Options? options)
    {
        sessionKey ??= Guid.NewGuid().ToString("N").ToUpper();
        
        var context = _sessions.GetOrAdd(sessionKey, _ => new LoadContext());
        if (options?.Date is not null)
            context.SetDate(LocalDateTime.FromDateTime(options.Date.Value));

        type = type.ToUpperInvariant();
        Stream stream;
        try
        {
            stream = AvailableContent.Open(type, id, _env);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"No {type} with ID '{id}' was found");
        }

        List<List<IDefinition>> table;
        DocLayoutOptions layoutOptions = new(excludedLanguages: options?.ExcludedLanguages);
        if (type == "DOC")
        {
            var doc = context.LookupDefinition(id) as Doc
                ?? context.LoadDoc(stream);
            
            var docLayout = new DocLayout(doc, layoutOptions);
            table = docLayout.CreateTable();
        }
        else if (type == "SET")
        {
            using var setArchive = SharpCompress.Archives.Zip.ZipArchive.Open(stream);
            using var setFolder = new OwlCore.Storage.SharpCompress.ReadOnlyArchiveFolder(setArchive, id, id);
            DocSetReader setReader = new(setFolder, context);
            await setReader.ReadDocs();
            
            var set = setReader.Set!;
            DocSetViewModel setVm = new(set)
            {
                LayoutOptions = layoutOptions
            };

            await setVm.CreateTablesAync();
            table = setVm.Tables.SelectMany(t => t).ToList();
        }
        else if (type == "SEQ")
        {
            var seqXml = await XDocument.LoadAsync(stream, LoadOptions.None, default);
            var seq = SequenceReader.ParseSequenceXml(seqXml, context);

            var docResolver = SequenceEx.LazyLoadedDocResolverFactory(context,
                AvailableContent.Sets.Keys
                    .Select(setId =>
                    {
                        var setStream = AvailableContent.Open("SET", setId, _env);
                        var setArchive = SharpCompress.Archives.Zip.ZipArchive.Open(setStream);
                        return new OwlCore.Storage.SharpCompress.ReadOnlyArchiveFolder(setArchive, setId, setId);
                    }).ToAsyncEnumerable()
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

        XElement xResponse = new("Layout");
        xResponse.SetAttributeValue("Session", sessionKey);
        foreach (var row in table)
        {
            XElement xRow = new("Row");
            foreach (var def in row)
            {
                var cell = DocWriter.SerializeTransformedObject(def);
                xRow.Add(cell);
            }
            xResponse.Add(xRow);
        }
        
        XDocument xDoc = new();
        xDoc.Add(xResponse);

        StringBuilder sb = new();
        await using var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings
        {
            Async = true,
        });
        await xDoc.WriteToAsync(xmlWriter, default);
        await xmlWriter.FlushAsync();
        
        return Content(sb.ToString(), "application/xml");
        //return Ok(new { Key = sessionKey, Table = table });
    }

    public record Options(DateTime? Date, List<string>? ExcludedLanguageTags)
    {
        public IEnumerable<LanguageInfo> ExcludedLanguages = ExcludedLanguageTags?.Select(LanguageInfo.Parse) ?? [];
    }
}