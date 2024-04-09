using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using CoptLib.Writing;
using CoptLib.Writing.Linguistics;

namespace CopticChanter.WebApi.Core.Requests;

[method: JsonConstructor]
public record TransliterationRequest(
    string Text, string Destination, string? Source, SyllableSeparatorSet? SyllableSeparators)
{
    public TransliterationRequest(string text, LanguageInfo dst, LanguageInfo? src, SyllableSeparatorSet? syll)
        : this(text, dst.ToString(), src?.ToString(), syll)
    {
    }

    public bool TryGetDestinationLanguage([NotNullWhen(true)] out LanguageInfo? dst)
        => LanguageInfo.TryParse(Destination, out dst);

    public bool TryGetSourceLanguage([NotNullWhen(true)] out LanguageInfo? src)
        => Source is not null
            ? LanguageInfo.TryParse(Source, out src)
            : LinguisticLanguageService.TryIdentifyLanguage(Text, out src);
}