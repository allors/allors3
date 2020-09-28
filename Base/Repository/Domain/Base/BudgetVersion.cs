// <copyright file="BudgetVersion.cs" company="Allors bvba">
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
    [Id("7FBECB76-27B6-44E3-BD08-FCBB6998B525")]
    #endregion
    public partial interface BudgetVersion : Version
    {
        #region Allors
        [Id("E119A204-C1A7-44FD-BEE0-7ADC93E72C44")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        BudgetState BudgetState { get; set; }

        #region Allors
        [Id("90AA323B-3DB8-483B-AA70-1667F66B92E0")]
        #endregion
        [Required]
        [Workspace(Default)]
        DateTime FromDate { get; set; }

        #region Allors
        [Id("0161AEB3-9955-4D04-B57B-FEFE4EE65A10")]
        #endregion
        [Workspace(Default)]
        DateTime ThroughDate { get; set; }

        #region Allors
        [Id("A29FBF93-FBDF-4E54-9044-F26452BB096B")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Comment { get; set; }

        #region Allors
        [Id("D77F2590-CDE3-45CB-8B69-75263FABEF84")]
        #endregion
        [Required]
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("7A2BC4B1-AD88-44E7-9962-B349BA6BA9D8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        BudgetRevision[] BudgetRevisions { get; set; }

        #region Allors
        [Id("02C70BA0-971A-417B-A9E1-E82DDE76173F")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string BudgetNumber { get; set; }

        #region Allors
        [Id("C3BF33B5-5D22-4B58-81E0-81A3FE88AAB1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        BudgetReview[] BudgetReviews { get; set; }

        #region Allors
        [Id("B264FB7F-A71B-47D2-A16D-3FBBC1DE6EC8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        BudgetItem[] BudgetItems { get; set; }
    }
}
