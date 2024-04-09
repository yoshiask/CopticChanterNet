using CopticChanter.WebApi.Core.Requests;
using CoptLib.Models;
using CoptLib.Writing.Linguistics;
using Microsoft.AspNetCore.Mvc;

namespace CopticChanter.WebApi.Controllers;

[Route("/linguistics")]
[ApiController]
public class LinguisticsController : ControllerBase
{
    [Route("transliterate")]
    [HttpPost]
    public IActionResult GetTransliteration([FromBody] TransliterationRequest request)
    {
        if (!request.TryGetDestinationLanguage(out var to))
            return BadRequest($"Invalid destination language '{request.Destination}'");

        if (!request.TryGetSourceLanguage(out var from))
            return BadRequest(request.Source is null
                ? "Unable to infer source language. Please provide it using the `Source` property."
                : $"Invalid source language '{request.Source}'");

        SimpleContent content = new(request.Text, null);

        var resultDef = LinguisticLanguageService.Default
            .Transliterate(content, to, from, request.SyllableSeparators);
        var resultText = ((IContent)resultDef).Inlines.ToString();
        return Ok(resultText);
    }
}
