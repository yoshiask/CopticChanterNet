using System.Text.Json.Serialization;
using CoptLib.Writing;

namespace CopticChanter.WebApi.Core.Requests;

[method: JsonConstructor]
public record LayoutRequest(DateTime? Date, IEnumerable<string> ExcludedLanguages, IEnumerable<string> Transliterations)
{
    public static LayoutRequest Create(DateTime? date, IEnumerable<LanguageInfo>? excludedLanguages,
        IEnumerable<LanguageInfo>? transliterations)
    {
        return new LayoutRequest(date,
            excludedLanguages?.Select(l => l.ToString()) ?? [],
            transliterations?.Select(l => l.ToString()) ?? []);
    }

    public IEnumerable<LanguageInfo> GetValidExcludedLanguages() => GetValidLanguages(ExcludedLanguages);

    public IEnumerable<LanguageInfo> GetValidTransliterations() => GetValidLanguages(Transliterations);

    private static IEnumerable<LanguageInfo> GetValidLanguages(IEnumerable<string>? langStrs)
    {
        if (langStrs is null) yield break;
        
        foreach (var langStr in langStrs)
            if (LanguageInfo.TryParse(langStr, out var l))
                yield return l;
    }
}