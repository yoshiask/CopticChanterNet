using CopticChanter.WebApi.Services;
using CoptLib.IO;

namespace CopticChanter.WebApi.DependencyInjection;

public static class SessionInjection
{
    public static IServiceCollection AddCoptSessions(this IServiceCollection services)
    {
        services.AddKeyedSingleton<SessionIndex>("sessions");

        services.AddScoped(ctx =>
        {
            var contextAccessor = ctx.GetRequiredService<IHttpContextAccessor>();
            var httpContext = contextAccessor.HttpContext!;

            var sessionKey = httpContext.Request.Query["sessionKey"].FirstOrDefault()
                ?? Guid.NewGuid().ToString("N").ToUpperInvariant();

            var sessions = ctx.GetRequiredKeyedService<SessionIndex>("sessions");
            var context = sessions.GetOrAdd(sessionKey, k =>
            {
                ILoadContext context = new LoadContext();
                var env = ctx.GetRequiredService<IWebHostEnvironment>();
                return new(k, context, env);
            });
            return context;
        });

        services.AddHostedService<SessionCleaupService>();

        return services;
    }
}
