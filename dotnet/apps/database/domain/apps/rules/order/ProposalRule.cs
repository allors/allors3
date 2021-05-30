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
    using Resources;

    public class ProposalRule : Rule
    {
        public ProposalRule(MetaPopulation m) : base(m, new Guid("F51A25BD-3FB7-4539-A541-5F19F124AA9F")) =>
            this.Patterns = new[]
            {
                m.Proposal.RolePattern(v => v.Issuer)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Proposal>())
            {
                if (@this.ExistCurrentVersion
                     && @this.CurrentVersion.ExistIssuer
                     && @this.Issuer != @this.CurrentVersion.Issuer)
                {
                    validation.AddError($"{@this} {this.M.Proposal.Issuer} {ErrorMessages.InternalOrganisationChanged}");
                }
            }
        }
    }
}
