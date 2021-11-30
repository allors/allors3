namespace Tests
{
    using Angular.Components;
    using Microsoft.Playwright;

    public class FormPage : AnnotatedComponent
    {
        public FormPage(IComponent container) : base(container, "FormComponent") { }

        public InputControl String => new InputControl(this, this.M.Data.String);

        public ILocator SaveLocator => this.Locator.Locator("text=SAVE");
    }
}
