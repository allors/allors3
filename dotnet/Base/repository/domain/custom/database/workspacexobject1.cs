// <copyright file="Data.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("668B1D3A-EBD0-4029-8D5E-E23E9D0E5145")]
    #endregion
    [Workspace(X)]
    public partial class WorkspaceXObject1 : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("5A61A363-085C-4FFF-B955-4BE9EECF75C1")]
        [Indexed]
        #endregion
        [Workspace(X)]
        public string WorkspaceXString { get; set; }

        #region Allors
        [Id("1CC4E48F-4AD7-4EDB-A73E-AE70170DA1AF")]
        [Indexed]
        #endregion
        [Workspace(Y)]
        public string WorkspaceYString { get; set; }

        #region Allors
        [Id("9EBF0B4D-1C07-463B-85FF-1596FE4D7F71")]
        [Indexed]
        #endregion
        [Workspace(X, Y)]
        public string WorkspaceXYString { get; set; }

        #region Allors
        [Id("A768F9FC-9FFE-4B78-A69D-F5215111B932")]
        [Indexed]
        #endregion
        public string WorkspaceNonString { get; set; }

        #region Allors
        [Id("BE86B92F-733F-481A-B135-45A1FB991F34")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(X)]
        public WorkspaceXObject2[] WorkspaceXToWorkspaceXObject2 { get; set; }

        #region Allors
        [Id("469AB203-80DC-4B7A-96F0-3BBAC70AE2FC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(X)]
        public WorkspaceYObject2[] WorkspaceXToWorkspaceYObject2 { get; set; }

        #region Allors
        [Id("B6D27268-A296-4D89-A62F-79EEE11A5E4D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(X)]
        public WorkspaceXYObject2[] WorkspaceXToWorkspaceXYObject2 { get; set; }

        #region Allors
        [Id("37733B6E-5415-4ED5-8ADF-34FB7D12F420")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(X)]
        public WorkspaceNonObject2[] WorkspaceXToWorkspaceNonObject2 { get; set; }

        #region Allors
        [Id("CE030A91-2814-4B48-93A0-AE3B4A6EC8EC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Y)]
        public WorkspaceXObject2[] WorkspaceYToWorkspaceXObject2 { get; set; }

        #region Allors
        [Id("9A2827BE-B134-4B55-87A4-1D040CE80755")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Y)]
        public WorkspaceYObject2[] WorkspaceYToWorkspaceYObject2 { get; set; }

        #region Allors
        [Id("527E1595-A2B9-492D-A75E-E58C479F6003")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Y)]
        public WorkspaceXYObject2[] WorkspaceYToWorkspacXYObject2 { get; set; }

        #region Allors
        [Id("3C471941-299B-4E88-B443-9FD013CEED8D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Y)]
        public WorkspaceNonObject2[] WorkspaceYToWorkspaceNonObject2 { get; set; }

        #region Allors
        [Id("EB900F5B-BEB8-4E0C-95F4-359975111BF0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public WorkspaceXObject2[] WorkspaceNoneToWorkspaceXObject2 { get; set; }

        #region Allors
        [Id("19E3ED0F-A9C8-4CE6-A858-B71175F3BECC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public WorkspaceYObject2[] WorkspaceNoneToWorkspaceYObject2 { get; set; }

        #region Allors
        [Id("9C88791E-EFD0-49B1-86C2-2613909052BB")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public WorkspaceXYObject2[] WorkspaceNoneToWorkspaceXYObject2 { get; set; }

        #region Allors
        [Id("A0A91D88-A3C0-47C5-B6A7-61204146703A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public WorkspaceNonObject2[] WorkspaceNoneToWorkspaceNonObject2 { get; set; }

        #region Allors

        [Id("1C66795A-4BA0-4BEA-9A99-D5F04C826AE6")]

        #endregion
        [Workspace(X)]
        public void DoX() { }

        #region Allors

        [Id("AC3ECBA1-E611-4DF7-BAAF-60A886827B2F")]

        #endregion
        [Workspace(Y)]
        public void DoY() { }

        #region Allors

        [Id("388AC021-A9A5-4520-AFA5-8EAE58ACF3C0")]

        #endregion
        [Workspace(Y)]
        public void DoXY() { }

        #region Allors

        [Id("C09E37EC-D5E4-4F14-BEDC-C7274A344BFF")]

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
