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

    public class InternalOrganisationAccountingSettingsInternalOrganisationVatRateSettingsRule : Rule
    {
        public InternalOrganisationAccountingSettingsInternalOrganisationVatRateSettingsRule(MetaPopulation m) : base(m, new Guid("f37da6be-78c6-4f34-b067-697b6abaa7cf")) =>
        this.Patterns = new Pattern[]
        {
            m.Country.RolePattern(v => v.DerivedVatRegimes, v => v.InternalOrganisationsWhereCountry),
            m.InternalOrganisation.RolePattern(v => v.Country),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Organisation>().Where(v => v.IsInternalOrganisation))
            {
                @this.DeriveInternalOrganisationAccountingSettingsInternalOrganisationVatRateSettings(validation);
            }
        }
    }

    public static class InternalOrganisationAccountingSettingsInternalOrganisationVatRateSettingsRuleExtensions
    {
        public static void DeriveInternalOrganisationAccountingSettingsInternalOrganisationVatRateSettings(this Organisation @this, IValidation validation)
        {
            foreach(var vatRegime in @this.Country.DerivedVatRegimes)
            {
                foreach(var vatRate in vatRegime.VatRates)
                {
                    if (!@this.SettingsForAccounting.SettingsForVatRates.Any(v => v.VatRate.Equals(vatRate)))
                    {
                        @this.SettingsForAccounting.AddSettingsForVatRate(new InternalOrganisationVatRateSettingsBuilder(@this.Transaction())
                            .WithVatRate(vatRate)
                            .Build());
                    }
                }
            }
        }
    }
}
