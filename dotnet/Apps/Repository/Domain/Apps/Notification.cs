namespace Allors.Repository
{
    public partial class Notification : EmailSource
    {
        #region inherited properties
        public EmailMessage EmailMessage { get; set; }
        #endregion
    }
}