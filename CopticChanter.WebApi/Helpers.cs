using OwlCore.Extensions;

namespace CopticChanter.WebApi;

public static class Helpers
{
    public static async Task<T?> TryDeserializeJson<T>(Stream stream, Func<T?>? defaultFactory = null)
    {
        defaultFactory ??= () => default;

        try
        {
            using MemoryStream memStream = new();
            await stream.CopyToAsync(memStream);
            var bytes = await memStream.ToBytesAsync();
            var json = System.Text.Encoding.UTF8.GetString(bytes);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
        catch
        {
            return defaultFactory();
        }
    }
}