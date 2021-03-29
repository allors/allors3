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
    using Resources;

    public class RequestForProposalRule : Rule
    {
        public RequestForProposalRule(M m) : base(m, new Guid("E2C5250C-5C18-4720-BBFE-859AC31D8D49")) =>
            this.Patterns = new[]
            {
                new RolePattern(m.RequestForProposal, m.RequestForProposal.Recipient),
                new RolePattern(m.RequestForProposal, m.RequestForProposal.RequestItems)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RequestForProposal>())
            {
                if (@this.ExistCurrentVersion
                   && @this.CurrentVersion.ExistRecipient
                   && @this.Recipient != @this.CurrentVersion.Recipient)
                {
                    validation.AddError($"{@this} {this.M.RequestForProposal.Recipient} {ErrorMessages.InternalOrganisationChanged}");
                }

                foreach (RequestItem requestItem in @this.RequestItems)
                {
                    requestItem.Sync(@this);
                }
            }
        }
    }
}
