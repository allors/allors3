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

    public class RequestRequestItemDelegatedAccessRule : Rule
    {
        public RequestRequestItemDelegatedAccessRule(MetaPopulation m) : base(m, new Guid("1731d3e7-efac-41ae-8720-df07ee23f03b")) =>
            this.Patterns = new[]
            {
                m.Request.RolePattern(v => v.RequestItems)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Request>())
            {
                foreach (var requestItem in @this.RequestItems)
                {
                    requestItem.DelegatedAccess = @this;
                }
            }
        }
    }
}
