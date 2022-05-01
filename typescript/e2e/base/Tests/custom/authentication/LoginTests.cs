namespace Tests.Authentication
{
    using System.Threading.Tasks;
    using Allors.E2E;
    using NUnit.Framework;

    public class LoginTests : Test
    {
        [Test]
        public async Task Login()
        {
            var page = new LoginComponent(this.Page);
            await page.Login("jane@example.com");

            Assert.AreEqual("Dashboard", await this.Page.TitleAsync());
        }
    }
}
