// <copyright file="AccountingPeriod.cs" company="Allors bvba">
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
    [Id("6b56e13b-d075-40f1-8e33-a9a4c6cadb96")]
    #endregion
    public partial class AccountingPeriod : Budget, Versioned
    {
        #region inherited properties

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Guid UniqueId { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public string Description { get; set; }

        public BudgetRevision[] BudgetRevisions { get; set; }

        public string BudgetNumber { get; set; }

        public BudgetReview[] BudgetReviews { get; set; }

        public BudgetItem[] BudgetItems { get; set; }

        public BudgetState PreviousBudgetState { get; set; }

        public BudgetState LastBudgetState { get; set; }

        public BudgetState BudgetState { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        #endregion

        #region Allors
        [Id("0fd97106-1e39-4629-a7bd-ad263bc2d296")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public AccountingPeriod Parent { get; set; }

        #region Allors
        [Id("93b16073-8196-40c2-8777-5719fe1e6360")]
        #endregion
        [Required]
        public bool Active { get; set; }

        #region Allors
        [Id("babffef0-47ad-44ad-9a55-ffefb0fec783")]
        #endregion
        [Required]
        public int PeriodNumber { get; set; }

        #region Allors
        [Id("d776c4f4-9408-4083-8eb4-a4f940f6066f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public TimeFrequency Frequency { get; set; }

        #region Versioning
        #region Allors
        [Id("553520A4-2FC7-43C5-A98A-9118E33BA455")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public AccountingPeriodVersion CurrentVersion { get; set; }

        #region Allors
        [Id("B6F890C3-69E3-4438-A4A8-012CD9FD9A2D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public AccountingPeriodVersion[] AllVersions { get; set; }
        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Close() { }

        public void Reopen() { }

        #endregion
    }
}
