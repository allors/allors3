// <copyright file="EntraDefaults.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    // The names the Entra plug-in registers its schemes under, and the configuration section it reads.
    // An application that registers Microsoft.Identity.Web itself may use other names and tell the
    // plug-in with AddAllorsEntraUsers.
    public static class EntraDefaults
    {
        // The cookie that keeps a browser signed in: the browser session that Core hardens.
        public const string SessionScheme = "Entra.Session";

        // The OpenID Connect scheme that signs a browser in with Microsoft Entra ID.
        public const string OpenIdConnectScheme = "Entra.OpenIdConnect";

        // The JWT bearer scheme that authenticates a request with an access token of the tenant.
        public const string BearerScheme = "Entra.Bearer";

        // The configuration section: Microsoft.Identity.Web's keys (Instance, TenantId, ClientId,
        // ClientSecret, ...) and the plug-in's own SessionLifetime.
        public const string ConfigurationSection = "Entra";

        // The authority of the public cloud, used when the section names no Instance.
        public const string Instance = "https://login.microsoftonline.com/";

        // A session ends this long after its sign-in, however often it was renewed, unless the
        // section says otherwise in SessionLifetime: Entra cannot end the application's session.
        public static readonly System.TimeSpan SessionLifetime = System.TimeSpan.FromHours(12);
    }
}
