using CommunityToolkit.Mvvm.Input;
using CopticChanter.WebApi.Client;
using Microsoft.AspNetCore.Components;
using OwlCore.Extensions;

namespace CopticChanter.Blazor.App.Pages;

public class TransliterateBase : ComponentBase
{
    public TransliterateBase()
    {
        TransliterateCommand = new AsyncRelayCommand(TransliterateAsync);
    }

    [Inject]
    public CoptClient Client { get; set; }

    public string? ErrorMessage { get; set; }

    public string? Response { get; set; }

    public bool Loading { get; set; } = false;


    public string Text { get; set; } = "";

    public string DstStr { get; set; } = "English";
    public string SrcStr { get; set; } = "";

    public IAsyncRelayCommand TransliterateCommand { get; }

    private async Task TransliterateAsync()
    {
        Loading = true;
        ErrorMessage = null;
        try
        {
            var dst = CoptLib.Writing.LanguageInfo.Parse(DstStr);
            _ = CoptLib.Writing.LanguageInfo.TryParse(SrcStr, out var src);

            Response = await Client.TransliterateAsync(Text, dst, src);
        }
        catch (Flurl.Http.FlurlHttpException httpEx)
        {
            Response = null;
            ErrorMessage = await httpEx.GetResponseStringAsync();
        }
        catch (Exception ex)
        {
            Response = null;
            ErrorMessage = ex.ToString();
        }

        Loading = false;
        StateHasChanged();
    }

    protected void BeginTransliteration() => TransliterateCommand.ExecuteAsync(null).Forget();
}
