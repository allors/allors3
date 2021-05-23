// <copyright file="VatRegimes.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class VatRegimes
    {
        public static readonly Guid DutchStandardId = new Guid("5973BE64-C785-480f-AF30-74D32C6D6AF9");
        public static readonly Guid DutchReducedId = new Guid("61fe8f79-eed2-4d40-a538-35361c47ad02");
        public static readonly Guid BelgiumStandardId = new Guid("a92b0454-118d-46f1-997f-50e935110599");
        public static readonly Guid BelgiumReducedTableAId = new Guid("942824ab-d7b9-4a86-8f32-46534710bd53");
        public static readonly Guid BelgiumReducedTableBId = new Guid("9ee2af8a-e353-4ab5-8b4e-fcd242ae728e");
        public static readonly Guid SpainStandardId = new Guid("4011c70f-d9f2-4da1-aa86-f48b76c9c3f0");
        public static readonly Guid SpainReducedId = new Guid("a82edb4e-dc92-4864-96fe-26ec6d1ef914");
        public static readonly Guid SpainCanaryIslandsId = new Guid("65df4f0e-d25f-4505-810c-4ce17f924397");
        public static readonly Guid SpainSuperReducedId = new Guid("26a285be-4bf5-4b10-9413-3664590a5b4e");
        public static readonly Guid ZeroRatedId = new Guid("3268B6E5-995D-4f4b-B94E-AF4BE25F4282");
        public static readonly Guid ExemptId = new Guid("82986030-5E18-43c1-8CBE-9832ACD4151D");
        public static readonly Guid IntraCommunautairId = new Guid("CFA1860E-DEBA-49a8-9062-E5577CDE0CCC");
        public static readonly Guid ServiceB2BId = new Guid("4D57C8ED-1DF4-4db2-9AAA-4552257DC2BF");

        private UniquelyIdentifiableCache<VatRegime> cache;

        public VatRegime DutchStandardTariff => this.Cache[DutchStandardId];

        public VatRegime DutchReducedTariff => this.Cache[DutchReducedId];

        public VatRegime BelgiumStandard => this.Cache[BelgiumStandardId];

        public VatRegime BelgiumReducedTableA => this.Cache[BelgiumReducedTableAId];

        public VatRegime BelgiumReducedTableB => this.Cache[BelgiumReducedTableBId];

        public VatRegime SpainStandard => this.Cache[SpainStandardId];

        public VatRegime SpainReduced => this.Cache[SpainReducedId];

        public VatRegime SpainSuperReduced => this.Cache[SpainSuperReducedId];

        public VatRegime SpainCanaryIslands => this.Cache[SpainCanaryIslandsId];

        public VatRegime ZeroRated => this.Cache[ZeroRatedId];

        public VatRegime IntraCommunautair => this.Cache[IntraCommunautairId];

        public VatRegime ServiceB2B => this.Cache[ServiceB2BId];

        public VatRegime Exempt => this.Cache[ExemptId];

        private UniquelyIdentifiableCache<VatRegime> Cache => this.cache ??= new UniquelyIdentifiableCache<VatRegime>(this.Transaction);

        protected override void AppsPrepare(Setup setup)
        {
            setup.AddDependency(this.ObjectType, M.Country);
            setup.AddDependency(this.ObjectType, this.M.VatRate);
            setup.AddDependency(this.ObjectType, this.M.VatClause);
        }

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;
            var netherlands = new Countries(this.Transaction).FindBy(M.Country.IsoCode, "NL");
            var belgium = new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE");
            var spain = new Countries(this.Transaction).FindBy(M.Country.IsoCode, "ES");

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(DutchStandardId, v =>
            {
                v.Name = "Dutch standard VAT tariff";
                localisedName.Set(v, dutchLocale, "Nederland hoog BTW-tarief");
                v.Country = netherlands;
                v.IsActive = true;
            });
            var vatregime = new VatRegimes(this.Transaction).FindBy(M.VatRegime.UniqueId, DutchStandardId);
            vatregime.AddVatRate(new VatRates(this.Transaction).Dutch21);

            merge(DutchReducedId, v =>
            {
                v.Name = "Dutch reduced VAT tariff";
                localisedName.Set(v, dutchLocale, "Nederland laag BTW-tarief");
                v.Country = netherlands;
                v.IsActive = true;
            });
            vatregime = new VatRegimes(this.Transaction).FindBy(M.VatRegime.UniqueId, DutchReducedId);
            vatregime.AddVatRate(new VatRates(this.Transaction).Dutch9);

            merge(BelgiumStandardId, v =>
            {
                v.Name = "Belgium standard VAT tariff";
                localisedName.Set(v, dutchLocale, "Belgie hoog BTW-tarief");
                v.Country = belgium;
                v.IsActive = true;
            });
            vatregime = new VatRegimes(this.Transaction).FindBy(M.VatRegime.UniqueId, BelgiumStandardId);
            vatregime.AddVatRate(new VatRates(this.Transaction).Belgium21);

            merge(BelgiumReducedTableAId, v =>
            {
                v.Name = "Belgium reduced VAT 6%";
                localisedName.Set(v, dutchLocale, "Belgie laag BTW-tarief 6%");
                v.Country = belgium;
                v.IsActive = true;
            });
            vatregime = new VatRegimes(this.Transaction).FindBy(M.VatRegime.UniqueId, BelgiumReducedTableAId);
            vatregime.AddVatRate(new VatRates(this.Transaction).Belgium6);

            merge(BelgiumReducedTableBId, v =>
            {
                v.Name = "Belgium reduced VAT tariff 12%";
                localisedName.Set(v, dutchLocale, "Belgie laag BTW-tarief 12%");
                v.Country = belgium;
                v.IsActive = true;
            });
            vatregime = new VatRegimes(this.Transaction).FindBy(M.VatRegime.UniqueId, BelgiumReducedTableBId);
            vatregime.AddVatRate(new VatRates(this.Transaction).Belgium12);

            merge(ServiceB2BId, v =>
            {
                v.Name = "Service B2B: Not VAT assessable";
                localisedName.Set(v, dutchLocale, "Service B2B: Niet BTW-plichtig");
                v.VatClause = new VatClauses(this.Transaction).ServiceB2B;
                v.Country = belgium;
                v.IsActive = true;
            });
            vatregime = new VatRegimes(this.Transaction).FindBy(M.VatRegime.UniqueId, ServiceB2BId);
            vatregime.AddVatRate(new VatRates(this.Transaction).BelgiumServiceB2B0);

            merge(SpainStandardId, v =>
            {
                v.Name = "Spain standard VAT tariff";
                localisedName.Set(v, dutchLocale, "Spanje hoog BTW-tarief");
                v.Country = spain;
                v.IsActive = true;
            });
            vatregime = new VatRegimes(this.Transaction).FindBy(M.VatRegime.UniqueId, SpainStandardId);
            vatregime.AddVatRate(new VatRates(this.Transaction).Spain21);

            merge(SpainReducedId, v =>
            {
                v.Name = "Spain reduced VAT tariff";
                localisedName.Set(v, dutchLocale, "Spanje laag BTW-tarief");
                v.Country = spain;
                v.IsActive = true;
            });
            vatregime = new VatRegimes(this.Transaction).FindBy(M.VatRegime.UniqueId, SpainReducedId);
            vatregime.AddVatRate(new VatRates(this.Transaction).Spain10);

            merge(SpainSuperReducedId, v =>
            {
                v.Name = "Spain super reduced VAT tariff";
                localisedName.Set(v, dutchLocale, "Spanje extra laag BTW-tarief");
                v.Country = spain;
                v.IsActive = true;
            });
            vatregime = new VatRegimes(this.Transaction).FindBy(M.VatRegime.UniqueId, SpainSuperReducedId);
            vatregime.AddVatRate(new VatRates(this.Transaction).Spain4);

            merge(SpainCanaryIslandsId, v =>
            {
                v.Name = "Spain Canary islands VAT tariff";
                localisedName.Set(v, dutchLocale, "Spanje Canarische eilanden BTW-tarief");
                v.Country = spain;
                v.IsActive = true;
            });
            vatregime = new VatRegimes(this.Transaction).FindBy(M.VatRegime.UniqueId, SpainCanaryIslandsId);
            vatregime.AddVatRate(new VatRates(this.Transaction).Spain7);

            merge(ZeroRatedId, v =>
            {
                v.Name = "VAT Zero rated tariff";
                localisedName.Set(v, dutchLocale, "BTW nul tarief");
                v.IsActive = true;
            });
            var zeroRatedvatregime = new VatRegimes(this.Transaction).FindBy(M.VatRegime.UniqueId, ZeroRatedId);
            zeroRatedvatregime.AddVatRate(new VatRates(this.Transaction).ZeroRated0);

            merge(ExemptId, v =>
            {
                v.Name = "VAT Exempt";
                localisedName.Set(v, dutchLocale, "Vrijgesteld van BTW");
                v.IsActive = true;
            });
            var exemptVatregime = new VatRegimes(this.Transaction).FindBy(M.VatRegime.UniqueId, ExemptId);
            exemptVatregime.AddVatRate(new VatRates(this.Transaction).Exempt0);

            merge(IntraCommunautairId, v =>
            {
                v.Name = "VAT intra-community";
                localisedName.Set(v, dutchLocale, "BTW Intracommunautair");
                v.VatClause = new VatClauses(this.Transaction).IntraCommunautair;
                v.IsActive = true;
            });
            var EuVatregime = new VatRegimes(this.Transaction).FindBy(M.VatRegime.UniqueId, IntraCommunautairId);
            EuVatregime.AddVatRate(new VatRates(this.Transaction).Intracommunity0);

            foreach (Country country in this.Transaction.Extent<Country>())
            {
                country.AddDerivedVatRegime(zeroRatedvatregime);
                country.AddDerivedVatRegime(exemptVatregime);

                if (Countries.EuMemberStates.Contains(country.IsoCode))
                {
                    country.AddDerivedVatRegime(EuVatregime);
                }
            }
        }
    }
}
