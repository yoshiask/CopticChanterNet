namespace CopticChanter.WebApi.Core;

[Serializable]
public record AvailableContent(Dictionary<string, string> Docs, Dictionary<string, string> Sets, Dictionary<string, string> Sequences);
