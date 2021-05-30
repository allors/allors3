// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations.Rules;
    using Meta;

    public class TransitionalDeniedPermissionRule : Rule
    {
        public TransitionalDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("5affa463-9365-4916-89ef-cfc18d41b4fb")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Transitional, m.Transitional.ObjectStates) ,
                m.ObjectState.RolePattern(v=>v.DeniedPermissions, v=> v.TransitionalsWhereObjectState),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Transitional>())
            {
                @this.TransitionalDeniedPermissions = @this.ObjectStates.SelectMany(v => v.DeniedPermissions).ToArray();
            }
        }
    }
}
