namespace Tests
{
    using System.Threading.Tasks;
    using Angular.Components;

    public class FormPage : ContainerComponent
    {
        public FormPage(IComponent container) : base(container) { }

        public AllorsMaterialAutocompleteComponent AutocompleteDerivedFilter => new AllorsMaterialAutocompleteComponent(this, this.M.Data.AutocompleteDerivedFilter);

        public AllorsMaterialAutocompleteComponent AutocompleteFilter => new AllorsMaterialAutocompleteComponent(this, this.M.Data.AutocompleteFilter);

        public AllorsMaterialAutocompleteComponent AutocompleteOptions => new AllorsMaterialAutocompleteComponent(this, this.M.Data.AutocompleteOptions);

        public AllorsMaterialCheckboxComponent Checkbox => new AllorsMaterialCheckboxComponent(this, this.M.Data.Checkbox);

        public AllorsMaterialChipsComponent Chips => new AllorsMaterialChipsComponent(this, this.M.Data.Chips);

        public AllorsMaterialInputComponent Decimal => new AllorsMaterialInputComponent(this, this.M.Data.Decimal);

        public AllorsMaterialDatepickerComponent Date => new AllorsMaterialDatepickerComponent(this, this.M.Data.Date);

        public AllorsMaterialDatetimepickerComponent DateTime => new AllorsMaterialDatetimepickerComponent(this, this.M.Data.DateTime);

        public AllorsMaterialFileComponent File => new AllorsMaterialFileComponent(this, this.M.Data.File);

        public AllorsMaterialFilesComponent MultipleFiles => new AllorsMaterialFilesComponent(this, this.M.Data.MultipleFiles);

        public AllorsMaterialMarkdownComponent Markdown => new AllorsMaterialMarkdownComponent(this, this.M.Data.Markdown);

        public AllorsMaterialRadioGroupComponent RadioGroup => new AllorsMaterialRadioGroupComponent(this, this.M.Data.RadioGroup);

        public AllorsMaterialSelectComponent Select => new AllorsMaterialSelectComponent(this, this.M.Data.Select);

        public AllorsMaterialSelectComponent SelectDerived => new AllorsMaterialSelectComponent(this, this.M.Data.SelectDerived);

        public AllorsMaterialSliderComponent Slider => new AllorsMaterialSliderComponent(this, this.M.Data.Slider);

        public AllorsMaterialSlideToggleComponent SlideToggle => new AllorsMaterialSlideToggleComponent(this, this.M.Data.SlideToggle);

        public AllorsMaterialInputComponent String => new AllorsMaterialInputComponent(this, this.M.Data.String);

        public AllorsMaterialTextareaComponent PlainText => new AllorsMaterialTextareaComponent(this, this.M.Data.PlainText);

        public async Task SaveAsync()
        {
            await this.Locator.Locator("text=SAVE").ClickAsync();
            await this.Page.WaitForAngular();
        }
    }
}
