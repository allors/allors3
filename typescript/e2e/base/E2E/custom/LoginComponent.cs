namespace Allors.E2E
{
    using System.Threading.Tasks;
    using Angular;
    using Microsoft.Playwright;

    public class LoginComponent
    {
        public IPage Page { get; }

        public LoginComponent(IPage page) => this.Page = page;

        public async Task Login(string username, string password = null)
        {
            await this.Page.GotoAsync("/");

            await this.Page.FillAsync("input[name=\"username\"]", username);
            await this.Page.FillAsync("input[name=\"password\"]", password ?? string.Empty);
            await this.Page.ClickAsync("button:has-text(\"Sign In\")");

            await this.Page.WaitForAngular();
        }
    }
}
