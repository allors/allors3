// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class CommunicationEventDeniedPermissionDerivation : DomainDerivation
    {
        public CommunicationEventDeniedPermissionDerivation(M m) : base(m, new Guid("a2565a9c-cb13-43ba-bcf2-9db538e456ae")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.CommunicationEvent.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<CommunicationEvent>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}
