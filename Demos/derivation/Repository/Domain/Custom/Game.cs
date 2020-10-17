namespace Allors.Repository
{
    using Attributes;
    using System;

    #region Allors
    [Id("4c5f597f-7f75-4f4d-888a-1d21ebc9596a")]
    #endregion
    public partial class Game : Object
    {
        #region Inherited Properties
        public Permission[] DeniedPermissions { get; set; }
        public SecurityToken[] SecurityTokens { get; set; }

        #endregion


        #region Allors

        [Id("30feae98-41ef-462f-a704-e70558a64df0")]

        #endregion Allors
        [Workspace]
        public DateTime StartDate { get; set; }

        #region Allors

        [Id("73576236-f86a-4e74-b69f-dc1eec9f9213")]

        #endregion Allors
        [Workspace]
        public DateTime EndDate { get; set; }

        #region Allors

        [Id("23fb081e-20bd-4af8-8cc9-418a109b2e80")]

        #endregion Allors
        [Multiplicity(Multiplicity.OneToMany)]
        [Synced]
        [Workspace]
        public Score[] Scores { get; set; }

        #region Allors

        [Id("d0c95831-bc8c-4834-8b24-286886285afc")]

        #endregion Allors
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public GameMode GameMode { get; set; }

        #region Allors

        [Id("94f1308a-391a-4d06-bb19-5abaf9ee8388")]

        #endregion Allors
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace]
        public Person[] Winners { get; set; }

        #region Allors

        [Id("1803100c-f5f0-468d-8f0b-090fb46982f7")]

        #endregion Allors
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace]
        public Person[] Declarers { get; set; }

        #region Allors

        [Id("375d3881-d54f-4692-b2db-8864669103f4")]

        #endregion Allors
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace]
        [Derived]
        public Person[] Defenders { get; set; }

        #region Allors

        [Id("99ecbe80-b9b2-43a4-b9a3-7cd2e74e8182")]

        #endregion Allors
        [Workspace]
        public int ExtraTricks { get; set; }

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
