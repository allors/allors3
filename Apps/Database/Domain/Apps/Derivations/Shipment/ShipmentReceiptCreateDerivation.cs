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

    public class ShipmentReceiptCreateDerivation : DomainDerivation
    {
        public ShipmentReceiptCreateDerivation(M m) : base(m, new Guid("19a7fa5f-7b12-4103-87a5-988234bed435")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.ShipmentPackage.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<ShipmentReceipt>())
            {
                if (!@this.ExistReceivedDateTime)
                {
                    @this.ReceivedDateTime = @this.Session().Now();
                }
            }
        }
    }
}
