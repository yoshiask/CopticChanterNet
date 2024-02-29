using Humanizer;
using System.Net.Http.Json;

namespace CopticChanter.Blazor.App;

public static class Helpers
{
    public static string GetName(string path)
    {
        return Path.GetFileNameWithoutExtension(path).Humanize(LetterCasing.Title);
    }
}
