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
    using Resources;

    public class SupplierRelationshipFromDateRule : Rule
    {
        public SupplierRelationshipFromDateRule(MetaPopulation m) : base(m, new Guid("0cfa305e-ab90-40a8-8cef-3c3c5d755de5")) =>
        this.Patterns = new Pattern[]
        {
            m.SupplierRelationship.RolePattern(v => v.FromDate),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SupplierRelationship>())
            {
                @this.DeriveSupplierRelationshipFromDate(validation);
            }
        }
    }

    public static class SupplierRelationshipFromDateRuleExtensions
    {
        public static void DeriveSupplierRelationshipFromDate(this SupplierRelationship @this, IValidation validation)
        {
            if (@this.Supplier.SupplierRelationshipsWhereSupplier.Except(new[] { @this })
                .FirstOrDefault(v => v.InternalOrganisation.Equals(@this.InternalOrganisation)
                                    && v.FromDate.Date <= @this.FromDate.Date
                                    && (!v.ExistThroughDate || v.ThroughDate.Value.Date >= @this.FromDate.Date))
                != null)
            {
                validation.AddError(@this, @this.Meta.FromDate, ErrorMessages.PeriodActive);
            }

            if (@this.Supplier.SupplierRelationshipsWhereSupplier.Except(new[] { @this })
                .FirstOrDefault(v => v.InternalOrganisation.Equals(@this.InternalOrganisation)
                                    && @this.FromDate.Date <= v.FromDate.Date
                                    && (!@this.ExistThroughDate || @this.ThroughDate.Value.Date >= v.FromDate.Date))
                != null)
            {
                validation.AddError(@this, @this.Meta.FromDate, ErrorMessages.PeriodActive);
            }
        }
    }
}
