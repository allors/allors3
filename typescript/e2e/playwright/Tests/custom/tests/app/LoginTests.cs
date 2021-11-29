namespace Tests
{
    using System.Threading.Tasks;
    using Microsoft.Playwright;
    using NUnit.Framework;

    public class LoginTests : Test
    {
        public override void Configure(BrowserTypeLaunchOptions options) => options.Headless = false;

        [Test]
        public async Task ShouldLogin()
        {
            var page = new LoginPage(this.Page);
            await page.Login("jane@example.com");

            Assert.AreEqual("AngularBaseApp", await this.Page.TitleAsync());
        }
    }
}
