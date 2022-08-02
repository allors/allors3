namespace ExcelDNA
{
    using Allors.Excel;
    using Allors.Workspace;

    public class ErrorService : IErrorService
    {
        public void Handle(IResult response, ISession session) => response.HandleErrors(session);
    }
}
