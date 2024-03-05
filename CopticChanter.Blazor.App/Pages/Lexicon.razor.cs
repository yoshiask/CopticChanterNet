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
    
    public LexiconEntry? SelectedEntry { get; set; }

    public bool Loading { get; set; } = false;
    public bool Searching { get; set; } = false;


    public string Query { get; set; } = "";

    public string UsageStr { get; set; } = "Coptic";

    public IAsyncRelayCommand SearchCommand { get; }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (Id is null)
            return;

        Loading = true;
        ErrorMessage = null;
        try
        {
            SelectedEntry = await Client.GetEntryAsync(Id);
        }
        catch (Flurl.Http.FlurlHttpException httpEx)
        {
            SelectedEntry = null;
            ErrorMessage = await httpEx.GetResponseStringAsync();
        }
        catch (Exception ex)
        {
            SelectedEntry = null;
            ErrorMessage = ex.ToString();
        }

        Loading = false;
        StateHasChanged();
    }

    private async Task SearchAsync()
    {
        Searching = true;
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

        Searching = false;
        StateHasChanged();
    }

    protected void BeginSearch() => SearchCommand.ExecuteAsync(null).Forget();

    protected LexiconEntry? GetSelectedEntry()
    {
        if (Id is null)
            return null;

        return Response is null
            ? SelectedEntry
            : Response.Entries.FirstOrDefault(e => e.Id == Id);
    }
}
