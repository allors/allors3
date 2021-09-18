// <copyright file="PersistentPreparedSelect.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("02C7569C-8F54-4F8D-AC09-1BACD9528F1F")]
    #endregion
    public partial class PersistentPreparedSelect : UniquelyIdentifiable, Deletable
    {
        #region inherited properties

        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("A8CFE3FC-EC2E-4407-B3C5-681603AA2B67")]
        #endregion
        [Size(256)]
        public string Name { get; set; }

        #region Allors
        [Id("B5A89EE5-960F-4ABC-A43D-19438264E019")]
        #endregion
        [Size(-1)]
        public string Description { get; set; }

        #region Allors
        [Id("B26239E3-FA65-43F1-AAFF-0058DCCB462A")]
        #endregion
        [Size(-1)]
        public string Content { get; set; }

        #region inherited methods

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild()
        {
        }

        public void OnPostBuild()
        {
        }

        public void OnInit()
        {
        }

        public void OnPostDerive()
        {
        }

        public void Delete()
        {
        }

        #endregion
    }
}
