// <copyright file="Priority.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("caa4814f-85a2-46a8-97a7-82220f8270cb")]
    #endregion
    public partial class Priority : Enumeration
    {
        #region inherited properties
        public LocalisedText[] LocalisedNames { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("5fc5b558-be52-4a09-b100-b8d1a01a9743")]
        #endregion
        [Required]
        [Indexed]
        [Workspace(Default)]
        public int DisplayOrder { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion

        #region Allors
        [Id("37C3DBE8-BB3F-47FC-ABAE-038EE9E725BE")]
        #endregion
        [Workspace(Default)]
        public void Delete() { }
    }
}
