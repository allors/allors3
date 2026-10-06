// <copyright file="IAllorsSessionValidator.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication.Cookies;

    // Authentication plug-ins participate inside Core's protected cookie callback, so application
    // configuration cannot replace their validation while leaving Core's other rules intact.
    internal interface IAllorsSessionValidator
    {
        Task ValidateAsync(CookieValidatePrincipalContext context);
    }
}
