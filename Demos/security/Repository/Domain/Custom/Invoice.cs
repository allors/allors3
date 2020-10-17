namespace Allors.Repository
{
    using Attributes;
    using System;

    #region Allors
    [Id("57c18ae0-b000-4e4c-9a80-25a65c41f491")]
    #endregion
    public partial class Invoice : Object
    {
        #region Inherited Properties
        public Permission[] DeniedPermissions { get; set; }
        public SecurityToken[] SecurityTokens { get; set; }

        #endregion


        #region Allors
        [Id("bb433309-4ba0-441c-97eb-e35e6492fe96")]
        #endregion Allors
        [Workspace]
        public DateTime CreationDate { get; set; }

        #region Allors

        [Id("9ef3ad24-28c4-476c-a21d-a5e5c12f496a")]
       
        #endregion Allors
        [Workspace]
        public string Name { get; set; }

        #region inherited methods

        public void OnBuild()
        {

        }

        public void OnDerive()
        {

        }

        public void OnInit()
        {

        }

        public void OnPostBuild()
        {

        }

        public void OnPostDerive()
        {

        }

        public void OnPreDerive()
        {

        }

        #endregion inherited methods
    }
}
