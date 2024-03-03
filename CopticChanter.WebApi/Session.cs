using CopticChanter.WebApi.ContentSources;
using CoptLib.IO;

namespace CopticChanter.WebApi;

public class Session
{
    public Session(string key, ILoadContext context, IWebHostEnvironment env)
    {
        Key = key;
        Context = context;

        Content = new MemoryContentSource();

        var contextSource = new LoadContextContentSource(context);
        var globalSource = new EnvironmentContentSource(env, context);
        MergedContent = new MergedContentSource([contextSource, Content, globalSource]);
    }

    public string Key { get; }

    public ILoadContext Context { get; }

    public IModifiableContentSource Content { get; }

    public IContentSource MergedContent { get; }
}
