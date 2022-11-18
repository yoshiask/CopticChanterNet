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
    private IHostEnvironment _env;

    public DocSetViewModel(IHostEnvironment env)
    {
        _env = env;
        loadSetCommand = new AsyncRelayCommand(LoadSetAync);
    }

    [ObservableProperty]
    private IAsyncRelayCommand loadSetCommand;

    [ObservableProperty]
    private ObservableCollection<Doc> docs;

    [ObservableProperty]
    private List<List<ContentPart>> layout;

    [ObservableProperty]
    private string title;

    private async Task LoadSetAync()
    {
        var filePath = Path.Combine("wwwroot", "content", "docs");
        var fileInfos = _env.ContentRootFileProvider.GetDirectoryContents(filePath);

        // TODO: Load set from file
        if (!fileInfos.Exists)
            return;

        Docs = new();
        Layout = new();
        foreach (var fileInfo in fileInfos)
        {
            using var file = fileInfo.CreateReadStream();
            var doc = DocReader.ReadDocXml(file);

            if (doc == null)
                continue;

            DocReader.ApplyDocTransforms(doc);
            docs.Add(doc);

            Layout.AddRange(GenerateLayout(doc));
        }

        Title = "Midnight Praises";
    }

    private List<List<ContentPart>> GenerateLayout(Doc doc)
    {
        int translationCount = doc.Translations.Children.Count;

        // Create rows for each stanza
        int numRows = doc.Translations?.CountRows() ?? 0;
        List<List<ContentPart>> layout = new(numRows);
        for (int i = 0; i <= numRows; i++)
            layout.Add(new(translationCount));

        for (int t = 0; t < translationCount; t++)
        {
            GenerateLayoutForContentPart(doc.Translations![t], layout, t, 0);
        }

        return layout;
    }

    private void GenerateLayoutForContentPart(ContentPart part, List<List<ContentPart>> layout, int column, int row)
    {
        switch (part)
        {
            case IContent content:
                layout[row].Add(part);
                break;

            case IContentCollectionContainer contentCollection:
                foreach (var content in contentCollection.Children)
                    GenerateLayoutForContentPart(content, layout, column, ++row);
                break;
        }
    }
}
