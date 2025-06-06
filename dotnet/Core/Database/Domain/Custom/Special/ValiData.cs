// <copyright file="Organisation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Person type.</summary>

namespace Allors.Database.Domain
{
    public partial class ValiData
    {
        public void CustomOnPostDerive(ObjectOnPostDerive objectOnPostDerive)
        {
            var derivation = objectOnPostDerive.Derivation;

            derivation.Validation.AssertIsUnique(derivation.ChangeSet, this, this.Meta, this.M.ValiData.ValueA, this.M.ValiData.ValueB);
        }
    }
}
