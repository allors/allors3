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

    public class RequestForInformationRequestItemsRule : Rule
    {
        public RequestForInformationRequestItemsRule(MetaPopulation m) : base(m, new Guid("1731d3e7-efac-41ae-8720-df07ee23f03b")) =>
            this.Patterns = new[]
            {
                m.RequestForInformation.RolePattern(v => v.RequestItems)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RequestForInformation>())
            {
                foreach (RequestItem requestItem in @this.RequestItems)
                {
                    requestItem.Sync(@this);
                }
            }
        }
    }
}
