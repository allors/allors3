namespace Workspace.ViewModels.Controllers;

using System.Collections.ObjectModel;
using Allors.Workspace;
using Allors.Workspace.Data;
using Allors.Workspace.Meta;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Services;
using Person = Allors.Workspace.Domain.Person;
using Task = Task;

public partial class PersonFormController : ObservableObject
{
    public PersonFormController(ISession session, IMessageService messageService)
    {
        this.Session = session;
        this.MessageService = messageService;
    }

    public ISession Session { get; set; }

    public IMessageService MessageService { get; }

    public ObservableCollection<PersonModel> People { get; } = new();

    [ObservableProperty] private PersonModel selected;

    partial void OnSelectedChanging(PersonModel selected)
    {
        Console.WriteLine(selected.FirstName);
    }

    [RelayCommand]
    private void ShowDialog()
    {
        var result = this.MessageService.ShowDialog("Yes or No?", "The Question");
        Console.WriteLine(result);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var m = this.Session.Workspace.Services.Get<M>();

        var pull = new Pull
        {
            Extent = new Filter(m.Person),
        };

        var result = await this.Session.PullAsync(pull);
        var people = result.GetCollection<Person>();

        this.People.Clear();
        foreach (var person in people)
        {
            this.People.Add(new PersonModel(person));
        }

        this.OnPropertyChanged(nameof(People));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var result = await this.Session.PushAsync();

        if (result.HasErrors)
        {
            this.MessageService.Show(result.ErrorMessage, "Error");
            return;
        }

        this.Session.Reset();

        await this.LoadAsync();
    }
}
