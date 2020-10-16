// <copyright file="RequestItemVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("CBEEED83-1411-4081-8605-8D2F4628BB52")]
    #endregion
    public partial class RequestItemVersion : Version
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public User LastModifiedBy { get; set; }

        #endregion

        #region Allors
        [Id("4768BB1B-5113-42D2-B301-6AEA705922BB")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        public RequestItemState RequestItemState { get; set; }

        #region Allors
        [Id("AFC66E59-4E01-4322-ADB8-3458AF745608")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        public string InternalComment { get; set; }

        #region Allors
        [Id("8DB3CC20-5583-4842-BA46-F644C5BB8D53")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string Description { get; set; }

        #region Allors
        [Id("6D0ACDF5-5BF9-4930-8FC3-4C39A87DE7A2")]
        #endregion
        [Workspace(Default)]
        public int Quantity { get; set; }

        #region Allors
        [Id("8A087F8A-6E70-45F2-9E4D-65465D0B8939")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public UnitOfMeasure UnitOfMeasure { get; set; }

        #region Allors
        [Id("D6CF8624-A47B-44D8-BDA2-208EBFF7D55A")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public Requirement[] Requirements { get; set; }

        #region Allors
        [Id("7285DB03-008F-45BF-9449-A596B22494B0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Deliverable Deliverable { get; set; }

        #region Allors
        [Id("DD74F2CE-9738-41D8-B683-94BFCC30B604")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ProductFeature ProductFeature { get; set; }

        #region Allors
        [Id("6CA576BB-FD8A-43E2-B504-23BE0237072B")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public NeededSkill NeededSkill { get; set; }

        #region Allors
        [Id("C8D8AB3A-42F9-40C2-951A-F806EC16A3E9")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Product Product { get; set; }

        #region Allors
        [Id("275F5E14-8DB8-4705-A202-631920EAC5FE")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal MaximumAllowedPrice { get; set; }

        #region Allors
        [Id("66ED32DE-B1E6-40FF-9D3B-14FE7CEF7755")]
        #endregion
        [Workspace(Default)]
        public DateTime RequiredByDate { get; set; }

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
