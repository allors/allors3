namespace Tests
{
    using System.Threading.Tasks;
    using Microsoft.Playwright;

    public class LoginPage
    {
        public IPage Page { get; }

        public LoginPage(IPage page) => this.Page = page;

        public async Task login(string administrator, string password = "")
        {
            await this.Page.GotoAsync("http://localhost:4200");

            await this.Page.FillAsync("angular=input[name=\"username\"]", "jane@example.com");
            await this.Page.FillAsync("input[name=\"password\"]", password);
            await this.Page.ClickAsync("button:has-text(\"Sign In\")");
        }
    }
}
