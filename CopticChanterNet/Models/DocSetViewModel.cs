using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CoptLib.IO;
using CoptLib.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
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
    private IHostEnvironment _env;

    public DocSetViewModel(string setId, IHostEnvironment env)
    {
        _setId = setId;
        _env = env;
        loadSetCommand = new AsyncRelayCommand(LoadSetAync);
    }

    [ObservableProperty]
    private IAsyncRelayCommand loadSetCommand;

    [ObservableProperty]
    private ObservableCollection<Doc> docs;

    [ObservableProperty]
    private List<List<object>> layout;

    [ObservableProperty]
    private string title;

    private async Task LoadSetAync()
    {
        // Prevent path traversal attacks
        if (!_setId.Any(char.IsLetter))
            throw new System.ArgumentException($"Set ID {_setId} was invalid.");

        var filePath = Path.Combine("wwwroot", "content", "sets", _setId);
        var fileInfos = _env.ContentRootFileProvider.GetDirectoryContents(filePath);

        // TODO: Load set from file
        if (!fileInfos.Exists)
            return;

        Docs = new();
        Layout = new();
        foreach (var fileInfo in fileInfos)
        {
            using var file = fileInfo.CreateReadStream();

            if (Path.GetExtension(fileInfo.Name) == ".xml")
            {
                var doc = DocReader.ReadDocXml(file);

                if (doc == null)
                    continue;

                DocReader.ApplyDocTransforms(doc);
                docs.Add(doc);

                Layout.AddRange(doc.Flatten());
            }
            else
            {
                using StreamReader sr = new(file);
                Title = sr.ReadToEnd();
            }
        }
    }
}
