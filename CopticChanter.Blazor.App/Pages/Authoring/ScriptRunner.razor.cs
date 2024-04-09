using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.Input;
using CopticChanter.WebApi.Client;
using Microsoft.AspNetCore.Components;
using OwlCore.Extensions;

namespace CopticChanter.Blazor.App.Pages.Authoring;

public class ScriptRunnerBase : ComponentBase
{
    public ScriptRunnerBase()
    {
        RunCommand = new AsyncRelayCommand(RunAsync);
    }

    [Inject]
    [NotNull]
    public CoptClient? Client { get; set; }

    public string? ErrorMessage { get; set; }

    public string? Response { get; set; }

    public bool Loading { get; set; }


    public string Body { get; set; } = "";

    public string TypeId { get; set; } = "lua";

    public IAsyncRelayCommand RunCommand { get; }

    private async Task RunAsync()
    {
        Loading = true;
        ErrorMessage = null;
        try
        {
            Response = await Client.TestScriptAsync(Body, TypeId, null);
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

    protected void BeginRun() => RunCommand.ExecuteAsync(null).Forget();
}