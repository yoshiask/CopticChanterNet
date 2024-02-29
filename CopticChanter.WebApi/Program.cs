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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

CoptLib.Scripting.DotNetScript.Register();
CoptLib.Scripting.LuaScript.Register();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "api";
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors(allowedOrigins);

app.MapControllers();

app.Run();
