using CopticChanter.WebApi.Core;
using CopticChanter.WebApi.Core.Responses;
using CoptLib.Writing;
using CoptLib.Writing.Lexicon;
using Microsoft.AspNetCore.Mvc;
using OwlCore.ComponentModel;

namespace CopticChanter.WebApi.Controllers;

[Route("/lexicon")]
[ApiController]
public class LexiconController(ILexicon _lexicon) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery(Name = "q")] string query, [FromQuery(Name = "usage")] string usageStr)
    {
        await InitLexiconAsync();

        if (!LanguageInfo.TryParse(usageStr, out var usage))
            return BadRequest($"Invalid usage language '{usageStr}'");

        var entries = await _lexicon.SearchAsync(query, usage).ToListAsync();

        LexiconSearchResponse response = new(query, entries);
        var xResponse = response.ToXml();
        var stream = await xResponse.ToStringAsync();
        
        return File(stream, ContentTypes.MIMETYPE_XML);
    }

    [Route("{id}")]
    [HttpGet]
    public async Task<IActionResult> Entry(string id)
    {
        await InitLexiconAsync();

        try
        {
            var entry = await _lexicon.GetEntryAsync(id) as LexiconEntry;
            if (entry is null)
                return NotFound("Super entries are not supported.");

            var xEntry = entry.ToXml();
            var stream = await xEntry.ToStringAsync();
            return File(stream, ContentTypes.MIMETYPE_XML);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"No entry with ID '{id}' was found in the lexicon.");
        }
    }
    
    private async Task InitLexiconAsync()
    {
        if (_lexicon is IAsyncInit l)
            await l.InitAsync();
    }
}
