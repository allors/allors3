// <copyright file="PolicyService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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
