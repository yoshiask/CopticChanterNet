using System.Diagnostics.CodeAnalysis;
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
    [NotNull]
    public CoptClient? Client { get; set; }

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
            Response = await Client.TransliterateAsync(new(Text, DstStr, SrcStr, null));
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
