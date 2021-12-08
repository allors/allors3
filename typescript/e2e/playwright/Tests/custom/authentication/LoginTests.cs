namespace Tests
{
    using System.Threading.Tasks;
    using Allors.E2E.Angular.Material.Authentication;
    using Microsoft.Playwright;
    using NUnit.Framework;

    public class LoginTests : Test
    {
        public override void Configure(BrowserTypeLaunchOptions options) => options.Headless = false;

        [Test]
        public async Task Login()
        {
            var page = new LoginComponent(this.Page);
            await page.Login("jane@example.com");

            Assert.AreEqual("Dashboard", await this.Page.TitleAsync());
        }
    }
}
