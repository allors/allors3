// <copyright file="Transitional.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial interface Transitional
    {
        TransitionalConfiguration[] TransitionalConfigurations { get; }
    }
}
