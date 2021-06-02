// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Derivations.Rules;
    using Resources;

    public class RequestItemValidationRule : Rule
    {
        public RequestItemValidationRule(MetaPopulation m) : base(m, new Guid("b619cde6-b636-46e6-a7e6-e3355867aa53")) =>
            this.Patterns = new Pattern[]
            {
                m.RequestItem.RolePattern(v => v.Product),
                m.RequestItem.RolePattern(v => v.ProductFeature),
                m.RequestItem.RolePattern(v => v.Description),
                m.RequestItem.RolePattern(v => v.NeededSkill),
                m.RequestItem.RolePattern(v => v.Deliverable),
                m.RequestItem.RolePattern(v => v.SerialisedItem),
                m.RequestItem.RolePattern(v => v.Quantity),
                m.RequestItem.AssociationPattern(v => v.RequestWhereRequestItem),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RequestItem>())
            {
                validation.AssertAtLeastOne(@this, this.M.RequestItem.Product, this.M.RequestItem.ProductFeature, this.M.RequestItem.SerialisedItem, this.M.RequestItem.Description, this.M.RequestItem.NeededSkill, this.M.RequestItem.Deliverable);
                validation.AssertExistsAtMostOne(@this, this.M.RequestItem.Product, this.M.RequestItem.ProductFeature, this.M.RequestItem.Description, this.M.RequestItem.NeededSkill, this.M.RequestItem.Deliverable);
                validation.AssertExistsAtMostOne(@this, this.M.RequestItem.SerialisedItem, this.M.RequestItem.ProductFeature, this.M.RequestItem.Description, this.M.RequestItem.NeededSkill, this.M.RequestItem.Deliverable);

                if (@this.ExistSerialisedItem && @this.Quantity != 1)
                {
                    validation.AddError(@this, @this.Meta.Quantity, ErrorMessages.SerializedItemQuantity);
                }
            }
        }
    }
}
