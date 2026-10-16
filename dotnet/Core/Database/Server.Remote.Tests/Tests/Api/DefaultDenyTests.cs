// <copyright file="DefaultDenyTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Allors.Database.Domain;
    using Xunit;

    [Collection("Api")]
    public class DefaultDenyTests : ApiTest
    {
        // The JSON API endpoints carry [Authorize] themselves, so an anonymous request is denied
        // whatever fallback policy the application sets.
        [Theory]
        [InlineData("pull")]
        [InlineData("push")]
        [InlineData("sync")]
        [InlineData("invoke")]
        [InlineData("access")]
        [InlineData("permission")]
        public async Task AnonymousJsonApiIsDenied(string endpoint)
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                BaseAddress = new Uri(Url),
            };

            var response = await client.PostAsync(new Uri(endpoint, UriKind.Relative), null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("remove-c1-string-read-grant")]
        [InlineData("add-denied-property-read-revocation")]
        [InlineData("remove-denied-property-read-revocation")]
        public async Task AnonymousPermissionFixtureActionsAreDenied(string action)
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = new Uri(Url) };
            using var response = await client.PostAsync(new Uri($"test/permissions/{action}?targetId=0", UriKind.Relative), null);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AnonymousReadApiIsDenied()
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = new Uri(Url) };
            using var response = await client.PostAsync(new Uri("read", UriKind.Relative), null);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AnonymousPushReadApiIsDenied()
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = new Uri(Url) };
            using var response = await client.PostAsync(new Uri("push-read", UriKind.Relative), null);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AuthenticatedPushReadApiUsesStrictRawDecoding()
        {
            await this.SignIn(this.Administrator);
            using var content = new StringContent("{\"v\":1,\"v\":1,\"i\":\"push-read\",\"push\":{}}", System.Text.Encoding.UTF8, "application/json");
            using var response = await this.HttpClient.PostAsync(new Uri("push-read", UriKind.Relative), content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PushReadApiReturnsCommittedReceiptAndBothRequestedReads()
        {
            var user = this.Administrator;
            await this.SignIn(user);
            var read = new Allors.Protocol.Json.Api.Read.ReadRequest
            {
                v = 1, i = "bind", k = "read",
                p = new Allors.Protocol.Json.Api.Read.ReadPull
                {
                    l = Array.Empty<Allors.Protocol.Json.Data.Pull>(),
                    d = Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>(),
                },
            };
            using var bound = await this.PostAsJsonAsync(new Uri("read", UriKind.Relative), read);
            bound.EnsureSuccessStatusCode();
            var scope = Assert.IsType<Allors.Protocol.Json.Api.Read.ReadSuccess>(Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeResponse(await bound.Content.ReadAsStringAsync(), read)).sc;
            var request = new Allors.Protocol.Json.Api.PushRead.PushReadRequest
            {
                v = 1, i = "save", sc = scope, returnChanged = true, discover = read.p,
                push = new Allors.Protocol.Json.Api.PushRead.PushReadChanges
                {
                    o = new[] { new Allors.Protocol.Json.Api.Push.PushRequestObject { d = user.Id, v = user.Strategy.ObjectVersion, r = Array.Empty<Allors.Protocol.Json.Api.Push.PushRequestRole>() } },
                },
            };
            using var saved = await this.PostAsJsonAsync(new Uri("push-read", UriKind.Relative), request);
            saved.EnsureSuccessStatusCode();
            var response = Allors.Protocol.Json.SystemTextJson.PushReadJson.DeserializeResponse(await saved.Content.ReadAsStringAsync(), request);
            Assert.Equal("committed", response.push.k);
            Assert.Contains(Assert.IsType<Allors.Protocol.Json.Api.PushRead.PushReadChanged>(response.changed).p, value => value.i == user.Id);
            Assert.IsType<Allors.Protocol.Json.Api.Read.DiscoverySuccess>(response.discovery);
        }

        [Theory]
        [InlineData(false, 0, HttpStatusCode.BadRequest)]
        [InlineData(true, 0, HttpStatusCode.BadRequest)]
        [InlineData(false, 1, HttpStatusCode.RequestEntityTooLarge)]
        [InlineData(true, 1, HttpStatusCode.RequestEntityTooLarge)]
        public async Task PushReadApiEnforcesBodyLimitWithOrWithoutContentLength(bool chunked, int extraBytes, HttpStatusCode expected)
        {
            await this.SignIn(this.Administrator);
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("push-read", UriKind.Relative))
            {
                Content = new StringContent(new string(' ', 1024 * 1024 + extraBytes)),
            };
            request.Headers.TransferEncodingChunked = chunked;
            using var response = await this.HttpClient.SendAsync(request);
            Assert.Equal(expected, response.StatusCode);
        }

        [Fact]
        public async Task AuthenticatedReadApiUsesStrictRawDecoding()
        {
            await this.SignIn(this.Administrator);
            using var content = new StringContent("{\"v\":1,\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}", System.Text.Encoding.UTF8, "application/json");
            using var response = await this.HttpClient.PostAsync(new Uri("read", UriKind.Relative), content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(false, 0, HttpStatusCode.BadRequest)]
        [InlineData(true, 0, HttpStatusCode.BadRequest)]
        [InlineData(false, 1, HttpStatusCode.RequestEntityTooLarge)]
        [InlineData(true, 1, HttpStatusCode.RequestEntityTooLarge)]
        public async Task ReadApiEnforcesBodyLimitWithOrWithoutContentLength(bool chunked, int extraBytes, HttpStatusCode expected)
        {
            await this.SignIn(this.Administrator);
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("read", UriKind.Relative))
            {
                // Whitespace at the limit reaches JSON validation; one byte more must receive 413.
                Content = new StringContent(new string(' ', 1024 * 1024 + extraBytes)),
            };
            request.Headers.TransferEncodingChunked = chunked;
            using var response = await this.HttpClient.SendAsync(request);
            Assert.Equal(expected, response.StatusCode);
        }

        [Fact]
        public async Task AnonymousInvokeReadApiIsDenied()
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = new Uri(Url) };
            using var response = await client.PostAsync(new Uri("invoke-read", UriKind.Relative), null);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AuthenticatedInvokeReadApiUsesStrictRawDecoding()
        {
            await this.SignIn(this.Administrator);
            using var content = new StringContent("{\"v\":1,\"v\":1,\"i\":\"invoke-read\",\"l\":[],\"refresh\":true}", System.Text.Encoding.UTF8, "application/json");
            using var response = await this.HttpClient.PostAsync(new Uri("invoke-read", UriKind.Relative), content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task InvokeReadApiReturnsCommittedReceiptAndAffectedObjects()
        {
            var target = new Allors.Database.Domain.OrganisationBuilder(this.Transaction).WithName("HTTP Invoke target").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            await this.SignIn(this.Administrator);
            var read = new Allors.Protocol.Json.Api.Read.ReadRequest
            {
                v = 1, i = "invoke-bind", k = "read",
                p = new Allors.Protocol.Json.Api.Read.ReadPull
                {
                    l = Array.Empty<Allors.Protocol.Json.Data.Pull>(),
                    d = Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>(),
                },
            };
            using var bound = await this.PostAsJsonAsync(new Uri("read", UriKind.Relative), read);
            bound.EnsureSuccessStatusCode();
            var scope = Assert.IsType<Allors.Protocol.Json.Api.Read.ReadSuccess>(Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeResponse(await bound.Content.ReadAsStringAsync(), read)).sc;
            var request = new Allors.Protocol.Json.Api.InvokeRead.InvokeReadRequest
            {
                v = 1, i = "invoke-committed", sc = scope, refresh = true,
                l = new[] { new Allors.Protocol.Json.Api.Invoke.Invocation { i = target.Id, v = target.Strategy.ObjectVersion, m = this.M.Organisation.JustDoIt.Tag } },
            };

            using var invoked = await this.PostAsJsonAsync(new Uri("invoke-read", UriKind.Relative), request);

            Assert.Equal(HttpStatusCode.OK, invoked.StatusCode);
            var raw = await invoked.Content.ReadAsStringAsync();
            var response = Allors.Protocol.Json.SystemTextJson.InvokeReadJson.DeserializeResponse(raw, request);
            Assert.Equal("committed", response.invoke.k);
            var changed = Assert.IsType<Allors.Protocol.Json.Api.PushRead.PushReadChanged>(response.changed);
            var record = Assert.Single(changed.p, value => value.i == target.Id);
            Assert.Null(record.p.r);
            Assert.Equal("true", System.Text.Json.JsonSerializer.Serialize(Assert.Single(record.p.ro, value => value.t == this.M.Organisation.JustDidIt.RelationType.Tag).v));
            Assert.Equal("true", System.Text.Json.JsonSerializer.Serialize(Assert.Single(record.p.ro, value => value.t == this.M.Organisation.JustDidItDerived.RelationType.Tag).v));
            using var document = System.Text.Json.JsonDocument.Parse(raw);
            Assert.False(document.RootElement.TryGetProperty("discovery", out _));
            Assert.False(document.RootElement.GetProperty("invoke").TryGetProperty("n", out _));
            using var verification = this.Transaction.Database.CreateTransaction();
            var persisted = (Allors.Database.Domain.Organisation)verification.Instantiate(target.Id);
            Assert.True(persisted.JustDidIt);
            Assert.True(persisted.JustDidItDerived);
        }

        [Theory]
        [InlineData(false, 0, HttpStatusCode.BadRequest)]
        [InlineData(true, 0, HttpStatusCode.BadRequest)]
        [InlineData(false, 1, HttpStatusCode.RequestEntityTooLarge)]
        [InlineData(true, 1, HttpStatusCode.RequestEntityTooLarge)]
        public async Task InvokeReadApiEnforcesBodyLimitWithOrWithoutContentLength(bool chunked, int extraBytes, HttpStatusCode expected)
        {
            await this.SignIn(this.Administrator);
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("invoke-read", UriKind.Relative))
            {
                Content = new StringContent(new string(' ', 1024 * 1024 + extraBytes)),
            };
            request.Headers.TransferEncodingChunked = chunked;
            using var response = await this.HttpClient.SendAsync(request);
            Assert.Equal(expected, response.StatusCode);
        }

        // The test-harness bootstrap/setup endpoints opt out of the fallback ([AllowAnonymous]) so Nuke, the
        // e2e runner, jest and the remote suites can reset/populate/sign in without a credential.
        [Fact]
        public async Task AnonymousTestEndpointStaysAccessible()
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                BaseAddress = new Uri(Url),
            };

            var response = await client.GetAsync(new Uri("Test/Ready", UriKind.Relative));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // An authenticated (bearer, during the dual-scheme window) request passes the fallback: the
        // policy authorizes it, so it is never rejected with 401.
        [Fact]
        public async Task AuthenticatedJsonApiIsAuthorized()
        {
            await this.SignIn(this.Administrator);

            var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            var response = await this.HttpClient.PostAsync(new Uri("pull", UriKind.Relative), content);

            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
