namespace CopticChanter.WebApi;

public static class AvailableContent
{
    public static IReadOnlyDictionary<string, string> Docs = new Dictionary<string, string>
    {
        ["28c70071-ce5e-4add-aa5c-d093acfb2787"] = "The Morning Doxology.xml",
        ["urn:tasbehaorg-cr:471"] = "The Thursday Theotokia (suscopt).xml",
    };

    public static IReadOnlyDictionary<string, string> Sequences = new Dictionary<string, string>
    {
        ["midnight-praises"] = "midnight-praises-sequence.xml",
        ["nativity-paramoun"] = "nativity-paramoun-sequence.xml",
    };

    public static IReadOnlyDictionary<string, string> Sets = new Dictionary<string, string>
    {
        ["midnight-praises"] = "midnight-praises.zip",
        ["apostles-fast"] = "apostles-fast.zip",
        ["malankara-nativity"] = "malankara-nativity.zip",
        ["nativity-paramone"] = "nativity-paramone.zip",
    };

    public static string GetDocPath(string id) => Path.Combine("content/Docs", Docs[id]);
    public static string GetSetPath(string id) => Path.Combine("content/Sets", Sets[id]);
    public static string GetSequencePath(string id) => Path.Combine("content/Sequences", Sequences[id]);

    public static string GetPath(string type, string id)
    {
        return type.ToUpperInvariant() switch
        {
            "DOC" => GetDocPath(id),
            "SET" => GetSetPath(id),
            "SEQ" => GetSequencePath(id),
            _ => throw new ArgumentException(null, nameof(type))
        };
    }

    public static Stream Open(string type, string id, IWebHostEnvironment env)
    {
        return env.WebRootFileProvider.GetFileInfo(GetPath(type, id)).CreateReadStream();
    }
}