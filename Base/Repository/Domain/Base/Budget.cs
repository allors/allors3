// <copyright file="Budget.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;
    using static Workspaces;

    #region Allors
    [Id("ebd4da8c-b86a-4317-86b9-e90a02994dcc")]
    #endregion
    public partial interface Budget : Period, Commentable, UniquelyIdentifiable, Transitional
    {
        #region ObjectStates
        #region BudgetState
        #region Allors
        [Id("2DD77672-178C-4561-804F-DB95A24D4DB4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        BudgetState PreviousBudgetState { get; set; }

        #region Allors
        [Id("7F959434-3302-4F06-B008-D80C356AD271")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        BudgetState LastBudgetState { get; set; }

        #region Allors
        [Id("C9030E23-C2E0-4C2C-8484-6FB8F5C1BFE1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        BudgetState BudgetState { get; set; }
        #endregion
        #endregion

        #region Allors
        [Id("1848add9-ab90-4191-b7f1-eb392be3ec4e")]
        #endregion
        [Required]
        [Size(-1)]
        string Description { get; set; }

        #region Allors
        [Id("1c3dd3b4-b514-4a42-965f-d3200325d78c")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        BudgetRevision[] BudgetRevisions { get; set; }

        #region Allors
        [Id("494d04ef-aafc-4482-a5c2-4ec9fa93d158")]
        #endregion
        [Size(256)]
        string BudgetNumber { get; set; }

        #region Allors
        [Id("834432b1-65b2-4499-a83d-71f0db6e177b")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        BudgetReview[] BudgetReviews { get; set; }

        #region Allors
        [Id("f6078f5b-036f-45de-ab4f-fb26b6939d11")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        BudgetItem[] BudgetItems { get; set; }

        #region Allors
        [Id("A6ED3503-571A-4800-B1BE-379CE197584F")]
        #endregion
        void Close();

        #region Allors
        [Id("B33D83BD-D1E5-4544-9B18-999EF78E4AE2")]
        #endregion
        void Reopen();
    }
}
