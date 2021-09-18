// <copyright file="Data.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("DCCF9B09-308D-4FF3-953A-A15F7E332BAC")]
    #endregion
    public partial class WorkspaceNoneObject1 : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("C4AD393A-AA2A-4A72-AA51-24C68214828D")]
        [Indexed]
        #endregion
        [Workspace(X)]
        public string WorkspaceXString { get; set; }

        #region Allors
        [Id("FAB23C31-40BD-43E3-8410-59EA14D26F75")]
        [Indexed]
        #endregion
        [Workspace(Y)]
        public string WorkspaceYString { get; set; }

        #region Allors
        [Id("0D56AAB0-2E33-49F9-9D9F-9E396CFA5853")]
        [Indexed]
        #endregion
        [Workspace(X, Y)]
        public string WorkspaceXYString { get; set; }

        #region Allors
        [Id("94369FF3-D305-41A2-AAF2-424AB76BEE36")]
        [Indexed]
        #endregion
        public string WorkspaceNonString { get; set; }

        #region Allors
        [Id("D3C8BE05-3D7D-484E-B2D2-5182C48038C8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(X)]
        public WorkspaceXObject2[] WorkspaceXToWorkspaceXObject2 { get; set; }

        #region Allors
        [Id("27556E36-E0B6-421C-BFCB-E86D5FBBA3B8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(X)]
        public WorkspaceYObject2[] WorkspaceXToWorkspaceYObject2 { get; set; }

        #region Allors
        [Id("C38886A1-3BDE-455C-B4E4-40D8C558F37F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(X)]
        public WorkspaceXYObject2[] WorkspaceXToWorkspaceXYObject2 { get; set; }

        #region Allors
        [Id("E520B116-D693-48EA-9F5C-63E0C3F324A3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(X)]
        public WorkspaceNonObject2[] WorkspaceXToWorkspaceNonObject2 { get; set; }

        #region Allors
        [Id("57EE593D-1418-4650-B038-765F24CE75ED")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Y)]
        public WorkspaceXObject2[] WorkspaceYToWorkspaceXObject2 { get; set; }

        #region Allors
        [Id("C5428BAD-F1D3-46AE-A6E6-861708B33E73")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Y)]
        public WorkspaceYObject2[] WorkspaceYToWorkspaceYObject2 { get; set; }

        #region Allors
        [Id("75E105BF-6C1E-44AB-A07B-D1C69353B467")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Y)]
        public WorkspaceXYObject2[] WorkspaceYToWorkspacXYObject2 { get; set; }

        #region Allors
        [Id("5F33F504-70E3-4E35-9ABF-28764EAB9C1B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Y)]
        public WorkspaceNonObject2[] WorkspaceYToWorkspaceNonObject2 { get; set; }

        #region Allors
        [Id("3210B3C9-855A-4956-A255-0033042CDDF9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public WorkspaceXObject2[] WorkspaceNoneToWorkspaceXObject2 { get; set; }

        #region Allors
        [Id("C4776B8E-3809-466B-9981-59E8C9443ABB")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public WorkspaceYObject2[] WorkspaceNoneToWorkspaceYObject2 { get; set; }

        #region Allors
        [Id("0341145A-8FF1-4B84-8E32-9EFD0495EB8F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public WorkspaceXYObject2[] WorkspaceNoneToWorkspaceXYObject2 { get; set; }

        #region Allors
        [Id("DDBDCE07-F925-45A0-85A3-D3E097CE6E38")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public WorkspaceNonObject2[] WorkspaceNoneToWorkspaceNonObject2 { get; set; }

        #region Allors

        [Id("B0E355CF-3BB8-46C4-8EEA-DD80761DE7F2")]

        #endregion
        [Workspace(X)]
        public void DoX() { }

        #region Allors

        [Id("A9BBCA42-FC36-4F28-BCE7-CFA1D02D4A43")]

        #endregion
        [Workspace(Y)]
        public void DoY() { }

        #region Allors

        [Id("697102C6-7068-4302-AA2F-E938DD94933A")]

        #endregion
        [Workspace(Y)]
        public void DoXY() { }

        #region Allors

        [Id("E6FA36D8-443D-43DA-BA07-121A419ADDAC")]

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
