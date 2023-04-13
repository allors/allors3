namespace Workspace.ViewModels.Features;

using Allors.Workspace.Domain;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class PersonViewModel : ObservableObject
{
    private readonly Person person;

    public PersonViewModel(Person person) => this.person = person;

    public string FirstName
    {
        get => this.person.FirstName;
        set => this.SetProperty(this.person.FirstName, value, this.person, (v, w) => v.FirstName = w);
    }

    public string LastName
    {
        get => this.person.LastName;
        set => this.SetProperty(this.person.LastName, value, this.person, (v, w) => v.LastName = w);
    }
}
