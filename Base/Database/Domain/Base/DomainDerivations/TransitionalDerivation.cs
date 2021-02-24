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

    public class TransitionalDeniedPermissionDerivation : DomainDerivation
    {
        public TransitionalDeniedPermissionDerivation(M m) : base(m, new Guid("5affa463-9365-4916-89ef-cfc18d41b4fb")) =>
            this.Patterns = new[]
            {
                new AssociationPattern(m.Transitional.ObjectStates) ,
                new AssociationPattern(m.ObjectState.DeniedPermissions) { Steps = new []{ m.ObjectState.TransitionalsWhereObjectState} },
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
