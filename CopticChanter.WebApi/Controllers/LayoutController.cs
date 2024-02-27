using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using CoptLib.IO;
using CoptLib.Models;
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
        [FromBody] Options? options)
    {
        sessionKey ??= Guid.NewGuid().ToString("N").ToUpper();
        
        var context = _sessions.GetOrAdd(sessionKey, _ => new LoadContext());
        if (options?.Date is not null)
            context.SetDate(LocalDateTime.FromDateTime(options.Date.Value));

        type = type.ToUpperInvariant();
        var stream = new Lazy<Stream>(() => AvailableContent.Open(type, id, _env));

        List<List<IDefinition>> table = new();
        if (type == "DOC")
        {
            var doc = context.LookupDefinition(id) as Doc
                ?? context.LoadDoc(stream.Value);
            
            var docLayout = new DocLayout(doc, new(excludedLanguages: options?.ExcludedLanguages));
            table = docLayout.CreateTable();
        }

        XElement xResponse = new("Layout");
        xResponse.SetAttributeValue("Session", sessionKey);
        foreach (var row in table)
        {
            XElement xRow = new("Row");
            foreach (var def in row)
            {
                var cell = DocWriter.SerializeObject(def);
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

    public record Options(DateTime? Date, List<string> ExcludedLanguageTags)
    {
        public IEnumerable<LanguageInfo> ExcludedLanguages = ExcludedLanguageTags.Select(LanguageInfo.Parse);
    }
}