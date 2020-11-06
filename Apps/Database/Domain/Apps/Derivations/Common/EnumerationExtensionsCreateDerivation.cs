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

    public class EnumerationExtensionsCreateDerivation : DomainDerivation
    {
        public EnumerationExtensionsCreateDerivation(M m) : base(m, new Guid("fe5f8040-03db-4e55-962c-3cbca0f6715e")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(this.M.Enumeration.Interface)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Enumeration>())
            {
                if (!@this.IsActive.HasValue)
                {
                    @this.IsActive = true;
                }
            }
        }
    }
}
