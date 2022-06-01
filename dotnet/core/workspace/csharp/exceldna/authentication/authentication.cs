namespace ExcelDNA
{
    using System;
    using System.Threading.Tasks;
    using Allors.Excel;
    using Allors.Workspace.Adapters.Remote.ResthSharp;
    using Application;

    public class Authentication
    {
        public Authentication(Program program, DatabaseConnection databaseConnection, Client client, AppConfig configuration, UserIdService userIdService)
        {
            this.Program = program;
            this.DatabaseConnection = databaseConnection;
            this.Client = client;
            this.Configuration = configuration;
            this.UserIdService = userIdService;
        }

        public IProgram Program { get; }

        public DatabaseConnection DatabaseConnection { get; }

        public Client Client { get; }

        public AppConfig Configuration { get; }

        public UserIdService UserIdService { get; }

        public async Task Switch()
        {
            if (this.Client != null)
            {
                var wasLoggedIn = this.UserIdService.IsLoggedIn;

                if (this.UserIdService.IsLoggedIn)
                {
                    this.Client.Logoff();
                }
                else
                {
                    await this.Login();
                }

                long.TryParse(this.Client.UserId, out var userId);
                this.UserIdService.UserId = userId;

                if (wasLoggedIn != this.UserIdService.IsLoggedIn)
                {
                    if (this.UserIdService.IsLoggedIn)
                    {
                        await this.Program.OnLogin();
                    }
                    else
                    {
                        await this.Program.OnLogout();
                    }
                }
            }
        }

        private async Task Login()
        {
            try
            {
                if (!this.UserIdService.IsLoggedIn)
                {
                    var autoLogin = this.Configuration.AutoLogin;
                    var authenticationTokenUrl = this.Configuration.AllorsAuthenticationTokenUrl;

                    if (!string.IsNullOrWhiteSpace(autoLogin))
                    {
                        var user = this.Configuration.AutoLogin;
                        var uri = new Uri(authenticationTokenUrl, UriKind.Relative);
                        await this.Client.Login(uri, user, null);
                    }
                    else
                    {
                        if (!this.UserIdService.IsLoggedIn)
                        {
                            using var loginForm = new LoginForm
                            {
                                Client = this.Client,
                                Uri = new Uri(authenticationTokenUrl, UriKind.Relative)
                            };

                            loginForm.ShowDialog();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                e.Handle();
            }
        }
    }
}
