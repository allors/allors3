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

    public class ModelDeniedPermissionRule : Rule
    {
        public ModelDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("9654dfc9-5f56-439b-a0a8-33d966bad55d")) =>
            this.Patterns = new Pattern[]
        {
            m.Model.AssociationPattern(v => v.BrandWhereModel, m.Model),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Model>())
            {
                @this.DeriveModelDeniedPermission(validation);
            }
        }
    }

    public static class ModelDeniedPermissionRuleExtensions
    {
        public static void DeriveModelDeniedPermission(this Model @this, IValidation validation)
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
