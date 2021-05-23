// <copyright file="BudgetReview.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("d12719f0-2c0e-4a9d-869b-4a209fc35a56")]
    #endregion
    public partial class BudgetReview : Commentable, Object
    {
        #region inherited properties
        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("4396be4d-edb4-405d-a39a-ee6ff5c39ca5")]
        #endregion
        [Required]

        public DateTime ReviewDate { get; set; }

        #region Allors
        [Id("6d065017-6c6f-413c-bc79-1a6349180c34")]
        #endregion
        [Required]
        [Size(-1)]

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
