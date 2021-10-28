namespace Allors.Excel
{
    using Workspace;

    public interface IErrorService
    {
        void Handle(IResult result, ISession session);
    }
}
