// <copyright file="PositionType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("4d599ed2-c5e3-4c1d-8128-6ff61f9072c3")]
    #endregion
    public partial class PositionType : UniquelyIdentifiable, Object
    {
        #region inherited properties

        public Guid UniqueId { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("08ca7d83-ca74-4cc1-9d8a-6cc254c7bd5b")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string Description { get; set; }

        #region Allors
        [Id("44d5c360-a82d-40ca-a56c-e377327a4858")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public Responsibility[] Responsibilities { get; set; }

        #region Allors
        [Id("520649d5-7775-43d0-ab4b-762b2ec6557e")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal BenefitPercentage { get; set; }

        #region Allors
        [Id("8e8e40ff-d11d-4805-abde-845a1b3f1241")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string Title { get; set; }

        #region Allors
        [Id("aa3886a5-a407-4598-900c-8fc3bcfc0604")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PositionTypeRate PositionTypeRate { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion
    }
}
