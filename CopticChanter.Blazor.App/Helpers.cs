using System.Net.Http.Json;

namespace CopticChanter.Blazor.App;

public static class Helpers
{
    public static async Task<AvailableContent> GetAvailableContentAsync(HttpClient client)
    {
        var content = await client.GetFromJsonAsync<AvailableContent>("availableContent.json")
            ?? throw new Exception("Failed to load available content");
        return content;
    }

    public static async Task<string> ResolveDocPathAsync(HttpClient client, string id)
    {
        var allContent = await GetAvailableContentAsync(client);
        if (!allContent.Docs.TryGetValue(id, out var fileName))
        {
            // Try to load the ID directly, just in case the file
            // exists but isn't in the index yet.
            fileName = id + ".xml";
        }

        return $"content/Docs/{fileName}";
    }

    public static async Task<string> ResolveSetPathAsync(HttpClient client, string id)
    {
        var allContent = await GetAvailableContentAsync(client);
        if (!allContent.Sets.TryGetValue(id, out var fileName))
        {
            // Try to load the ID directly, just in case the file
            // exists but isn't in the index yet.
            fileName = id + ".zip";
        }

        return $"content/Sets/{fileName}";
    }

    public static async Task<string> ResolveSequencePathAsync(HttpClient client, string id)
    {
        var allContent = await GetAvailableContentAsync(client);
        if (!allContent.Sequences.TryGetValue(id, out var fileName))
        {
            // Try to load the ID directly, just in case the file
            // exists but isn't in the index yet.
            fileName = id + ".xml";
        }

        return $"content/Sequences/{fileName}";
    }
}
