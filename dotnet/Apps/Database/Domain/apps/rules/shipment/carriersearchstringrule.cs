// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

    public class CarrierSearchStringRule : Rule
    {
        public CarrierSearchStringRule(MetaPopulation m) : base(m, new Guid("bf501999-1d53-4b56-b77f-e98325b8f0de")) =>
            this.Patterns = new Pattern[]
        {
            m.Carrier.RolePattern(v => v.Name),
            m.Carrier.AssociationPattern(v => v.ShipmentsWhereCarrier),
            m.Carrier.AssociationPattern(v => v.StoresWhereDefaultCarrier),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Carrier>())
            {
                var array = new string[] {
                    @this.Name,
                    @this.ExistShipmentsWhereCarrier ? string.Join(" ", @this.ShipmentsWhereCarrier?.Select(v => v.ShipmentNumber ?? string.Empty).ToArray()) : null,
                    @this.ExistStoresWhereDefaultCarrier ? string.Join(" ", @this.StoresWhereDefaultCarrier?.Select(v => v.Name ?? string.Empty).ToArray()) : null,
                };

                if (array.Any(s => !string.IsNullOrEmpty(s)))
                {
                    @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
                }
            }
        }
    }
}
