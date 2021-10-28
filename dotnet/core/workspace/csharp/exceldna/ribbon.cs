namespace ExcelDNA
{
    using System;
    using ExcelDna.Integration.CustomUI;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;
    using Allors.Excel;
    using Allors.Excel.Interop;
    using Allors.Ranges;
    using Allors.Workspace;
    using Allors.Workspace.Adapters;
    using Allors.Workspace.Adapters.Remote;
    using Allors.Workspace.Adapters.Remote.ResthSharp;
    using Allors.Workspace.Meta.Lazy;
    using Application;
    using RestSharp;
    using RestSharp.Serializers.NewtonsoftJson;
    using Configuration = Allors.Workspace.Adapters.Remote.Configuration;
    using DatabaseConnection = Allors.Workspace.Adapters.Remote.ResthSharp.DatabaseConnection;
    using InteropApplication = Microsoft.Office.Interop.Excel.Application;
    using ExcelDna.Integration;

    [ComVisible(true)]
    public class Ribbon : ExcelRibbon, IRibbon
    {
        public IRibbonUI RibbonUI { get; private set; }

        public DatabaseConnection DatabaseConnection { get; private set; }

        public Client Client { get; private set; }

        public AsyncDatabaseClient AsyncDatabaseClient { get; private set; }

        public IWorkspace Workspace { get; private set; }

        public AddIn AddIn { get; private set; }

        public Program Program { get; private set; }

        public Authentication Authentication { get; private set; }

        public AppConfig AppConfig { get; private set; }

        public override string GetCustomUI(string _)
        {
            try
            {
                SynchronizationContext windowsFormsSynchronizationContext = new WindowsFormsSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(windowsFormsSynchronizationContext);

                this.AppConfig = new AppConfig();

                var metaPopulation = new MetaBuilder().Build();
                var objectFactory = new ReflectionObjectFactory(metaPopulation, typeof(Allors.Workspace.Domain.Person));
                var configuration = new Configuration("Default", metaPopulation, objectFactory);
                var idGenerator = new IdGenerator();
                var defaultRanges = new DefaultStructRanges<long>();

                var excelServices = new ExcelServices(this);

                IRestClient RestClientFactory() => new RestClient(this.AppConfig.AllorsDatabaseAddress).UseNewtonsoftJson();
                this.Client = new Client(RestClientFactory);
                this.DatabaseConnection = new DatabaseConnection(configuration, () => new WorkspaceServices(excelServices), this.Client, idGenerator, defaultRanges);
                this.Workspace = this.DatabaseConnection.CreateWorkspace();
                this.AsyncDatabaseClient = new AsyncDatabaseClient(this.DatabaseConnection);

                this.Program = new Program(this.Workspace, this.AsyncDatabaseClient);
                this.AddIn = new AddIn((InteropApplication)ExcelDnaUtil.Application, this.Program, this)
                {
                    //ExistentialAttribute = "AllorsExcel"
                };

                this.Authentication = new Authentication(this.Program, this.DatabaseConnection, this.Client, this.AppConfig, excelServices.UserIdService);

                return RibbonResources.Ribbon;
            }
            catch (Exception e)
            {
                e.Handle();
                throw;
            }
        }

        public async void OnLoad(IRibbonUI ribbon)
        {
            this.RibbonUI = ribbon;

            try
            {
                await this.Program.OnStart(this.AddIn);
            }
            catch (Exception e)
            {
                e.Handle();
            }
        }

        public void Invalidate() => this.RibbonUI.Invalidate();

        #region Ribbon Labels

        public string UserLabel { get; set; } = "Not logged in";

        public string AuthenticationLabel { get; set; } = "Log in";


        public string GetUserLabel(IRibbonControl control) => this.UserLabel;

        public string GetAuthenticateLabel(IRibbonControl control) => this.AuthenticationLabel;

        #endregion

        #region Ribbon Callbacks

        public async void OnAuthenticate(IRibbonControl _)
        {
            try
            {
                await this.Authentication.Switch();
            }
            catch (Exception e)
            {
                e.Handle();
            }
        }

        public async void OnClick(IRibbonControl control)
        {
            if (this.AddIn == null)
            {
                return;
            }

            try
            {
                await this.AddIn.Program.OnHandle(control.Id);
            }
            catch (Exception e)
            {
                e.Handle();
            }
        }

        public bool GetEnabled(IRibbonControl control) => this.AddIn.Program.IsEnabled(control.Id, control.Tag);

        #endregion

        #region Ribbon Helpers
        public override object LoadImage(string imageId)
        {
            // This will return the image resource with the name specified in the image='xxxx' tag
            return RibbonResources.ResourceManager.GetObject(imageId);
        }
        #endregion
    }
}
