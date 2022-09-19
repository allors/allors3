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

    public class SerialisedItemNameRule : Rule
    {
        public SerialisedItemNameRule(MetaPopulation m) : base(m, new Guid("3d6cf84b-b2af-4a0f-b2d9-a5b9f991f2cb")) =>
            this.Patterns = new Pattern[]
            {
                //m.SerialisedItem.RolePattern(v => v.Name),
                //m.SerialisedItem.RolePattern(v => v.SerialNumber),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                //if (!@this.ExistName && @this.ExistSerialNumber)
                //{
                //    @this.Name = @this.SerialNumber;
                //}
            }
        }
    }
}
