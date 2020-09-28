// <copyright file="CustomEngagementItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("78022da7-d11c-4ab7-96f5-099d6608c4bb")]
    #endregion
    public partial class CustomEngagementItem : EngagementItem
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

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("71a3ed63-922f-44ae-8e89-6425759b3eb3")]
        #endregion
        [Size(-1)]

        public string DescriptionOfWork { get; set; }

        #region Allors
        [Id("f0b91526-924e-4f11-b27c-187010e1dff7")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal AgreedUponPrice { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void DelegateAccess() { }

        #endregion
    }
}
