// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public class PostalAddressDerivation : DomainDerivation
    {
        public PostalAddressDerivation(M m) : base(m, new Guid("412c754a-4806-40ab-ac49-b5569bb9b9a9")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.PostalAddress.Locality),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PostalAddress>())
            {
                validation.AssertExistsAtMostOne(@this, @this.M.PostalAddress.PostalAddressBoundaries, @this.M.PostalAddress.Locality);
                validation.AssertExistsAtMostOne(@this, @this.M.PostalAddress.PostalAddressBoundaries, @this.M.PostalAddress.Region);
                validation.AssertExistsAtMostOne(@this, @this.M.PostalAddress.PostalAddressBoundaries, @this.M.PostalAddress.PostalCode);
                validation.AssertExistsAtMostOne(@this, @this.M.PostalAddress.PostalAddressBoundaries, @this.M.PostalAddress.Country);

                if (!@this.ExistPostalAddressBoundaries)
                {
                    validation.AssertExists(@this, @this.M.PostalAddress.Locality);
                    validation.AssertExists(@this, @this.M.PostalAddress.Country);
                }
            }
        }
    }
}
