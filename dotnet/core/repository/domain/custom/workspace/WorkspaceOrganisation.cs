// <copyright file="AccessClass.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("58d6dde7-5f03-43e4-bfb3-2373a01c773a")]
    #endregion
    [Workspace(Default)]
    [Origin(Origin.Workspace)]
    public partial class WorkspaceOrganisation
    {
        #region Allors
        [Id("3b27e46f-db39-4d92-a84c-3ccc2187f5ab")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Person[] WorkspaceDatabaseEmployees { get; set; }

        #region Allors
        [Id("b095916e-92f9-4ddf-8765-14ac203afb17")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Person WorkspaceDatabaseManager { get; set; }

        #region Allors
        [Id("cda36601-7d90-402f-8fb9-4f8cf6be4ee1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Person WorkspaceDatabaseOwner { get; set; }

        #region Allors
        [Id("7a07dbf3-addb-4260-910b-380c8df2e949")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Person[] WorkspaceDatabaseShareholders { get; set; }

        #region Allors
        [Id("6acaae68-a9fb-4015-8957-5060adef003f")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WorkspacePerson[] WorkspaceWorkspaceEmployees { get; set; }

        #region Allors
        [Id("2854f102-627d-44df-8321-05002636cc44")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WorkspacePerson WorkspaceWorkspaceManager { get; set; }

        #region Allors
        [Id("10ca877e-8c88-493a-be9f-501093ee21ac")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WorkspacePerson WorkspaceWorkspaceOwner { get; set; }

        #region Allors
        [Id("2e579566-b0b0-4cc6-996a-a8f670b27a67")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WorkspacePerson[] WorkspaceWorkspaceShareholders { get; set; }
    }
}
