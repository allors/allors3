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

    public class BrandDeniedPermissionRule : Rule
    {
        public BrandDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("410c211c-53ee-4d0b-8caf-4a5d19144bdc")) =>
            this.Patterns = new Pattern[]
        {
            m.Brand.AssociationPattern(v => v.PartsWhereBrand, m.Brand),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Brand>())
            {
                @this.DeriveBrandDeniedPermission(validation);
            }
        }
    }

    public static class BrandDeniedPermissionRuleExtensions
    {
        public static void DeriveBrandDeniedPermission(this Brand @this, IValidation validation)
        {
            var deleteRevocation = new Revocations(@this.Strategy.Transaction).BrandDeleteRevocation;

            if (@this.IsDeletable)
            {
                @this.RemoveRevocation(deleteRevocation);
            }
            else
            {
                @this.AddRevocation(deleteRevocation);
            }
        }
    }
}
