namespace Tests
{
    using System.Threading.Tasks;
    using Allors.E2E.Angular;
    using Allors.E2E.Angular.Cdk;
    using Microsoft.Playwright;
    using NUnit.Framework;

    public abstract class Test : E2ETest
    {
        public AppRoot AppRoot { get; private set; }

        public OverlayContainer OverlayContainer { get; private set; }

        public string WorkspaceName => "Default";

        public override void Configure(BrowserTypeLaunchOptions options) => options.Headless = false;

        public override void Configure(BrowserNewContextOptions options) => options.BaseURL = "http://localhost:4200";

        [SetUp]
        public async Task TestSetup()
        {
            await this.Page.GotoAsync("/");
            await this.Page.WaitForAngular();

            this.AppRoot = await AppRoot.New(this.Page, this.M, "allors-root");
            this.OverlayContainer = new OverlayContainer(this.AppRoot);
        }
    }
}
