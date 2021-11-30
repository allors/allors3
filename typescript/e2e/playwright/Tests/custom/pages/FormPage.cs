namespace Tests
{
    using System.Threading.Tasks;
    using Angular.Components;

    public class FormPage : ContainerComponent
    {
        public FormPage(IComponent container) : base(container, "FormComponent") { }

        public AllorsMaterialAutocompleteComponent AutocompleteDerivedFilter => new AllorsMaterialAutocompleteComponent(this, this.M.Data.AutocompleteDerivedFilter);

        public AllorsMaterialAutocompleteComponent AutocompleteFilter => new AllorsMaterialAutocompleteComponent(this, this.M.Data.AutocompleteFilter);

        public AllorsMaterialInputComponent Decimal => new AllorsMaterialInputComponent(this, this.M.Data.Decimal);

        public AllorsMaterialInputComponent String => new AllorsMaterialInputComponent(this, this.M.Data.String);

        public async Task SaveAsync() => await this.Locator.Locator("text=SAVE").ClickAsync();
    }
}
