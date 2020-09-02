// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public class AgreementTermDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("2F28CF03-571A-4F7B-B71C-D8ACEBC734AC");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.AgreementTerm.Interface),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var agreementTerm in matches.Cast<AgreementTerm>())
            {
                cycle.Validation.AssertAtLeastOne(agreementTerm, M.AgreementTerm.TermType, M.AgreementTerm.Description);
            }
        }
    }
}
