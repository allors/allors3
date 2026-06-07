// <copyright file="EmbeddedMediaContent.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class EmbeddedMediaContent
    {
        // Data is a persisted role; it satisfies MediaContent.Data directly.

        public bool HasData => this.ExistData && this.Data.Length > 0;

        public void CoreOnPostDerive(ObjectOnPostDerive method)
        {
            if (!this.HasData)
            {
                method.Derivation.Validation.AddError(this, this.Meta.Data, "Empty data");
            }
        }
    }
}
