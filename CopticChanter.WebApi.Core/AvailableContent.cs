namespace CopticChanter.WebApi.Core;

[Serializable]
public record AvailableContent(List<string> Docs, List<string> Sets, List<string> Sequences);