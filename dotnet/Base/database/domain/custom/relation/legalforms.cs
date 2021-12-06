// <copyright file="LegalForms.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class LegalForms
    {
        private static readonly Guid UKPublicLimitedCompanyId = new Guid("2C8413C4-C9A0-4193-ACF5-6F8EC6BCD5CB");
        private static readonly Guid UKLimitedLiabilityCompanyId = new Guid("91462406-14D0-41DB-AA80-32DADA7E9A12");
        private static readonly Guid UKOnePersonPrivateLimitedCompanyId = new Guid("B003323F-18AA-4378-B342-CA041289EAE1");
        private static readonly Guid UKCooperativeCompanyWithLimitedLiabilityId = new Guid("9B298557-F8DE-4014-A2FA-32E2A693C1FC");
        private static readonly Guid UKCooperativeCompanyWithUnlimitedLiabilityId = new Guid("A5BEC3D4-0A7A-47C8-AA79-2078582B770D");
        private static readonly Guid UKGeneralPartnershipId = new Guid("2380C77F-9FDD-4F75-AE29-69A555FBF285");
        private static readonly Guid UKLimitedPartnershipId = new Guid("B49E8E41-FED7-4023-A9AB-224B2DB57E0C");
        private static readonly Guid UKNonStockCorporationId = new Guid("91369994-A3C4-433F-890D-FAD793AF3E6F");
        private static readonly Guid UKCompanyEstablishedForSocialPurposesId = new Guid("C18249DF-1ED2-4E93-965A-A860C4040348");
        private static readonly Guid UKSelfEmployedId = new Guid("CFF2D0C0-46F7-4A84-A22B-5F49F79AF32B");

        private UniquelyIdentifiableCache<LegalForm> cache;

        public LegalForm UKPublicLimitedCompany => this.Cache[UKPublicLimitedCompanyId];
        public LegalForm UKLimitedLiabilityCompany => this.Cache[UKLimitedLiabilityCompanyId];
        public LegalForm UKOnePersonPrivateLimitedCompany => this.Cache[UKOnePersonPrivateLimitedCompanyId];
        public LegalForm UKCooperativeCompanyWithLimitedLiability => this.Cache[UKCooperativeCompanyWithLimitedLiabilityId];
        public LegalForm UKCooperativeCompanyWithUnlimitedLiability => this.Cache[UKCooperativeCompanyWithUnlimitedLiabilityId];
        public LegalForm UKGeneralPartnership => this.Cache[UKGeneralPartnershipId];
        public LegalForm UKLimitedPartnership => this.Cache[UKLimitedPartnershipId];
        public LegalForm UKNonStockCorporation => this.Cache[UKNonStockCorporationId];
        public LegalForm UKCompanyEstablishedForSocialPurposes => this.Cache[UKCompanyEstablishedForSocialPurposesId];
        public LegalForm UKSelfEmployed => this.Cache[UKSelfEmployedId];


        private UniquelyIdentifiableCache<LegalForm> Cache => this.cache ??= new UniquelyIdentifiableCache<LegalForm>(this.Transaction);

        protected override void CustomSetup(Setup setup)
        {

            var merge = this.Cache.Merger().Action();

            merge(UKPublicLimitedCompanyId, v => v.Name = "UK - Public Limited Company");
            merge(UKLimitedLiabilityCompanyId, v => v.Name = "UK - Limited Liability Company");
            merge(UKOnePersonPrivateLimitedCompanyId, v => v.Name = "UK - One Person Private Limited Company");
            merge(UKCooperativeCompanyWithLimitedLiabilityId, v => v.Name = "UK - Cooperative Company With Limited Liability");
            merge(UKCooperativeCompanyWithUnlimitedLiabilityId, v => v.Name = "UK - Cooperative Company With Unlimited Liability");
            merge(UKGeneralPartnershipId, v => v.Name = "UK - General Partnership");
            merge(UKLimitedPartnershipId, v => v.Name = "UK - Limited Partnership");
            merge(UKNonStockCorporationId, v => v.Name = "UK - Non Stock Corporation");
            merge(UKCompanyEstablishedForSocialPurposesId, v => v.Name = "UK - Company Established For Social Purposes");
            merge(UKSelfEmployedId, v => v.Name = "UK - Self Employed");
        }
    }
}
