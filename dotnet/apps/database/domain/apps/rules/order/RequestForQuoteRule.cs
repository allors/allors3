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
    using Derivations.Rules;
    using Resources;

    public class RequestForQuoteRule : Rule
    {
        public RequestForQuoteRule(MetaPopulation m) : base(m, new Guid("BD181210-419E-4F87-8B3C-3AEF43711514")) =>
            this.Patterns = new[]
            {
                m.RequestForQuote.RolePattern(v => v.Recipient),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RequestForQuote>())
            {
                if (@this.ExistCurrentVersion
                      && @this.CurrentVersion.ExistRecipient
                      && @this.Recipient != @this.CurrentVersion.Recipient)
                {
                    validation.AddError($"{@this} {this.M.RequestForQuote.Recipient} {ErrorMessages.InternalOrganisationChanged}");
                }
            }
        }
    }
}
