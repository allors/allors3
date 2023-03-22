namespace Workspace.ViewModels.Controllers;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Services;

public partial class MainFormController : ObservableObject
{
    public MainFormController(IMdiService mdiService)
    {
        this.MdiService = mdiService;
    }

    public IMdiService MdiService { get; }

    [RelayCommand]
    private void ShowPerson()
    {
        this.MdiService.Open(typeof(PersonFormController));
    }
}
