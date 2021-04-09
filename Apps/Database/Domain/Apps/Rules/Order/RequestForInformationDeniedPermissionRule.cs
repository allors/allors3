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

    public class RequestForInformationDeniedPermissionRule : Rule
    {
        public RequestForInformationDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("3227f658-588b-42eb-bf4f-f76d1d4b85c4")) =>
            this.Patterns = new Pattern[]
        {
            m.RequestForInformation.RolePattern(v => v.TransitionalDeniedPermissions),
            m.RequestItem.RolePattern(v => v.RequestItemState, v => v.RequestWhereRequestItem, m.RequestForInformation),
            m.RequestItem.RolePattern(v => v.RequestItemState, v => v.RequestWhereRequestItem, m.RequestForInformation),
            m.Request.AssociationPattern(v => v.QuoteWhereRequest, m.RequestForInformation),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RequestForInformation>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Delete);
                if (@this.IsDeletable())
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
