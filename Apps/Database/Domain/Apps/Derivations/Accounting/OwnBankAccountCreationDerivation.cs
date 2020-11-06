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

    public class OwnBankAccountCreationDerivation : DomainDerivation
    {
        public OwnBankAccountCreationDerivation(M m) : base(m, new Guid("a7eae078-ea0a-4f50-846e-0ae44ea9d60e")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(m.OwnBankAccount.Class)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<OwnBankAccount>())
            {
                if (!@this.ExistIsActive)
                {
                    @this.IsActive = true;
                }
            }
        }
    }
}
