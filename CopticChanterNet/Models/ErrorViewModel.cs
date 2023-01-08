using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Net;
using System;
using Microsoft.AspNetCore.Http;

namespace CopticChanterNet.Models
{
    public class ErrorViewModel : PageModel
    {
        public ErrorViewModel(HttpContext ctx)
        {
            ArgumentNullException.ThrowIfNull(ctx);

            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var feature = ctx.Features.Get<IExceptionHandlerPathFeature>();

            Message = feature?.Error switch
            {
                Exception ex => ex.Message,

                _ => null,
            };
        }

        public string? RequestId { get; set; }

        public string? Message { get; set; }

        public uint? ErrorCode { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}