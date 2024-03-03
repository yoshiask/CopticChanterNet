using System.Diagnostics.CodeAnalysis;

namespace CopticChanter.WebApi.Core;

public static class ContentTypes
{
    public const string MIMETYPE_DOCXML = "application/com.askharoun.coptic.doc+xml";
    public const string MIMETYPE_SET = "application/com.askharoun.coptic.set+zip";
    public const string MIMETYPE_SEQXML = "application/com.askharoun.coptic.seq+zip";

    public static string TypeToMime(string type)
    {
        return type.ToUpperInvariant() switch
        {
            "DOC" => MIMETYPE_DOCXML,
            "SET" => MIMETYPE_SET,
            "SEQ" => MIMETYPE_SEQXML,
            _ => throw new ArgumentException()
        };
    }

    public static bool TryMimeToType(string mime, [NotNullWhen(true)] out string? type)
    {
        type = mime switch
        {
            MIMETYPE_DOCXML => "DOC",
            MIMETYPE_SET => "SET",
            MIMETYPE_SEQXML => "SEQ",
            _ => null
        };

        return type is not null;
    }

    public static string MimeToType(string mime)
    {
        if (TryMimeToType(mime, out var type))
            return type;
        throw new ArgumentException();
    }
}
