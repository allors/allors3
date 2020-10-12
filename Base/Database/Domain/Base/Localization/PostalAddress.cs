// <copyright file="PostalAddress.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class PostalAddress
    {
        public bool IsPostalAddress => true;

        public void BaseOnDerive(ObjectOnDerive method)
        {
            var derivation = method.Derivation;

            derivation.Validation.AssertExistsAtMostOne(this, this.M.PostalAddress.PostalAddressBoundaries, this.M.PostalAddress.Locality);
            derivation.Validation.AssertExistsAtMostOne(this, this.M.PostalAddress.PostalAddressBoundaries, this.M.PostalAddress.Region);
            derivation.Validation.AssertExistsAtMostOne(this, this.M.PostalAddress.PostalAddressBoundaries, this.M.PostalAddress.PostalCode);
            derivation.Validation.AssertExistsAtMostOne(this, this.M.PostalAddress.PostalAddressBoundaries, this.M.PostalAddress.Country);

            if (!this.ExistPostalAddressBoundaries)
            {
                derivation.Validation.AssertExists(this, this.M.PostalAddress.Locality);
                derivation.Validation.AssertExists(this, this.M.PostalAddress.Country);
            }
        }
    }
}
