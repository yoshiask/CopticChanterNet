using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CoptLib.IO;
using CoptLib.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using OwlCore.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CopticChanterNet.Models;

[ObservableObject]
public partial class DocSetViewModel
{
    private readonly string _setId;
    private readonly IHostEnvironment _env;

    public DocSetViewModel(string setId, IHostEnvironment env)
    {
        Guard.IsNotNull(setId);
        Guard.IsNotNull(env);

        _setId = setId;
        _env = env;
        loadSetCommand = new RelayCommand(LoadSet);
    }

    [ObservableProperty]
    private IRelayCommand loadSetCommand;

    [ObservableProperty]
    private ObservableCollection<Doc> docs;

    [ObservableProperty]
    private LoadContext context = new();

    [ObservableProperty]
    private List<List<object>> layout;

    [ObservableProperty]
    private string title;

    private void LoadSet()
    {
        // Prevent path traversal attacks
        if (!_setId.Any(char.IsLetter))
            throw new System.ArgumentException($"Set ID {_setId} was invalid.");

        var filePath = Path.Combine("wwwroot", "content", "sets", _setId);
        var fileInfos = _env.ContentRootFileProvider
            .GetDirectoryContents(filePath);

        // TODO: Load set from file
        if (!fileInfos.Exists)
            return;

        // Load definitions shared across all docs
        var cmDefsPath = Path.Combine("wwwroot", "content", "docs", "Common Definitions.xml");
        var cmDefsInfo = _env.ContentRootFileProvider.GetFileInfo(cmDefsPath);

        Docs = new();
        Layout = new();

        var setDocs = fileInfos
            .OrderBy(f => f.Name)
            .Prepend(cmDefsInfo);

        foreach (var fileInfo in setDocs)
        {
            using var file = fileInfo.CreateReadStream();

            if (Path.GetExtension(fileInfo.Name) == ".xml")
            {
                try
                {
                    var doc = context.LoadDoc(file);

                    if (doc == null)
                        continue;

                    DocReader.ApplyDocTransforms(doc);
                    docs.Add(doc);

                    Layout.AddRange(doc.Flatten());
                }
                catch (System.Exception ex)
                {
                    Layout.Add(new SimpleContent($"Document failed to load:\r\n{ex}", null).IntoList<object>());
                }
            }
            else
            {
                using StreamReader sr = new(file);
                Title = sr.ReadToEnd();
            }
        }
    }
}
