// <copyright file="IrpfRegimes.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class IrpfRegimes
    {
        public static readonly Guid Assessable15Id = new Guid("f6780ae0-39fc-459c-8ec0-d3c5a5fb066c");
        public static readonly Guid Assessable19Id = new Guid("7F122CD7-4A8B-4BF5-B5AC-2FAD9155F9DD"); // TODO: old guid ("a82edb4e-dc92-4864-96fe-26ec6d1ef914")
        public static readonly Guid ExemptId = new Guid("424BF73C-9923-4D53-8159-9F2393015145"); // TODO: old guid ("82986030-5E18-43c1-8CBE-9832ACD4151D")

        private UniquelyIdentifiableCache<IrpfRegime> cache;

        public IrpfRegime Assessable15 => this.Cache[Assessable15Id];

        public IrpfRegime Assessable19 => this.Cache[Assessable19Id];

        public IrpfRegime Exempt => this.Cache[ExemptId];

        private UniquelyIdentifiableCache<IrpfRegime> Cache => this.cache ??= new UniquelyIdentifiableCache<IrpfRegime>(this.Transaction);

        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.IrpfRate);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(Assessable15Id, v =>
            {
                v.Name = "IRPF Assessable 15%";
                localisedName.Set(v, dutchLocale, "IRPF-plichtig 15%");
                v.IsActive = true;
            });
            var irpfregime = new IrpfRegimes(this.Transaction).FindBy(M.VatRegime.UniqueId, Assessable15Id);
            irpfregime.AddIrpfRate(new IrpfRates(this.Transaction).fifteen);

            merge(Assessable19Id, v =>
            {
                v.Name = "IRPF Assessable 19%";
                localisedName.Set(v, dutchLocale, "IRPF-plichtig 19%");
                v.IsActive = true;
            });
            irpfregime = new IrpfRegimes(this.Transaction).FindBy(M.VatRegime.UniqueId, Assessable19Id);
            irpfregime.AddIrpfRate(new IrpfRates(this.Transaction).nineteen);

            merge(ExemptId, v =>
            {
                v.Name = "Exempt";
                localisedName.Set(v, dutchLocale, "Vrijgesteld");
                v.IsActive = true;
            });
            irpfregime = new IrpfRegimes(this.Transaction).FindBy(M.VatRegime.UniqueId, ExemptId);
            irpfregime.AddIrpfRate(new IrpfRates(this.Transaction).Zero);
        }
    }
}
