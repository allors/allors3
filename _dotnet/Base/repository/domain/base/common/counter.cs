// <copyright file="Counter.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("0568354f-e3d9-439e-baac-b7dce31b956a")]
    #endregion
    public partial class Counter : UniquelyIdentifiable
    {
        #region inherited properties
        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("309d07d9-8dea-4e99-a3b8-53c0d360bc54")]
        #endregion
        [Required]
        public int Value { get; set; }

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

        #endregion
    }
}
