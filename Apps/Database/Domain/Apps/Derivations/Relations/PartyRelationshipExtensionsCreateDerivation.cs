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

    public class PartyRelationshipExtensionsCreateDerivation : DomainDerivation
    {
        public PartyRelationshipExtensionsCreateDerivation(M m) : base(m, new Guid("3e968d79-3f8f-434f-b880-ab62c5a7450e")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.PartyRelationship.Interface),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<PartyRelationship>())
            {
                if (!@this.ExistFromDate)
                {
                    @this.FromDate = @this.Strategy.Session.Now();
                }
            }
        }
    }
}
