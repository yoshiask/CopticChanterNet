using CoptLib.IO;
using CoptLib.Scripting;
using System.Collections.Concurrent;

const string allowedOrigins = nameof(allowedOrigins);

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: allowedOrigins,
            policy  =>
            {
                // Leave the API open for everyone
                policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
    });
}

builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddKeyedSingleton<ConcurrentDictionary<string, ILoadContext>>("sessions");
builder.Services.AddScoped(ctx =>
{
    var contextAccessor = ctx.GetRequiredService<IHttpContextAccessor>();
    var httpContext = contextAccessor.HttpContext!;

    var sessionKey = httpContext.Request.Query["sessionKey"].FirstOrDefault()
        ?? Guid.NewGuid().ToString("N").ToUpperInvariant();
    httpContext.Items["sessionKey"] = sessionKey;

    var sessions = ctx.GetRequiredKeyedService<ConcurrentDictionary<string, ILoadContext>>("sessions");
    var context = sessions.GetOrAdd(sessionKey, _ => new LoadContext());
    return context;
});

DotNetScript.Register();
LuaScript.Register();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors(allowedOrigins);

app.MapControllers();

app.Run();
