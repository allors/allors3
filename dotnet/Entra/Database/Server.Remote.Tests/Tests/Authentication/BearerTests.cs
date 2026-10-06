// <copyright file="BearerTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Database.Domain;
    using Xunit;

    // A client calls the Allors API with an access token of the tenant. Microsoft.Identity.Web
    // validates it as it validates Microsoft's: the fake issues the tokens Microsoft would, and on
    // request the tokens Microsoft never would.
    [Collection("Api")]
    public class BearerTests : ApiTest
    {
        [Fact]
        public async Task APersonsTokenCreatesThePersonAtItsFirstRequest()
        {
            var token = await this.PersonTokenAsync(FakeEntraAccounts.Tester);

            var response = await this.HttpClient.SendAsync(Bearer(HttpMethod.Get, "/allors/UserInfo", token));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var userInfo = await JsonAsync(response);
            var user = this.FindUser(FakeEntraAccounts.Tester);
            Assert.IsType<Person>(user);
            Assert.Equal(user.Id.ToString(), userInfo.GetProperty("u").GetString());
            Assert.Equal(FakeEntraAccounts.Tester.UserName, userInfo.GetProperty("userName").GetString());
            Assert.Equal(FakeEntraAccounts.Tester.DisplayName, user.EntraDisplayName);
        }

        // A program is a user too, of the class the application's factory chooses, and has no profile.
        [Fact]
        public async Task AProgramsTokenCreatesAnAgent()
        {
            var token = await this.ProgramTokenAsync();

            var response = await this.HttpClient.SendAsync(Bearer(HttpMethod.Get, "/allors/UserInfo", token));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var user = this.FindUser(FakeEntraAccounts.Program);
            Assert.IsType<Agent>(user);
            Assert.False(user.ExistEntraUserName);
            Assert.False(user.ExistEntraDisplayName);
            Assert.False(user.EntraIsGuest);
        }

        // An API registration that does not ask for v2.0 tokens gets v1.0 tokens: another issuer, the
        // api:// audience, upn instead of preferred_username. Microsoft.Identity.Web takes both.
        [Fact]
        public async Task AV1TokenIsAcceptedToo()
        {
            var token = await this.PersonTokenAsync(FakeEntraAccounts.Tester, ("x_token_version", "1"));

            var response = await this.HttpClient.SendAsync(Bearer(HttpMethod.Get, "/allors/UserInfo", token));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var user = this.FindUser(FakeEntraAccounts.Tester);
            Assert.Equal(FakeEntraAccounts.Tester.UserName, user.EntraUserName);
            Assert.Equal(FakeEntra.IssuerV1(this.TenantId), user.EntraIdentityProvider);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task AV1TokenReturnsTheUserName(bool alreadyAdmitted)
        {
            string existingUserId = null;
            if (alreadyAdmitted)
            {
                var firstToken = await this.PersonTokenAsync(FakeEntraAccounts.Tester);
                var firstResponse = await this.HttpClient.SendAsync(Bearer(HttpMethod.Get, "/allors/UserInfo", firstToken));
                Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
                existingUserId = (await JsonAsync(firstResponse)).GetProperty("u").GetString();
            }

            var token = await this.PersonTokenAsync(FakeEntraAccounts.Tester, ("x_token_version", "1"));

            var response = await this.HttpClient.SendAsync(Bearer(HttpMethod.Get, "/allors/UserInfo", token));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var userInfo = await JsonAsync(response);
            var user = this.FindUser(FakeEntraAccounts.Tester);
            Assert.IsType<Person>(user);
            Assert.Equal(user.Id.ToString(), userInfo.GetProperty("u").GetString());
            Assert.Equal(FakeEntraAccounts.Tester.UserName, userInfo.GetProperty("userName").GetString());
            Assert.Single(this.AllUsers());
            if (alreadyAdmitted)
            {
                Assert.Equal(existingUserId, userInfo.GetProperty("u").GetString());
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task AV1GuestWithoutUpnReturnsTheUserName(bool alreadyAdmitted)
        {
            string existingUserId = null;
            if (alreadyAdmitted)
            {
                var firstToken = await this.PersonTokenAsync(FakeEntraAccounts.Guest);
                var firstResponse = await this.HttpClient.SendAsync(Bearer(HttpMethod.Get, "/allors/UserInfo", firstToken));
                Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
                existingUserId = (await JsonAsync(firstResponse)).GetProperty("u").GetString();
            }

            var token = await this.PersonTokenAsync(FakeEntraAccounts.Guest, ("x_token_version", "1"));
            var claims = new Microsoft.IdentityModel.JsonWebTokens.JsonWebToken(token).Claims.ToArray();
            Assert.DoesNotContain(claims, v => v.Type == "upn" || v.Type == "preferred_username");
            Assert.Contains(claims, v => v.Type == "unique_name" && v.Value == FakeEntraAccounts.Guest.UserName);

            var response = await this.HttpClient.SendAsync(Bearer(HttpMethod.Get, "/allors/UserInfo", token));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var userInfo = await JsonAsync(response);
            var user = this.FindUser(FakeEntraAccounts.Guest);
            Assert.IsType<Person>(user);
            Assert.True(user.EntraIsGuest);
            Assert.Equal(user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), userInfo.GetProperty("u").GetString());
            Assert.Equal(FakeEntraAccounts.Guest.UserName, userInfo.GetProperty("userName").GetString());
            Assert.Equal(FakeEntraAccounts.Guest.UserName, user.EntraUserName);
            Assert.Single(this.AllUsers());
            if (alreadyAdmitted)
            {
                Assert.Equal(existingUserId, userInfo.GetProperty("u").GetString());
            }
        }

        [Fact]
        public async Task TheSameTokenFindsTheSameUserAcrossParallelRequests()
        {
            var token = await this.PersonTokenAsync(FakeEntraAccounts.Tester);
            var firstResponse = await this.HttpClient.SendAsync(Bearer(HttpMethod.Get, "/allors/UserInfo", token));
            Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
            var firstUserId = (await JsonAsync(firstResponse)).GetProperty("u").GetString();

            var responses = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => this.HttpClient.SendAsync(Bearer(HttpMethod.Get, "/allors/UserInfo", token))));

            Assert.All(responses, v => Assert.Equal(HttpStatusCode.OK, v.StatusCode));
            var ids = await Task.WhenAll(responses.Select(async v => (await JsonAsync(v)).GetProperty("u").GetString()));
            Assert.Single(ids.Distinct());
            Assert.All(ids, v => Assert.Equal(firstUserId, v));
            Assert.Single(this.AllUsers());
        }

        // Deletion removes the local user, but the application's factory still owns admission.
        // A valid token may create another user when that factory continues to admit the identity.
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ADeletedUserMayBeReadmittedWithTheSameBearerToken(bool application)
        {
            var account = application ? FakeEntraAccounts.Program : FakeEntraAccounts.Tester;
            var token = application ? await this.ProgramTokenAsync() : await this.PersonTokenAsync(account);
            Assert.Equal(HttpStatusCode.OK, (await this.HttpClient.SendAsync(Bearer(HttpMethod.Get, "/allors/UserInfo", token))).StatusCode);
            var user = this.FindUser(account);
            var deletedUserId = user.Id;
            user.Delete();
            this.Transaction.Derive();
            this.Transaction.Commit();
            Assert.Null(this.FindUser(account));

            var response = await this.HttpClient.SendAsync(Bearer(HttpMethod.Get, "/allors/UserInfo", token));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var replacement = this.FindUser(account);
            Assert.NotEqual(deletedUserId, replacement.Id);
            Assert.Equal(replacement.Id.ToString(), (await JsonAsync(response)).GetProperty("u").GetString());
            if (application)
            {
                Assert.IsType<Agent>(replacement);
            }
            else
            {
                Assert.IsType<Person>(replacement);
            }

            Assert.Single(this.AllUsers());
        }

        [Fact]
        public async Task AnExistingSessionResolvesAUserReadmittedBeforeItsNextRequest()
        {
            var (browser, jar) = NewBrowser();
            Assert.Equal(HttpStatusCode.OK, (await SignInAsync(browser, FakeEntraAccounts.Tester)).StatusCode);
            var sessionCookie = Cookie(jar, "Allors.Auth");
            var token = await this.PersonTokenAsync(FakeEntraAccounts.Tester);
            var user = this.FindUser(FakeEntraAccounts.Tester);
            var deletedUserId = user.Id;
            user.Delete();
            this.Transaction.Derive();
            this.Transaction.Commit();
            var admitted = await this.HttpClient.SendAsync(Bearer(HttpMethod.Get, "/allors/UserInfo", token));
            Assert.Equal(HttpStatusCode.OK, admitted.StatusCode);
            var replacementId = (await JsonAsync(admitted)).GetProperty("u").GetString();
            Assert.NotEqual(deletedUserId.ToString(), replacementId);

            var response = await browser.GetAsync("allors/UserInfo");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(replacementId, (await JsonAsync(response)).GetProperty("u").GetString());
            Assert.Equal(sessionCookie, Cookie(jar, "Allors.Auth"));
            Assert.Single(this.AllUsers());
        }

        // The tokens Microsoft never issues for the tenant, each refused by Microsoft.Identity.Web's
        // validation before the plug-in sees a principal: no user is created for any of them.
        [Theory]
        [InlineData("another tenant's issuer", "tenant")]
        [InlineData("the tenant's issuer with another tid", "x_tid")]
        [InlineData("a key the tenant's documents do not list", "x_key")]
        [InlineData("an expired token", "x_expires_in")]
        [InlineData("the api:// audience on a v2.0 token", "x_aud")]
        [InlineData("a tampered signature", "tamper")]
        public async Task ATokenMicrosoftWouldNotIssueIs401(string what, string knob)
        {
            var otherTenant = Guid.NewGuid();
            var token = knob switch
            {
                "tenant" => await this.PersonTokenAsync(otherTenant, FakeEntraAccounts.Tester),
                "x_tid" => await this.PersonTokenAsync(FakeEntraAccounts.Tester, ("x_tid", otherTenant.ToString())),
                "x_key" => await this.PersonTokenAsync(FakeEntraAccounts.Tester, ("x_key", "unknown")),
                "x_expires_in" => await this.PersonTokenAsync(FakeEntraAccounts.Tester, ("x_expires_in", "-600")),
                "x_aud" => await this.PersonTokenAsync(FakeEntraAccounts.Tester, ("x_aud", "api://" + this.ClientId)),
                _ => Tamper(await this.PersonTokenAsync(FakeEntraAccounts.Tester)),
            };

            var response = await this.HttpClient.SendAsync(Bearer(HttpMethod.Get, "/allors/UserInfo", token));

            Assert.True(HttpStatusCode.Unauthorized == response.StatusCode, $"{what} should be refused with 401; was {(int)response.StatusCode}.");
            Assert.Contains(response.Headers.WwwAuthenticate, v => v.Scheme == "Bearer" && v.Parameter?.Contains("invalid_token", StringComparison.Ordinal) == true);
            Assert.Null(this.FindUser(FakeEntraAccounts.Tester));
        }

        // A token of a person the application does not admit is valid, and still gets no user.
        [Fact]
        public async Task ARefusedPersonsTokenIs401WithoutAUser()
        {
            var token = await this.PersonTokenAsync(FakeEntraAccounts.Refused);

            var response = await this.HttpClient.SendAsync(Bearer(HttpMethod.Get, "/allors/UserInfo", token));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Null(this.FindUser(FakeEntraAccounts.Refused));
        }

        private static string Tamper(string token)
        {
            var signature = token[(token.LastIndexOf('.') + 1)..];
            var flipped = signature[0] == 'A' ? 'B' : 'A';
            return token[..(token.LastIndexOf('.') + 1)] + flipped + signature[1..];
        }
    }
}
