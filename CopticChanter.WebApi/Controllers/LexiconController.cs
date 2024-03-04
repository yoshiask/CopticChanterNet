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
    public async Task<IActionResult> Search([FromQuery(Name = "q")] string query)
    {
        await InitLexiconAsync();

        var results = await _lexicon
            .SearchAsync(query, new LanguageInfo(KnownLanguage.CopticBohairic))
            .ToListAsync();
        
        return Ok(results);
    }

    private async Task InitLexiconAsync()
    {
        if (_lexicon is IAsyncInit l)
            await l.InitAsync();
    }
}
