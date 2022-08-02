namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("d89d0bd1-2670-4168-bf8c-41f5a71bd225")]
    #endregion
    public partial interface EmailSource : Object
    {
        /// <summary>
        /// The EmailMessage.
        /// </summary>
        #region Allors
        [Id("4a79b583-ef01-4cdd-b114-9eceaabf3b9b")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        #endregion
        EmailMessage EmailMessage { get; set; }
    }
}
