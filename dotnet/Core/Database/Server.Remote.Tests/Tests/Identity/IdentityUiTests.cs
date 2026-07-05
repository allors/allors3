// <copyright file="IdentityUiTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("Api")]
    public class IdentityUiTests : ApiTest
    {
        [Fact]
        public async Task LoginPageIsServed()
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                BaseAddress = new Uri("http://localhost:5000/"),
            };

            var response = await client.GetAsync(new Uri("Identity/Account/Login", UriKind.Relative));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
