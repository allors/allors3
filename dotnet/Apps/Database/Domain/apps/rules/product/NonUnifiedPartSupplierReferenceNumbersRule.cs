// <copyright file="NonUnifiedPartBrandDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Meta;
    using Database.Derivations;
    using Resources;
    using Allors.Database.Domain.Derivations.Rules;

    public class NonUnifiedPartSupplierReferenceNumbersRule : Rule
    {
        public NonUnifiedPartSupplierReferenceNumbersRule(MetaPopulation m) : base(m, new Guid("a071aa46-d14b-45ac-b4ab-e51f400baec8")) =>
            this.Patterns = new Pattern[]
            {
                m.Part.RolePattern(v => v.SuppliedBy),
                m.SupplierOffering.RolePattern(v => v.SupplierProductId, v => v.Part.Part, m.NonUnifiedPart),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonUnifiedPart>())
            {
                @this.DeriveNonUnifiedPartSupplierReferenceNumbers(validation);
            }
        }
    }

    public static class NonUnifiedPartSupplierReferenceNumbersRuleExtensions
    {
        public static void DeriveNonUnifiedPartSupplierReferenceNumbers(this NonUnifiedPart @this, IValidation validation)
        {
            var array = new string[] {
                    string.Join(", ", @this.SupplierOfferingsWherePart?.Select((v) => v.SupplierProductId ?? string.Empty).ToArray())
                };

            if (array.Any(s => !string.IsNullOrEmpty(s)))
            {
                @this.SupplierReferenceNumbers = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
            else
            {
                @this.RemoveSupplierReferenceNumbers();
            }
        }
    }
}
