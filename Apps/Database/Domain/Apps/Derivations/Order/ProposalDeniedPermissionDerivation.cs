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

    public class ProposalDeniedPermissionDerivation : DomainDerivation
    {
        public ProposalDeniedPermissionDerivation(M m) : base(m, new Guid("bbb213db-af36-4686-8e6c-2555b21c4d8c")) =>
            this.Patterns = new Pattern[]
        {
            new AssociationPattern(this.M.Proposal.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Proposal>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta.ObjectType, @this.Meta.Delete);
                if (@this.IsDeletable)
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }
            }
        }
    }
}
