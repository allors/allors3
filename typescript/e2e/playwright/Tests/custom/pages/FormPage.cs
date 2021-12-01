namespace Tests
{
    using System.Threading.Tasks;
    using Angular.Components;

    public class FormPage : ContainerComponent
    {
        public FormPage(IComponent container) : base(container, "FormComponent") { }

        public AllorsMaterialAutocompleteComponent AutocompleteDerivedFilter => new AllorsMaterialAutocompleteComponent(this, this.M.Data.AutocompleteDerivedFilter);

        public AllorsMaterialAutocompleteComponent AutocompleteFilter => new AllorsMaterialAutocompleteComponent(this, this.M.Data.AutocompleteFilter);

        public AllorsMaterialAutocompleteComponent AutocompleteOptions => new AllorsMaterialAutocompleteComponent(this, this.M.Data.AutocompleteOptions);

        public AllorsMaterialCheckboxComponent Checkbox => new AllorsMaterialCheckboxComponent(this, this.M.Data.Checkbox);

        public AllorsMaterialChipsComponent Chips => new AllorsMaterialChipsComponent(this, this.M.Data.Chips);

        public AllorsMaterialInputComponent Decimal => new AllorsMaterialInputComponent(this, this.M.Data.Decimal);

        public AllorsMaterialDatepickerComponent Date => new AllorsMaterialDatepickerComponent(this, this.M.Data.Date);

        public AllorsMaterialDatetimepickerComponent DateTime => new AllorsMaterialDatetimepickerComponent(this, this.M.Data.DateTime);

        public AllorsMaterialFileComponent File => new AllorsMaterialFileComponent(this, this.M.Data.File);

        public AllorsMaterialInputComponent String => new AllorsMaterialInputComponent(this, this.M.Data.String);

        public async Task SaveAsync()
        {
            await this.Locator.Locator("text=SAVE").ClickAsync();
            await this.Page.WaitForAngular();
        }
    }
}
