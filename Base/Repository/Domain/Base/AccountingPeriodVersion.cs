// <copyright file="AccountingPeriodVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;
    using static Workspaces;

    #region Allors
    [Id("3A39A249-7AF8-413D-8EC1-EE395A216A29")]
    #endregion
    public partial class AccountingPeriodVersion : BudgetVersion
    {
        #region inherited properties

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public User LastModifiedBy { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

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

        #region Allors
        [Id("EAE62DBF-6001-4A38-B0ED-483CD1A7F92D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public AccountingPeriod Parent { get; set; }

        #region Allors
        [Id("A4FA1929-2614-4C49-AAE1-7983343F25FF")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool Active { get; set; }

        #region Allors
        [Id("BA03A49E-3D5B-454C-8B36-C425B72694C8")]
        #endregion
        [Required]
        [Workspace(Default)]
        public int PeriodNumber { get; set; }

        #region Allors
        [Id("5A881C35-3912-47C2-9593-94FA98FEE458")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public TimeFrequency Frequency { get; set; }

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
