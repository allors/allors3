// <copyright file="GoodOrderItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("c1b6fac9-8e69-4c07-8cec-e9b52c690e72")]
    #endregion
    public partial class GoodOrderItem : EngagementItem
    {
        #region inherited properties
        public QuoteItem QuoteItem { get; set; }

        public string Description { get; set; }

        public DateTime ExpectedStartDate { get; set; }

        public DateTime ExpectedEndDate { get; set; }

        public WorkEffort EngagementWorkFulfillment { get; set; }

        public EngagementRate[] EngagementRates { get; set; }

        public EngagementRate CurrentEngagementRate { get; set; }

        public EngagementItem[] OrderedWiths { get; set; }

        public Person CurrentAssignedProfessional { get; set; }

        public Product Product { get; set; }

        public ProductFeature ProductFeature { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("de65b7a6-b2b3-4d77-9cb4-94720adb43f0")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal Price { get; set; }

        #region Allors
        [Id("f7399ebd-64f0-4bfa-a063-e75389d6a7cc")]
        #endregion

        public int Quantity { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void DelegateAccess() { }

        #endregion
    }
}
