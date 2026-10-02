// <copyright file="RateLimitPartitionTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Net;
    using Allors.Server;
    using Microsoft.AspNetCore.Http;
    using Xunit;

    // The Identity plug-in names the paths it signs in on, and Core's rate limiting building block
    // limits them.
    public class RateLimitPartitionTests
    {
        [Fact]
        public void IdentityLoginPathIsAnAuthenticationPath()
        {
            var partition = AuthenticationRateLimitPolicy.Partition(Context("/Identity/Account/Login", "203.0.113.7"), Settings(IdentityPaths.Authentication));

            Assert.Equal("authentication:203.0.113.7", partition.PartitionKey);
        }

        private static AuthenticationRateLimitSettings Settings(params string[] paths) => new() { Paths = paths };

        private static HttpContext Context(string path, string remoteIp)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;
            context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIp);
            return context;
        }
    }
}
