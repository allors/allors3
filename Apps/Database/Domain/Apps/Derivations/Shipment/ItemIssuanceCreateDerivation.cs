// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class ItemIssuanceCreateDerivation : DomainDerivation
    {
        public ItemIssuanceCreateDerivation(M m) : base(m, new Guid("5634abf7-615a-46bb-a67d-c7c61e12e297")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.ItemIssuance.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<ItemIssuance>())
            {
                if (!@this.ExistIssuanceDateTime)
                {
                    @this.IssuanceDateTime = @this.Strategy.Session.Now();
                }
            }
        }
    }
}
