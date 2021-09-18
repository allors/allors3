// <copyright file="Data.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;
    
    #region Allors
    [Id("A2B4CABF-66F4-4A65-8C36-D50E6DA30435")]
    #endregion
    [Workspace(Y)]
    public partial class WorkspaceYObject1 : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("CF3CFC90-0D3E-4E1E-B4CB-C2E66C970394")]
        [Indexed]
        #endregion
        [Workspace(X)]
        public string WorkspaceXString { get; set; }

        #region Allors
        [Id("F3C15357-8200-432C-9747-D94CAC6C38A1")]
        [Indexed]
        #endregion
        [Workspace(Y)]
        public string WorkspaceYString { get; set; }

        #region Allors
        [Id("3B7EABBE-182B-4A92-8E47-8EA70224DA25")]
        [Indexed]
        #endregion
        [Workspace(X, Y)]
        public string WorkspaceXYString { get; set; }

        #region Allors
        [Id("97194854-34DB-4BA7-8AFC-2D28CE2E780A")]
        [Indexed]
        #endregion
        public string WorkspaceNonString { get; set; }

        #region Allors
        [Id("2E7B02F2-A8AA-49BA-8E1C-3E96CD407E78")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(X)]
        public WorkspaceXObject2[] WorkspaceXToWorkspaceXObject2 { get; set; }

        #region Allors
        [Id("39431908-072C-46AF-9C47-DC0337F998A8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(X)]
        public WorkspaceYObject2[] WorkspaceXToWorkspaceYObject2 { get; set; }

        #region Allors
        [Id("67FFE32A-DF51-4CB3-8E47-989CB7301861")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(X)]
        public WorkspaceXYObject2[] WorkspaceXToWorkspaceXYObject2 { get; set; }

        #region Allors
        [Id("C3EA4F80-67B7-489B-8863-84AA8F15203C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(X)]
        public WorkspaceNonObject2[] WorkspaceXToWorkspaceNonObject2 { get; set; }

        #region Allors
        [Id("413D3449-2A29-4765-9765-3C7C13BE667F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Y)]
        public WorkspaceXObject2[] WorkspaceYToWorkspaceXObject2 { get; set; }

        #region Allors
        [Id("1BBFF37E-183C-4015-AE72-A3E4E44089D6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Y)]
        public WorkspaceYObject2[] WorkspaceYToWorkspaceYObject2 { get; set; }

        #region Allors
        [Id("A7D2BC21-003A-4DB4-A84E-048B2B3E4DA4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Y)]
        public WorkspaceXYObject2[] WorkspaceYToWorkspacXYObject2 { get; set; }

        #region Allors
        [Id("9822B65C-1FC3-4DE3-9D1F-F937530DCA3D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Y)]
        public WorkspaceNonObject2[] WorkspaceYToWorkspaceNonObject2 { get; set; }

        #region Allors
        [Id("A718A9BD-6CEE-42F9-BF22-45D3A1C399E9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public WorkspaceXObject2[] WorkspaceNoneToWorkspaceXObject2 { get; set; }

        #region Allors
        [Id("366CD8DC-95F1-4568-95F8-AE09DD4750BF")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public WorkspaceYObject2[] WorkspaceNoneToWorkspaceYObject2 { get; set; }

        #region Allors
        [Id("114D2D9D-954E-4712-AB71-FE30C82BD722")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public WorkspaceXYObject2[] WorkspaceNoneToWorkspaceXYObject2 { get; set; }

        #region Allors
        [Id("09808BF3-29C4-493F-B33A-CB751EA0B03B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public WorkspaceNonObject2[] WorkspaceNoneToWorkspaceNonObject2 { get; set; }

        #region Allors

        [Id("83CD6D91-3173-4C38-88B6-60B8487F206D")]

        #endregion
        [Workspace(X)]
        public void DoX() { }

        #region Allors

        [Id("538B9ECB-442F-413F-94ED-18E3914C2FB5")]

        #endregion
        [Workspace(Y)]
        public void DoY() { }

        #region Allors

        [Id("4395C8C6-8244-4491-9F6D-073BE4CFB22E")]

        #endregion
        [Workspace(Y)]
        public void DoXY() { }

        #region Allors

        [Id("5B192A0A-673C-4E3D-ABFC-A4ED6A88B504")]

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
