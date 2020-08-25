namespace Allors.Repository
{
    using Attributes;
    using System;

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
        [AssociationId("b6e87c7b-038a-4c37-8845-e3f11f1874fa")]
        [RoleId("4dc3ddba-87b9-4ae7-99e7-8c2e1b2a7f99")]

        #endregion Allors
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace]
        public Person[] Players { get; set; }

        #region Allors

        [Id("01ecdeef-5e64-4fdb-969d-b6ef1bd1ed3a")]
        [AssociationId("1acbfb96-e403-4ec3-b9ff-1efa59bd2993")]
        [RoleId("63960f82-6a8c-49ba-8b32-b9d2097cc6e5")]

        #endregion Allors
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace]
        public Game[] Games { get; set; }

        #region Allors

        [Id("b15bf813-1867-4e6f-8d86-1fbe9617d181")]
        [AssociationId("f98d612c-16b7-4a10-8353-21bc2ffb147c")]
        [RoleId("4c8d8092-d076-4a9f-9482-08acf6db0752")]

        #endregion Allors
        [Multiplicity(Multiplicity.OneToMany)]
        [Synced]
        [Workspace]
        public Score[] AccumulatedScores { get; set; }

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