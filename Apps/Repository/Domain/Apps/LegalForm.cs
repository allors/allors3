// <copyright file="LegalForm.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("528cf616-6c67-42e1-af69-b5e6cb1192ea")]
    #endregion
    public partial class LegalForm : UniquelyIdentifiable
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }


        #endregion

        #region Allors
        [Id("2867d3b0-5def-4fc6-880a-be4bfe1d2597")]
        #endregion
        [Required]
        [Workspace(Default)]
        public string Description { get; set; }

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
