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
    using Resources;

    public class StatementOfWorkDerivation : DomainDerivation
    {
        public StatementOfWorkDerivation(M m) : base(m, new Guid("8307B027-0A59-409F-B47C-B2B2C98267C8")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.StatementOfWork.QuoteItems),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;


            foreach (var @this in matches.Cast<StatementOfWork>())
            {
                if (@this.ExistCurrentVersion
                    && @this.CurrentVersion.ExistIssuer
                    && @this.Issuer != @this.CurrentVersion.Issuer)
                {
                    validation.AddError($"{@this} {this.M.StatementOfWork.Issuer} {ErrorMessages.InternalOrganisationChanged}");
                }

                Sync(@this);
            }

            void Sync(StatementOfWork statementOfWork)
            {
                // session.Prefetch(this.SyncPrefetch, this);
                foreach (QuoteItem quoteItem in statementOfWork.QuoteItems)
                {
                    quoteItem.Sync(statementOfWork);
                }
            }
        }
    }
}
