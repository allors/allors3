// <copyright file="DerviationCountedExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public static partial class DerivationCountedExtensions
    {
        public static void CustomOnPostDerive(this DerivationCounted @this, ObjectOnPostDerive _) => @this.DerivationCount += 1;
    }
}
