namespace Allors.Database.Domain
{
    public partial class Notification
    {
        public bool ShouldEmail => !this.ExistEmailMessage;
    }
}
