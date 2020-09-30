// <copyright file="PartyContactMechanism.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    // TODO: Rename to ContactMechanism
    #region Allors
    [Id("ca633037-ba1e-4304-9f2c-3353c287474b")]
    #endregion
    public partial class PartyContactMechanism : Commentable, Auditable, Period, Deletable
    {
        #region inherited properties
        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        #endregion

        #region Allors
        [Id("2ca2f403-67f8-49e6-9a62-4547d2cc83a1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public ContactMechanismPurpose[] ContactPurposes { get; set; }

        #region Allors
        [Id("afd94e13-db8e-45cd-8d6c-d9085054d71f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public ContactMechanism ContactMechanism { get; set; }

        #region Allors
        [Id("eb412c34-7127-4b37-8831-5280b9ed1885")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool UseAsDefault { get; set; }

        #region Allors
        [Id("f859fd15-4359-4de1-9927-75b6e443ffab")]
        #endregion
        [Workspace(Default)]
        public bool NonSolicitationIndicator { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }
        #endregion
    }
}
