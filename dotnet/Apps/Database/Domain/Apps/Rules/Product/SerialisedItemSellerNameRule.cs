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

    public class SerialisedItemSellerNameRule : Rule
    {
        public SerialisedItemSellerNameRule(MetaPopulation m) : base(m, new Guid("4a4e4dee-d03e-4ff1-b97a-29ac913528c0")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.Seller),
                m.InternalOrganisation.RolePattern(v => v.DisplayName, v => v.SerialisedItemsWhereSeller.ObjectType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveSerialisedItemSellerName(validation);
            }
        }
    }

    public static class SerialisedItemSellerNameRuleExtensions
    {
        public static void DeriveSerialisedItemSellerName(this SerialisedItem @this, IValidation validation) => @this.SellerName = @this.Seller?.DisplayName;
    }
}
