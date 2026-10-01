// <copyright file="IdentityPaths.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    public static class IdentityPaths
    {
        // The Identity pages that check a password or start an account recovery: the paths to pass to
        // AddAllorsRateLimiting when the application limits sign-in attempts.
        public static string[] Authentication => new[]
        {
            "/Identity/Account/Login",
            "/Identity/Account/ForgotPassword",
            "/Identity/Account/ResetPassword",
        };
    }
}
