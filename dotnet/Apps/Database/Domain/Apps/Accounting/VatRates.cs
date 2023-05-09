// <copyright file="VatRates.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class VatRates
    {
        public static readonly Guid ZeroRatedId = new Guid("0D1BB08D-E2C3-4417-8587-EB5738A5FBBF");
        public static readonly Guid ExemptId = new Guid("61e17ad1-8457-4eb9-a8b9-6696065a5dbe");
        public static readonly Guid IntracommunityId = new Guid("875bd768-375a-4a8f-8494-aeb5d6843426");
        public static readonly Guid SpainSuperReducedId = new Guid("3c240520-2549-46ae-ab8b-3be525be6bf5");
        public static readonly Guid SpainIgicId = new Guid("0e2fe4dd-d7e8-43e8-8b30-1fc15cd5e675");
        public static readonly Guid SpainReducedId = new Guid("dba36f12-db21-40a6-93ec-7e9d85561459");
        public static readonly Guid SpainStandardId = new Guid("528ffde3-5740-46d8-ab0f-03c78626fbcd");
        public static readonly Guid DutchStandardId = new Guid("7CDDE391-1BB5-4329-A224-E7C26E1EE73E");
        public static readonly Guid DutchReducedId = new Guid("5276f5bb-c2fb-4d96-a8b3-e0d8caffa91b");
        public static readonly Guid BelgiumReducedId = new Guid("9D70146B-A18E-4A69-A134-0619AAB9FE52");
        public static readonly Guid BelgiumIntermediateId = new Guid("2D5E377F-A78C-4F38-8249-5A0F46F5DDAB");
        public static readonly Guid BelgiumStandardId = new Guid("220dbc32-1cf8-4b29-8585-784e55c1abc5");
        public static readonly Guid BelgiumServiceB2BId = new Guid("7e865270-fd83-4eb0-81da-aaca6d7c2d3a");

        private UniquelyIdentifiableCache<VatRate> cache;

        public VatRate ZeroRated => this.Cache[ZeroRatedId];

        public VatRate Exempt => this.Cache[ExemptId];

        public VatRate Intracommunity => this.Cache[IntracommunityId];

        public VatRate DutchReduced => this.Cache[DutchReducedId];

        public VatRate DutchStandard => this.Cache[DutchStandardId];

        public VatRate SpainSuperReduced => this.Cache[SpainSuperReducedId];

        public VatRate SpainIgic => this.Cache[SpainIgicId];

        public VatRate SpainReduced => this.Cache[SpainReducedId];

        public VatRate SpainStandard => this.Cache[SpainStandardId];

        public VatRate BelgiumReduced => this.Cache[BelgiumReducedId];

        public VatRate BelgiumIntermediate => this.Cache[BelgiumIntermediateId];

        public VatRate BelgiumStandard => this.Cache[BelgiumStandardId];

        public VatRate BelgiumServiceB2B => this.Cache[BelgiumServiceB2BId];

        private UniquelyIdentifiableCache<VatRate> Cache => this.cache ??= new UniquelyIdentifiableCache<VatRate>(this.Transaction);

        protected override void AppsPrepare(Security security) => security.AddDependency(this.Meta, this.M.Revocation);

        protected override void AppsSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();


            merge(ZeroRatedId, v =>
            {
                v.FromDate = new DateTime(2000, 01, 01, 0, 0, 0, DateTimeKind.Utc);
                v.Rate = 0;
            });

            merge(ExemptId, v =>
            {
                v.FromDate = new DateTime(2000, 01, 01, 0, 0, 0, DateTimeKind.Utc);
                v.Rate = 0;
            });

            merge(IntracommunityId, v =>
            {
                v.FromDate = new DateTime(2000, 01, 01, 0, 0, 0, DateTimeKind.Utc);
                v.Rate = 0;
            });

            merge(SpainSuperReducedId, v =>
            {
                v.FromDate = new DateTime(2012, 09, 01, 0, 0, 0, DateTimeKind.Utc);
                v.Rate = 4;
            });

            merge(SpainIgicId, v =>
            {
                v.FromDate = new DateTime(2020, 01, 01, 0, 0, 0, DateTimeKind.Utc);
                v.Rate = 7;
            });

            merge(SpainReducedId, v =>
            {
                v.FromDate = new DateTime(2012, 09, 01, 0, 0, 0, DateTimeKind.Utc);
                v.Rate = 10;
            });

            merge(SpainStandardId, v =>
            {
                v.FromDate = new DateTime(2012, 09, 01, 0, 0, 0, DateTimeKind.Utc);
                v.Rate = 21;
            });

            merge(DutchStandardId, v =>
            {
                v.FromDate = new DateTime(2012, 10, 01, 0, 0, 0, DateTimeKind.Utc);
                v.Rate = 21;
            });

            merge(DutchReducedId, v =>
            {
                v.FromDate = new DateTime(2019, 01, 01, 0, 0, 0, DateTimeKind.Utc);
                v.Rate = 9;
            });

            merge(BelgiumReducedId, v =>
            {
                v.FromDate = new DateTime(2000, 01, 01, 0, 0, 0, DateTimeKind.Utc);
                v.Rate = 6;
            });

            merge(BelgiumIntermediateId, v =>
            {
                v.FromDate = new DateTime(2000, 01, 01, 0, 0, 0, DateTimeKind.Utc);
                v.Rate = 12;
            });

            merge(BelgiumStandardId, v =>
            {
                v.FromDate = new DateTime(2000, 01, 01, 0, 0, 0, DateTimeKind.Utc);
                v.Rate = 21;
            });

            merge(BelgiumServiceB2BId, v =>
            {
                v.FromDate = new DateTime(2000, 01, 01, 0, 0, 0, DateTimeKind.Utc);
                v.Rate = 0;
            });
        }


        protected override void AppsSecure(Security config)
        {
            var revocations = new Revocations(this.Transaction);
            var permissions = new Permissions(this.Transaction);

            revocations.VatRateDeleteRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Delete),
            };
        }
    }
}
