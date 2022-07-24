namespace ExcelDNA
{
    using Allors.Excel;
    using Allors.Workspace;

    public class ExcelServices : IExcelServices
    {
        public ExcelServices(Ribbon ribbon)
        {
            this.ErrorService = new ErrorService();
            this.LoggerService = new LoggerService();
            this.MessageService = new MessageService();
            this.UserIdService = new UserIdService();
            this.RibbonService = new RibbonService(ribbon);
        }

        IErrorService IExcelServices.ErrorService => this.ErrorService;
        public ErrorService ErrorService { get; }

        ILoggerService IExcelServices.LoggerService => this.LoggerService;
        public LoggerService LoggerService { get; }

        IMessageService IExcelServices.MessageService => this.MessageService;
        public MessageService MessageService { get; }

        IUserIdService IExcelServices.UserIdService => this.UserIdService;
        public UserIdService UserIdService { get; }

        IRibbonService IExcelServices.RibbonService => this.RibbonService;
        public RibbonService RibbonService { get; }
    }
}
