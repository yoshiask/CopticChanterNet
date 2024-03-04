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
        var stream = await LexiconSearchResponseReaderWriter.ToXmlStringAsync(response);
        
        return File(stream, "application/xml");
    }

    private async Task InitLexiconAsync()
    {
        if (_lexicon is IAsyncInit l)
            await l.InitAsync();
    }
}
