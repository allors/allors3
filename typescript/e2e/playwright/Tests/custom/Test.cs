namespace Tests
{
    using Microsoft.Playwright;

    public abstract class Test : E2ETest
    {
        public override void Configure(BrowserTypeLaunchOptions options) => options.Headless = false;

        public override void Configure(BrowserNewContextOptions options) => options.BaseURL = "http://localhost:4200";
    }
}
