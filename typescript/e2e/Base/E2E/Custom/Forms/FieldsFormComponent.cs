namespace Allors.E2E.Test
{
    using System.Threading.Tasks;
    using Angular;
    using E2E;

    public partial class FieldsFormComponent : ContainerComponent
    {

        public async Task SaveAsync()
        {
            await this.Locator.Locator("text=SAVE").ClickAsync();
            await this.Page.WaitForAngular();
        }
    }
}
