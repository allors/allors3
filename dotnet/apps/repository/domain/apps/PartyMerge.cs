// <copyright file="PartyMerge.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("804cc63e-644a-4ee3-858d-6df50c33ba46")]
    #endregion
    public partial class PartyMerge : Deletable, Auditable
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        #endregion

        #region Allors
        [Id("d9acbc8f-9c4d-4fa5-9baf-03e74c0a940d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Party From { get; set; }

        #region Allors
        [Id("081db432-fd94-469f-b017-e49b8a0fc7ed")]
        #endregion
        public string FromId { get; set; }

        #region Allors
        [Id("bd9a97d3-ed62-4d8d-9071-790cf598bc34")]
        #endregion
        public Guid FromUniqueId { get; set; }

        #region Allors
        [Id("68c14bd9-788d-4a19-8855-ffe762642ee2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Party Into { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }
        #endregion
    }
}
