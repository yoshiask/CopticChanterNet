using System.Text;
using CopticChanter.WebApi.Core;
using CoptLib.IO;
using CoptLib.Models;
using CoptLib.Scripting;
using Microsoft.AspNetCore.Mvc;
using OwlCore.Extensions;

namespace CopticChanter.WebApi.Controllers;

[Route("/authoring")]
public class AuthoringController : Controller
{
    [Route("script")]
    [HttpPost]
    public async Task<IActionResult> RunScript([FromQuery] string typeId,
        [FromServices] Session session)
    {
        string body;
        using (MemoryStream bodyStream = new())
        {
            await Request.Body.CopyToAsync(bodyStream);
            var bodyBytes = bodyStream.ToArray();
            body = Encoding.UTF8.GetString(bodyBytes);
        }
        
        var script = ScriptingEngine.CreateScript(typeId, body);
        var context = session.Context;
        
        var output = script.Execute(context);
        if (output is not IDefinition def)
            return Ok(output?.ToString() ?? "");
        
        var xml = DocWriter.SerializeTransformedObject(def);
        var stream = await xml.ToStringAsync();
        return File(stream, ContentTypes.MIMETYPE_XML);
    }
}