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
            // Cookie auth: sign in through the test endpoint (Identity application cookie, shared with
            // the browser context via the dev proxy), then load the app authenticated.
            await this.Page.Context.APIRequest.PostAsync(
                "/allors/TestAuthentication/SignIn",
                new APIRequestContextOptions { DataObject = new { l = username } });

            await this.Page.GotoAsync("/");
            await this.Page.WaitForAngular();
        }
    }
}
