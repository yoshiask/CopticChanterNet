using CommunityToolkit.Mvvm.Input;
using CopticChanter.WebApi.Client;
using CopticChanter.WebApi.Core.Responses;
using CoptLib.Writing.Lexicon;
using Microsoft.AspNetCore.Components;
using OwlCore.Extensions;

namespace CopticChanter.Blazor.App.Pages;

public class LexiconBase : ComponentBase
{
    protected LexiconBase()
    {
        SearchCommand = new AsyncRelayCommand(SearchAsync);
    }

    [Inject]
    public CoptClient Client { get; set; }

    [Parameter]
    public string? Id { get; set; }


    public string? ErrorMessage { get; set; }

    public LexiconSearchResponse? Response { get; set; }

    public bool Loading { get; set; } = false;


    public string Query { get; set; } = "";

    public string UsageStr { get; set; } = "Coptic";

    public IAsyncRelayCommand SearchCommand { get; }

    private async Task SearchAsync()
    {
        Loading = true;
        ErrorMessage = null;
        try
        {
            var usage = CoptLib.Writing.LanguageInfo.Parse(UsageStr);

            Response = await Client.SearchLexiconAsync(Query, usage);
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

    protected void BeginSearch() => SearchCommand.ExecuteAsync(null).Forget();

    protected LexiconEntry? GetSelectedEntry()
    {
        if (Id is null)
            return null;

        return Response?.Entries.FirstOrDefault(e => e.Id == Id);
    }
}
