// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class RequestForProposalDeniedPermissionDerivation : DomainDerivation
    {
        public RequestForProposalDeniedPermissionDerivation(M m) : base(m, new Guid("1eb65d1c-7164-4c98-aef5-47c3e96f26d7")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.RequestForProposal.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RequestForProposal>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}
