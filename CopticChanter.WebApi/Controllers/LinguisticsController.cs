using CoptLib.Models;
using CoptLib.Writing;
using CoptLib.Writing.Linguistics;
using Microsoft.AspNetCore.Mvc;

namespace CopticChanter.WebApi.Controllers;

[Route("/linguistics")]
[ApiController]
public class LinguisticsController : ControllerBase
{
    [Route("transliterate")]
    [HttpGet]
    public IActionResult GetTransliteration([FromQuery] string text,
        [FromQuery(Name = "dst")] string dstStr, [FromQuery(Name = "src")] string? srcStr,
        [FromQuery(Name = "syll")] string? syllableSeparator)
    {
        if (!LanguageInfo.TryParse(dstStr, out var to))
            return BadRequest($"Invalid destination language '{dstStr}'");

        LanguageInfo? from;
        if (srcStr is not null && !LanguageInfo.TryParse(srcStr, out from))
            return BadRequest($"Invalid source language '{srcStr}'");
        else if (!LinguisticLanguageService.TryIdentifyLanguage(text, out from))
            return BadRequest($"Unable to infer source language. Please provide it using the `src` parameter.");

        SimpleContent content = new(text, null);

        var resultDef = LinguisticLanguageService.Default.Transliterate(content, to, from, syllableSeparator);
        var resultText = ((IContent)resultDef).Inlines.ToString();
        return Ok(resultText);
    }
}
