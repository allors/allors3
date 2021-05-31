namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("ec923a15-6ff4-4ad6-8a7a-eb06a2c2e1b6")]
    #endregion
    public partial class Scoreboard : Object
    {
        #region Inherited Properties
        public Permission[] DeniedPermissions { get; set; }
        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors

        [Id("8f31ae77-ed28-4931-a689-9261315ad44c")]

        #endregion Allors
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace]
        public Person[] Players { get; set; }

        #region Allors

        [Id("01ecdeef-5e64-4fdb-969d-b6ef1bd1ed3a")]

        #endregion Allors
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace]
        public Game[] Games { get; set; }

        #region Allors

        [Id("b15bf813-1867-4e6f-8d86-1fbe9617d181")]

        #endregion Allors
        [Multiplicity(Multiplicity.OneToMany)]
        [Synced]
        [Workspace]
        public Score[] AccumulatedScores { get; set; }

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
