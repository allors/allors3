// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class ProductQuoteApprovalParticipantsRule : Rule
    {
        public ProductQuoteApprovalParticipantsRule(MetaPopulation m) : base(m, new Guid("be3fae87-8d8b-478d-950e-ae9eb659a644")) =>
            this.Patterns = new Pattern[]
            {
                m.ProductQuoteApproval.RolePattern(v => v.DateClosed),
                m.ProductQuoteApproval.RolePattern(v => v.ProductQuote),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<ProductQuoteApproval>())
            {
                var participants = @this.ExistDateClosed
                                       ? (IEnumerable<Person>)Array.Empty<Person>()
                                       : new UserGroups(@this.Transaction()).Administrators.Members.Select(v => (Person)v).ToArray();
                @this.AssignParticipants(participants);
            }
        }
    }
}
