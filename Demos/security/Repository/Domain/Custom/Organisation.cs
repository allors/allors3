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

        #endregion Allors
        [Workspace]
        public string Name { get; set; }

        #region Allors

        [Id("397125dc-4246-47a6-a615-9f090cf5006b")]

        #endregion Allors
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace]
        public Invoice[] Invoices { get; set; }

        #region Allors

        [Id("6305f5d8-1316-46fc-9a44-bac6136d3b7b")]

        #endregion Allors
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public Person Owner { get; set; }

        #region inherited methods

        public void OnBuild()
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

        #endregion inherited methods
    }
}
