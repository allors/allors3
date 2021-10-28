namespace Allors.Excel
{
    using Workspace;

    public interface IUserIdService
    {
        bool IsLoggedIn { get; }
        long UserId { get; }
    }
}
