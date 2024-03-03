namespace CopticChanter.WebApi.Core.Responses;

[Serializable]
public record CustomContentResponse(string SessionKey, string ContentId);
