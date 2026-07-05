// <copyright file="SecurityHeaderTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("Api")]
    public class SecurityHeaderTests : ApiTest
    {
        [Fact]
        public async Task ResponsesCarryBaselineSecurityHeaders()
        {
            var response = await this.HttpClient.GetAsync(new Uri("Test/Ready", UriKind.Relative));

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
            Assert.Equal("no-referrer", Assert.Single(response.Headers.GetValues("Referrer-Policy")));
            Assert.True(response.Headers.Contains("Permissions-Policy"), "Permissions-Policy header is missing");
            Assert.True(response.Headers.Contains("Content-Security-Policy"), "Content-Security-Policy header is missing");
        }
    }
}
