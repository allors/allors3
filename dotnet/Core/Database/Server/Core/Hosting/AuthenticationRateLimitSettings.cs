// <copyright file="AuthenticationRateLimitSettings.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using Microsoft.Extensions.Configuration;

    public class AuthenticationRateLimitSettings
    {
        public string[] Paths { get; set; } =
        {
            "/allors/Authentication/Token",
            "/allors/TestAuthentication/Token",
            "/Identity/Account/Login",
            "/Identity/Account/ForgotPassword",
            "/Identity/Account/ResetPassword",
        };

        public int PermitLimit { get; set; } = 10;

        public int WindowSeconds { get; set; } = 60;

        public int LoopbackPermitLimit { get; set; } = 1000;

        public static AuthenticationRateLimitSettings From(IConfiguration configuration)
        {
            var settings = new AuthenticationRateLimitSettings();
            configuration.GetSection("Security:AuthenticationRateLimit").Bind(settings);
            return settings;
        }
    }
}
