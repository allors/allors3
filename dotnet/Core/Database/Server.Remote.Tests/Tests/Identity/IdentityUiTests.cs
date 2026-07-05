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
    using Database.Domain;
    using Xunit;

    [Collection("Api")]
    public class IdentityUiTests : ApiTest
    {
        public IdentityUiTests()
        {
            var user = new PersonBuilder(this.Transaction).WithUserName("cookieuser").Build();
            user.SetPassword("cookie-password");

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        [Fact]
        public async Task LoginPageRendersAUserNameField()
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                BaseAddress = new Uri("http://localhost:5000/"),
            };

            var response = await client.GetAsync(new Uri("Identity/Account/Login", UriKind.Relative));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Input.UserName", body);
            Assert.DoesNotContain("Input.Email", body);
        }

        [Fact]
        public async Task RegisterPageIsDisabled()
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                BaseAddress = new Uri("http://localhost:5000/"),
            };

            var response = await client.GetAsync(new Uri("Identity/Account/Register", UriKind.Relative));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CookieSignInGrantsAccessToManage()
        {
            var client = await this.SignInWithCookieAsync("cookieuser", "cookie-password");

            var response = await client.GetAsync(new Uri("Identity/Account/Manage", UriKind.Relative));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ManageWithoutCookieRedirectsToLogin()
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                BaseAddress = new Uri("http://localhost:5000/"),
            };

            var response = await client.GetAsync(new Uri("Identity/Account/Manage", UriKind.Relative));

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Identity/Account/Login", response.Headers.Location?.OriginalString ?? string.Empty);
        }
    }
}
