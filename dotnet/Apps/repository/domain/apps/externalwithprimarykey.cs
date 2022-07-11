namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("0915d423-4fba-4b9c-bc0d-6378ec539985")]
    #endregion
    public partial interface ExternalWithPrimaryKey : Object
    {
        #region Allors
        [Id("3bf14c47-4f21-4fa4-a7f0-28a737b514f3")]
        [Indexed]
        [Size(256)]
        #endregion
        [Workspace]
        string ExternalPrimaryKey { get; set; }
    }
}
