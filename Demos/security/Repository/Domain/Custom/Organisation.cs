namespace Allors.Repository
{
    using Attributes;
    using System;

    #region Allors
    [Id("bdf8a5ff-878d-464a-a0b5-fdaad81fb423")]
    #endregion
    public partial class Organisation : Object
    {
        #region Inherited Properties
        public Permission[] DeniedPermissions { get; set; }
        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors

        [Id("d8abd521-1c71-4e46-9e07-cf59a932a8f6")]
        [AssociationId("55e550f7-38b8-437c-b1c9-22a3f7b6cd11")]
        [RoleId("34aa1dd4-bb26-43b9-a332-99374c38264e")]

        #endregion Allors
        [Workspace]
        public string Name { get; set; }

        #region Allors

        [Id("397125dc-4246-47a6-a615-9f090cf5006b")]
        [AssociationId("41b2e56e-5d85-499b-a681-e95aaf17340a")]
        [RoleId("dcb2a767-f4ed-44bf-b08b-6d493cff9b34")]

        #endregion Allors
        [Workspace]
        public Invoice[] Invoices { get; set; }

        #region Allors

        [Id("6305f5d8-1316-46fc-9a44-bac6136d3b7b")]
        [AssociationId("87c31b4f-920d-412c-955a-b3176dd6520f")]
        [RoleId("7050ec3a-c9b8-45c6-9d40-988f52313fbe")]

        #endregion Allors
        [Workspace]
        public Person Owner { get; set; }

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