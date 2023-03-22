namespace Workspace.ViewModels.Controllers;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Services;

public partial class PersonFormController : ObservableObject
{
    public PersonFormController(IMessageService messageService)
    {
        this.MessageService = messageService;
    }

    public IMessageService MessageService { get; }

    [RelayCommand]
    private void ShowDialog()
    {
        var result = this.MessageService.ShowDialog("Yes or No?", "The Question");
        Console.WriteLine(result);
    }
}
