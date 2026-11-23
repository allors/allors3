// <copyright file="StartupTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Server.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Xunit;

    public class StartupTests
    {
        [Theory]
        [InlineData(null, "localhost:6182", HttpStatusCode.Unauthorized)]
        [InlineData(null, "unlisted.example", HttpStatusCode.BadRequest)]
        [InlineData(" ; ", "localhost:6182", HttpStatusCode.Unauthorized)]
        [InlineData(" ; ", "unlisted.example", HttpStatusCode.BadRequest)]
        [InlineData("sample.example", "localhost:5000", HttpStatusCode.BadRequest)]
        public async Task TheServerChecksAllowedHostsBeforeAuthentication(string allowedHosts, string authority, HttpStatusCode status)
        {
            using var factory = new ServerFactory(new Dictionary<string, string>
            {
                ["adapter"] = "snapshot",
                ["JwtToken:Key"] = ApiTest.Key,
                ["AllowedHosts"] = allowedHosts,
            });
            using var client = factory.CreateClient();
            using var response = await client.PostAsJsonAsync($"http://{authority}/allors/permission", new object());

            Assert.Equal(status, response.StatusCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("0123456789012345678901234567890")]
        public void TheServerDoesNotStartWithAMissingOrShortSigningKey(string key)
        {
            using var factory = new ServerFactory(new Dictionary<string, string>
            {
                ["adapter"] = "snapshot",
                ["JwtToken:Key"] = key,
            });

            var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());
            Assert.Contains("'JwtToken:Key'", exception.Message);
        }
    }
}
