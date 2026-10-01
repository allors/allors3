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

        // The 2FA sign-in pages are [AllowAnonymous] in the default UI, so the 404 is observable
        // without a cookie.
        [Theory]
        [InlineData("Identity/Account/LoginWith2fa")]
        [InlineData("Identity/Account/LoginWithRecoveryCode")]
        public async Task TwoFactorLoginPagesAreDisabled(string path)
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                BaseAddress = new Uri("http://localhost:5000/"),
            };

            var response = await client.GetAsync(new Uri(path, UriKind.Relative));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Manage pages sit behind the authenticated-user fallback policy (anonymous callers are
        // redirected to login before the page filter runs), so the 404 needs a signed-in cookie.
        [Theory]
        [InlineData("Identity/Account/Manage/EnableAuthenticator")]
        [InlineData("Identity/Account/Manage/ResetAuthenticator")]
        [InlineData("Identity/Account/Manage/GenerateRecoveryCodes")]
        [InlineData("Identity/Account/Manage/ShowRecoveryCodes")]
        [InlineData("Identity/Account/Manage/Disable2fa")]
        public async Task TwoFactorManagePagesAreDisabledForSignedInUsers(string path)
        {
            var client = await this.SignInWithCookieAsync("cookieuser", "cookie-password");

            var response = await client.GetAsync(new Uri(path, UriKind.Relative));

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
