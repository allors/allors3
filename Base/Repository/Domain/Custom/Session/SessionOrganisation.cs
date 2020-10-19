// <copyright file="AccessClass.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("f50bdd26-e2fa-40cc-988d-904a49ad5a08")]
    #endregion
    [Workspace(Default)]
    [Origin(Origin.Session)]
    public partial class SessionOrganisation
    {
        #region Allors
        [Id("3e318d16-7f59-46c1-8566-e5ddf1804fab")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Person[] SessionDatabaseEmployees { get; set; }

        #region Allors
        [Id("3e56a2c0-be14-415d-a705-4f2fc6daa09b")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Person SessionDatabaseManager { get; set; }

        #region Allors
        [Id("48d028f9-7ad5-4777-ac37-3f3bb8656bdf")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Person SessionDatabaseOwner { get; set; }

        #region Allors
        [Id("395f663b-0c54-4679-b588-eaae12a0ee3f")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Person[] SessionDatabaseShareholders { get; set; }

        #region Allors
        [Id("bcff2d76-aca4-4ce2-808a-d12831d6b54a")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WorkspacePerson[] SessionWorkspaceEmployees { get; set; }

        #region Allors
        [Id("832c7308-dc8f-42d8-aec3-f2432c6dad50")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WorkspacePerson SessionWorkspaceManager { get; set; }

        #region Allors
        [Id("e60c563e-e52a-4839-8e04-287d0c308279")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WorkspacePerson SessionWorkspaceOwner { get; set; }

        #region Allors
        [Id("9987d140-ef37-4362-8b64-de032bc3c278")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WorkspacePerson[] SessionWorkspaceShareholders { get; set; }

        #region Allors
        [Id("f98ddd9a-4aaf-4247-b5b8-efb486032f13")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SessionPerson[] SessionSessionEmployees { get; set; }

        #region Allors
        [Id("14a9c7e1-e0fd-4c21-b2f5-1fa6b542bbab")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SessionPerson SessionSessionManager { get; set; }

        #region Allors
        [Id("a4c2b80a-7879-416a-aa88-fca652b1cb6e")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SessionPerson SessionSessionOwner { get; set; }

        #region Allors
        [Id("b70d5a12-c92e-49c7-be2a-7d1b2ca36aff")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SessionPerson[] SessionSessionShareholders { get; set; }
    }
}
