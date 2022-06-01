namespace Allors.Excel
{
    public interface IUserIdService
    {
        bool IsLoggedIn { get; }
        long UserId { get; }
    }
}
