// <copyright file="IncoTermTypes.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class IncoTermTypes
    {
        public static readonly Guid ExwId = new Guid("08F45D13-4354-494E-889E-BD84F73749D8");
        public static readonly Guid FcaId = new Guid("689D7B46-6DE5-4276-AF1B-F9A8A3DEB7CF");
        public static readonly Guid CptId = new Guid("CAF35B5B-7156-45D0-95E3-26632D0D4BF7");
        public static readonly Guid CipId = new Guid("D93EF678-15FE-4794-8C94-7384DD84ACBB");
        public static readonly Guid DatId = new Guid("8412EB47-C674-49C3-B2F5-F93B1E2C28FD");
        public static readonly Guid DapId = new Guid("2E050969-A3B8-481A-B37F-ABC11C50A9DD");
        public static readonly Guid DdpId = new Guid("D9CB935C-539E-403E-A368-15564038591B");
        public static readonly Guid FasId = new Guid("DC851808-BA41-46C4-AB32-18219E52939D");
        public static readonly Guid FobId = new Guid("5005D64C-2D6E-411F-9CF7-FBE3BE0DA79E");
        public static readonly Guid CfrId = new Guid("14C74444-F935-4AF2-9108-8CCEBD1920B2");
        public static readonly Guid CifId = new Guid("13C6ACE8-A928-45D4-8F2D-7F2320E38EE0");
        public static readonly Guid DduId = new Guid("691a8c51-6524-450f-8c73-d85cdb10418d");

        private UniquelyIdentifiableCache<IncoTermType> cache;

        public IncoTermType Exw => this.Cache[ExwId];

        public IncoTermType Fca => this.Cache[FcaId];

        public IncoTermType Cpt => this.Cache[CptId];

        public IncoTermType Dat => this.Cache[DatId];

        public IncoTermType Dap => this.Cache[DapId];

        public IncoTermType Ddp => this.Cache[DdpId];

        public IncoTermType Cip => this.Cache[CipId];

        public IncoTermType Fas => this.Cache[FasId];

        public IncoTermType Fob => this.Cache[FobId];

        public IncoTermType Cfr => this.Cache[CfrId];

        public IncoTermType Cif => this.Cache[CifId];

        public IncoTermType Ddu => this.Cache[DduId];

        private UniquelyIdentifiableCache<IncoTermType> Cache => this.cache ??= new UniquelyIdentifiableCache<IncoTermType>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).LocaleByName["nl"];

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(ExwId, v =>
            {
                v.Name = "Incoterm EXW (Ex Works)";
                v.Abbreviation = "EXW";
                localisedName.Set(v, dutchLocale, "Incoterm EXW (Af fabriek)");
                v.IsActive = true;
            });

            merge(FcaId, v =>
            {
                v.Name = "Incoterm FCA (Free Carrier)";
                v.Abbreviation = "FCA";
                localisedName.Set(v, dutchLocale, "Incoterm FCA (Vrachtvrij tot vervoerder)");
                v.IsActive = true;
            });

            merge(CptId, v =>
            {
                v.Name = "Incoterm CPT (Carriage Paid To)";
                v.Abbreviation = "CPT";
                localisedName.Set(v, dutchLocale, "Incoterm CPT (Vrachtvrij tot)");
                v.IsActive = true;
            });

            merge(CipId, v =>
            {
                v.Name = "Incoterm CIP (Carriage and Insurance Paid To))";
                v.Abbreviation = "CIP";
                localisedName.Set(v, dutchLocale, "Incoterm CIP (Vrachtvrij inclusief verzekering tot)");
                v.IsActive = true;
            });

            merge(DatId, v =>
            {
                v.Name = "Incoterm DAT (Delivered At Terminal)";
                v.Abbreviation = "DAT";
                localisedName.Set(v, dutchLocale, "Incoterm DAT (Franco terminal)");
                v.IsActive = true;
            });

            merge(DapId, v =>
            {
                v.Name = "Incoterm DAP (Delivered At Place)";
                v.Abbreviation = "DAP";
                localisedName.Set(v, dutchLocale, "Incoterm DAP (Franco ter plaatse)");
                v.IsActive = true;
            });

            merge(DdpId, v =>
            {
                v.Name = "Incoterm DDP (Delivered Duty Paid)";
                v.Abbreviation = "DDP";
                localisedName.Set(v, dutchLocale, "Incoterm DDP (Franco inclusief rechten)");
                v.IsActive = true;
            });

            merge(FasId, v =>
            {
                v.Name = "Incoterm FAS (Free Alongside Ship)";
                v.Abbreviation = "FAS";
                localisedName.Set(v, dutchLocale, "Incoterm FAS (Vrij langszij schip)");
                v.IsActive = true;
            });

            merge(FobId, v =>
            {
                v.Name = "Incoterm FOB (Free On Board)";
                v.Abbreviation = "FOB";
                localisedName.Set(v, dutchLocale, "Incoterm FOB (Vrij aan boord)");
                v.IsActive = true;
            });

            merge(CfrId, v =>
            {
                v.Name = "Incoterm CFR (Cost and Freight)";
                v.Abbreviation = "CFR";
                localisedName.Set(v, dutchLocale, "Incoterm CFR (Kostprijs en vracht)");
                v.IsActive = true;
            });

            merge(CifId, v =>
            {
                v.Name = "Incoterm CIF (Cost, Insurance and Freight)";
                v.Abbreviation = "CIF";
                localisedName.Set(v, dutchLocale, "Incoterm CIF (Kostprijs, verzekering en vracht)");
                v.IsActive = true;
            });

            merge(DduId, v =>
            {
                v.Name = "Incoterm DDU (Delivered Duty Unpaid)";
                v.Abbreviation = "DDU";
                localisedName.Set(v, dutchLocale, "Incoterm DDU (Delivered Duty Unpaid)");
                v.IsActive = true;
            });
        }
    }
}
