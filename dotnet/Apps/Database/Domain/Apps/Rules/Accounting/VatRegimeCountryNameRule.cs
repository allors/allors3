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

    public class VatRegimeCountryNameRule : Rule
    {
        public VatRegimeCountryNameRule(MetaPopulation m) : base(m, new Guid("bef42373-7c6d-4f71-b6f6-1535e684d7f1")) =>
            this.Patterns = new Pattern[]
            {
                m.VatRegime.RolePattern(v => v.Country),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<VatRegime>())
            {
                @this.DeriveVatRegimeCountryName(validation);
            }
        }
    }

    public static class VatRegimeCountryNameNameRuleExtensions
    {
        public static void DeriveVatRegimeCountryName(this VatRegime @this, IValidation validation) => @this.CountryName = @this.Country?.Name;
    }
}
