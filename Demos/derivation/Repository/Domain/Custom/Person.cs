namespace Allors.Repository
{
    using Attributes;

    public partial class Person : Deletable
    {
        #region Allors
        [Id("105CF367-F076-45F8-8E2A-2431BB2D65C7")]
        [Size(256)]
        #endregion
        [Workspace]
        public string DomainFullName { get; set; }

        #region Allors
        [Id("0DDC847A-713D-4A19-9C6F-E8FE9175301D")]
        [Size(256)]
        #endregion
        [Workspace]
        public string DomainGreeting { get; set; }


        #region inherited methods

        public void Delete()
        {
        }

        #endregion inherited methods
    }
}
