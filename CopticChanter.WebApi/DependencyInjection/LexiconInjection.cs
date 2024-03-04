using CoptLib.Writing.Lexicon;

namespace CopticChanter.WebApi.DependencyInjection;

public static class LexiconInjection
{
    public static IServiceCollection AddCopticLexicon(this IServiceCollection services)
    {
        CopticScriptoriumLexicon lexicon = new();
        return services.AddSingleton<ILexicon>(lexicon);
    }
}
