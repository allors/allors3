// <copyright file="IPolicyService.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System.Security.Claims;

    public interface IClaimsPrincipalService
    {
        ClaimsPrincipal User { get; set; }
    }
}
