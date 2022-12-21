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

    public class CustomerRelationshipFromDateRule : Rule
    {
        public CustomerRelationshipFromDateRule(MetaPopulation m) : base(m, new Guid("1fdcf1c3-f40a-44aa-bdeb-2fdeb5edf431")) =>
        this.Patterns = new Pattern[]
        {
            m.CustomerRelationship.RolePattern(v => v.FromDate),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<CustomerRelationship>())
            {
                @this.DeriveCustomerRelationshipFromDate(validation);
            }
        }
    }

    public static class CustomerRelationshipFromDateRuleExtensions
    {
        public static void DeriveCustomerRelationshipFromDate(this CustomerRelationship @this, IValidation validation)
        {
            if (@this.ExistCustomer && @this.ExistInternalOrganisation)
            {
                if (@this.Customer.CustomerRelationshipsWhereCustomer.Except(new[] { @this })
                    .FirstOrDefault(v => v.InternalOrganisation.Equals(@this.InternalOrganisation)
                                        && v.FromDate.Date <= @this.FromDate.Date
                                        && (!v.ExistThroughDate || v.ThroughDate.Value.Date >= @this.FromDate.Date))
                    != null)
                {
                    validation.AddError(@this, @this.Meta.FromDate, ErrorMessages.PeriodActive);
                }

                if (@this.Customer.CustomerRelationshipsWhereCustomer.Except(new[] { @this })
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
}
