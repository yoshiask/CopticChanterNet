using CoptLib.Models;

namespace CopticChanter.WebApi.Core;

[Serializable]
public record Layout(string SessionKey, List<List<IDefinition>> Rows);