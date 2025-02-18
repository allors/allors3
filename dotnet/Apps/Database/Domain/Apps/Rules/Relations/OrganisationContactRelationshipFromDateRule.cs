// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class OrganisationContactRelationshipFromDateRule : Rule
    {
        public OrganisationContactRelationshipFromDateRule(MetaPopulation m) : base(m, new Guid("8f477a8b-aced-4048-b90b-e4aba328a4b7")) =>
        this.Patterns = new Pattern[]
        {
            m.OrganisationContactRelationship.RolePattern(v => v.FromDate),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<OrganisationContactRelationship>())
            {
                @this.DeriveOrganisationContactRelationshipFromDate(validation);
            }
        }
    }

    public static class OrganisationContactRelationshipFromDateRuleExtensions
    {
        public static void DeriveOrganisationContactRelationshipFromDate(this OrganisationContactRelationship @this, IValidation validation)
        {
            if (@this.ExistContact && @this.ExistOrganisation)
            {
                if (@this.Contact.OrganisationContactRelationshipsWhereContact.Except(new[] { @this })
                    .FirstOrDefault(v => v.Organisation.Equals(@this.Organisation)
                                        && v.FromDate.Date <= @this.FromDate.Date
                                        && (!v.ExistThroughDate || v.ThroughDate.Value.Date >= @this.FromDate.Date))
                    != null)
                {
                    validation.AddError(@this, @this.Meta.FromDate, ErrorMessages.PeriodActive);
                }

                if (@this.Contact.OrganisationContactRelationshipsWhereContact.Except(new[] { @this })
                    .FirstOrDefault(v => v.Organisation.Equals(@this.Organisation)
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
