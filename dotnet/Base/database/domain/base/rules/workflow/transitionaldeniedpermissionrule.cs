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
    using Derivations.Rules;
    using Meta;

    public class TransitionalDeniedPermissionRule : Rule
    {
        public TransitionalDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("5affa463-9365-4916-89ef-cfc18d41b4fb")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Transitional, m.Transitional.ObjectStates) ,
                m.ObjectState.RolePattern(v=>v.Revocations, v=> v.TransitionalsWhereObjectState),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Transitional>())
            {
                @this.DeriveTransitionalDeniedPermission(validation);
            }
        }
    }

    public static class TransitionalDeniedPermissionRuleExtensions
    {
        public static void DeriveTransitionalDeniedPermission(this Transitional @this, IValidation validation)
        {
            @this.TransitionalRevocations = @this.ObjectStates.Select(v => v.ObjectRevocation).ToArray();
        }
    }
}
