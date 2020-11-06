// <copyright file="PartyFinancialRelationshipCreationDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class PaymentExtensionsCreateDerivation : DomainDerivation
    {
        public PaymentExtensionsCreateDerivation(M m) : base(m, new Guid("1756425d-17a3-4765-a79e-cd4a6037059c")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(m.Payment.Interface)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Payment>())
            {
                if (!@this.ExistEffectiveDate)
                {
                    @this.EffectiveDate = @this.Strategy.Session.Now().Date;
                }
            }
        }
    }
}
