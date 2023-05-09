// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class InternalOrganisationAccountingSettingsInternalOrganisationIrpfRateSettingsRule : Rule
    {
        public InternalOrganisationAccountingSettingsInternalOrganisationIrpfRateSettingsRule(MetaPopulation m) : base(m, new Guid("1c8125a6-7e2b-4c37-812c-8c03b3294076")) =>
        this.Patterns = new Pattern[]
        {
            m.InternalOrganisation.RolePattern(v => v.Country),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Organisation>().Where(v => v.IsInternalOrganisation))
            {
                @this.DeriveInternalOrganisationAccountingSettingsInternalOrganisationIrpfRateSettings(validation);
            }
        }
    }

    public static class InternalOrganisationAccountingSettingsInternalOrganisationIrpfRateSettingsRuleExtensions
    {
        public static void DeriveInternalOrganisationAccountingSettingsInternalOrganisationIrpfRateSettings(this Organisation @this, IValidation validation)
        {
            if (@this.Country.IsoCode == "ES")
            {
                foreach (IrpfRegime irpfRegime in new IrpfRegimes(@this.Transaction()).Extent())
                {
                    foreach (var irpfRate in irpfRegime.IrpfRates)
                    {
                        if (!@this.SettingsForAccounting.SettingsForIrpfRates.Any(v => v.IrpfRate.Equals(irpfRate)))
                        {
                            @this.SettingsForAccounting.AddSettingsForIrpfRate(new InternalOrganisationIrpfRateSettingsBuilder(@this.Transaction())
                                .WithIrpfRate(irpfRate)
                                .Build());
                        }
                    }
                }
            }
        }
    }
}
