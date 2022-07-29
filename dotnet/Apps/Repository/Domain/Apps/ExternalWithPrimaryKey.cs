namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("7a569bda-923f-4466-bef6-5a9ac91c9d89")]
    #endregion
    public partial interface ExternalWithPrimaryKey : Object
    {
        #region Allors
        [Id("f6338fcf-0aec-4658-84ac-d45d38bf4098")]
        [Indexed]
        [Size(256)]
        #endregion
        [Workspace]
        string ExternalPrimaryKey { get; set; }
    }
}
