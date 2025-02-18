// <copyright file="PolicyService.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System.Security.Claims;

    public class ClaimsPrincipalService : IClaimsPrincipalService
    {
        public ClaimsPrincipal User { get; set; }
    }
}
