namespace Tests
{
    using System;
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Database.Meta;
    using Allors.E2E.Angular;
    using Allors.E2E.Angular.Material.Authentication;
    using Microsoft.Playwright;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public abstract class Test
    {
        private static readonly Task<IPlaywright> PlaywrightFactory = Init();

        private static readonly object FixtureLockObject = new object();
        private Fixture fixture;

        public IPlaywright Playwright { get; private set; }

        public IBrowserType BrowserType { get; private set; }

        public IBrowserContext Context { get; private set; }

        public IBrowser Browser { get; set; }

        public IPage Page { get; private set; }

        public AppRoot AppRoot => new AppRoot(this.Page, this.M, "allors-root");

        public static async Task<IPlaywright> Init() => await Microsoft.Playwright.Playwright.CreateAsync();

        public Fixture Fixture
        {
            get
            {
                if (this.fixture != null)
                {
                    return this.fixture;
                }

                lock (FixtureLockObject)
                {
                    this.fixture ??= new Fixture();
                }

                return this.fixture;
            }
        }

        public MetaPopulation M { get; set; }

        public IDatabase Database { get; set; }

        public ITransaction Transaction { get; set; }

        [SetUp]
        public async Task TestSetup()
        {
            this.Playwright = await PlaywrightFactory;
            this.BrowserType = this.Playwright[(Environment.GetEnvironmentVariable("BROWSER") ?? Microsoft.Playwright.BrowserType.Chromium).ToLower()];

            var browserTypeLaunchOptions = new BrowserTypeLaunchOptions();
            if (bool.TryParse(Environment.GetEnvironmentVariable("HEADLESS"), out var headless))
            {
                browserTypeLaunchOptions.Headless = headless;
            }
            this.Configure(browserTypeLaunchOptions);
            this.Browser = await this.BrowserType.LaunchAsync(browserTypeLaunchOptions);
            var browserNewContextOptions = new BrowserNewContextOptions();
            this.Configure(browserNewContextOptions);
            this.Context = await this.Browser.NewContextAsync(browserNewContextOptions);
            this.Page = await this.Context.NewPageAsync();

            this.M = this.Fixture.MetaPopulation;
            this.Database = this.Fixture.Init();
            this.Transaction = this.Database.CreateTransaction();
        }

        [TearDown]
        public async Task TestTearDown()
        {
            await this.Page.CloseAsync();
            await this.Context.CloseAsync();
            await this.Browser.CloseAsync();
        }

        public virtual void Configure(BrowserTypeLaunchOptions options)
        {
        }

        public virtual void Configure(BrowserNewContextOptions options)
        {
        }

        protected async Task LoginAsync(string username, string password = null)
        {
            var loginPage = new LoginComponent(this.Page);
            await loginPage.Login(username, password);
        }

        protected async Task GotoAsync(string url)
        {
            await this.Page.GotoAsync(url);
            await this.Page.WaitForAngular();
        }
    }
}
