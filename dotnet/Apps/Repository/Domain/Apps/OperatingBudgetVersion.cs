// <copyright file="OperatingBudgetVersion.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("B6594D57-4A7D-4747-B173-68F326C87E4D")]
    #endregion
    public partial class OperatingBudgetVersion : BudgetVersion
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public User LastModifiedBy { get; set; }

        public BudgetState BudgetState { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public string Comment { get; set; }

        public string Description { get; set; }

        public BudgetRevision[] BudgetRevisions { get; set; }

        public string BudgetNumber { get; set; }

        public BudgetReview[] BudgetReviews { get; set; }

        public BudgetItem[] BudgetItems { get; set; }

        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}
