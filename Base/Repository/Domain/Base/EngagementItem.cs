// <copyright file="EngagementItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("aa3bf631-5aa5-48ab-a249-ef61f640fb72")]
    #endregion
    public partial interface EngagementItem : DelegatedAccessControlledObject
    {
        #region Allors
        [Id("141333b6-2cc9-487e-acc1-86d314f2b30a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        QuoteItem QuoteItem { get; set; }

        #region Allors
        [Id("2a187dcd-5004-4722-a0ec-e571cd5b5bc6")]
        #endregion
        [Required]
        [Size(-1)]

        string Description { get; set; }

        #region Allors
        [Id("33fe3f86-8b73-4a70-b9c0-62ac27531ac3")]
        #endregion

        DateTime ExpectedStartDate { get; set; }

        #region Allors
        [Id("3635cb84-2d4f-4fa1-ac18-4c8a6cc129c5")]
        #endregion

        DateTime ExpectedEndDate { get; set; }

        #region Allors
        [Id("40b24df7-6834-401a-a598-82203af63f99")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        WorkEffort EngagementWorkFulfillment { get; set; }

        #region Allors
        [Id("9133f59e-048d-4020-88e4-5a4bc36d663b")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]

        EngagementRate[] EngagementRates { get; set; }

        #region Allors
        [Id("9e1f4da4-41af-4030-b67f-79f1f49fa076")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]

        EngagementRate CurrentEngagementRate { get; set; }

        #region Allors
        [Id("b445f2d6-55a6-4cb4-9550-5be8863eddb6")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]

        EngagementItem[] OrderedWiths { get; set; }

        #region Allors
        [Id("c2ec3c6b-af56-4c6b-bdaf-76d3ea340bf7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]

        Person CurrentAssignedProfessional { get; set; }

        #region Allors
        [Id("c7204c16-67b1-4e6d-b787-ce8ab9c6c111")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        Product Product { get; set; }

        #region Allors
        [Id("dbb3d0c5-836d-477b-a42f-b260f3316458")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        ProductFeature ProductFeature { get; set; }
    }
}
