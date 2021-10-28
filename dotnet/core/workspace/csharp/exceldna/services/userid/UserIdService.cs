namespace ExcelDNA
{
    using Allors.Excel;

    public class UserIdService : IUserIdService
    {
        public bool IsLoggedIn => this.UserId != 0;

        public long UserId { get; set; }
    }
}
