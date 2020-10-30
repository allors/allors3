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

    public class CreationDerivation : DomainDerivation
    {
        public CreationDerivation(M m) : base(m, new Guid("0f357e77-22f2-4389-9f6e-9ede99a7e2dc")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(m.AA.Class),
            };


        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var aa in matches.Cast<AA>())
            {
                if (!aa.IsCreated.HasValue)
                {
                    aa.IsCreated = true;
                }
            }
        }
    }
}
