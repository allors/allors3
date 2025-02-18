// <copyright file="HomeAddress.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the HomeAddress type.</summary>

namespace Allors.Database.Domain
{
    /// <summary>
    /// A HomeAddress is a fysical address with a street/number and a place.
    /// </summary>
    public partial class HomeAddress
    {
        public void CustomOnPostDerive(ObjectOnPostDerive method)
        {
            var derivation = method.Derivation;

            derivation.Validation.AssertExists(this, this.M.HomeAddress.Street);
            derivation.Validation.AssertNonEmptyString(this, this.M.HomeAddress.Street);
        }
    }
}
