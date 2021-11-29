namespace Tests
{
    using System.Threading.Tasks;
    using Microsoft.Playwright;

    public class DashboardPage
    {
        public IPage Page { get; }

        public DashboardPage(IPage page) => this.Page = page;

        public async Task login(string username, string password = "")
        {
            await this.Page.GotoAsync("http://localhost:4200");

            await this.Page.FillAsync("input[name=\"username\"]", username);
            await this.Page.FillAsync("input[name=\"password\"]", password);
            await this.Page.ClickAsync("button:has-text(\"Sign In\")");
        }
    }
}
