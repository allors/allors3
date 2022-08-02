// <copyright file="Data.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;
    
    #region Allors
    [Id("98C0FF20-FE92-49EB-B99D-D0008F17AE98")]
    #endregion
    [Workspace(X, Y)]
    public partial class WorkspaceXYObject1 : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("C5213C10-7594-4738-810A-9E20231E12BC")]
        [Indexed]
        #endregion
        [Workspace(X)]
        public string WorkspaceXString { get; set; }

        #region Allors
        [Id("DFB48A4A-9A3D-4D68-B619-25E65FBE2678")]
        [Indexed]
        #endregion
        [Workspace(Y)]
        public string WorkspaceYString { get; set; }

        #region Allors
        [Id("D73B0673-9C3C-4607-BDD0-62EED374D648")]
        [Indexed]
        #endregion
        [Workspace(X, Y)]
        public string WorkspaceXYString { get; set; }

        #region Allors
        [Id("D2B2889B-EED1-49BB-92F1-01DF9B66F6AD")]
        [Indexed]
        #endregion
        public string WorkspaceNonString { get; set; }

        #region Allors
        [Id("3CEC2045-58CE-47DD-A9FF-F5A00C8F465D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(X)]
        public WorkspaceXObject2[] WorkspaceXToWorkspaceXObject2 { get; set; }

        #region Allors
        [Id("50397978-E647-48EB-880C-ED2E0F1510A9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(X)]
        public WorkspaceYObject2[] WorkspaceXToWorkspaceYObject2 { get; set; }

        #region Allors
        [Id("B09F6E18-71E0-4424-8490-7B222656D64C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(X)]
        public WorkspaceXYObject2[] WorkspaceXToWorkspaceXYObject2 { get; set; }

        #region Allors
        [Id("74599039-37F5-41AD-83BB-A7DBBC2ABF0A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(X)]
        public WorkspaceNonObject2[] WorkspaceXToWorkspaceNonObject2 { get; set; }

        #region Allors
        [Id("AE79CB92-F841-4751-B11C-8A4C8FA394FF")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Y)]
        public WorkspaceXObject2[] WorkspaceYToWorkspaceXObject2 { get; set; }

        #region Allors
        [Id("0E5EA187-E62A-40EE-BA54-84267ADC3EB7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Y)]
        public WorkspaceYObject2[] WorkspaceYToWorkspaceYObject2 { get; set; }

        #region Allors
        [Id("C66E4C45-CDA9-4CB0-824C-EF06EDC63DA0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Y)]
        public WorkspaceXYObject2[] WorkspaceYToWorkspacXYObject2 { get; set; }

        #region Allors
        [Id("D5789302-42D3-4285-97E7-8B714FC1F10D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Y)]
        public WorkspaceNonObject2[] WorkspaceYToWorkspaceNonObject2 { get; set; }

        #region Allors
        [Id("99B964AC-7D7D-41B6-8BAA-0FF7D1655A3B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public WorkspaceXObject2[] WorkspaceNoneToWorkspaceXObject2 { get; set; }

        #region Allors
        [Id("F4A423F5-7714-4A75-9F8E-58237C31EC2D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public WorkspaceYObject2[] WorkspaceNoneToWorkspaceYObject2 { get; set; }

        #region Allors
        [Id("2B621192-635A-42B8-A76C-0DA00212668D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public WorkspaceXYObject2[] WorkspaceNoneToWorkspaceXYObject2 { get; set; }

        #region Allors
        [Id("0A9B85FC-C33F-445B-8312-6DD29344EE42")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public WorkspaceNonObject2[] WorkspaceNoneToWorkspaceNonObject2 { get; set; }

        #region Allors

        [Id("29D0B05E-41B1-49AA-B959-CC6BD9A02750")]

        #endregion
        [Workspace(X)]
        public void DoX() { }

        #region Allors

        [Id("A875001A-627E-4501-9DCF-45873EEC6D0E")]

        #endregion
        [Workspace(Y)]
        public void DoY() { }

        #region Allors

        [Id("8AA43004-96C7-4A35-865B-59183F078B73")]

        #endregion
        [Workspace(Y)]
        public void DoXY() { }

        #region Allors

        [Id("0790BB5C-F373-40BC-8D26-A1234FA3578C")]

        #endregion
        [Workspace(Y)]
        public void DoNone() { }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}
