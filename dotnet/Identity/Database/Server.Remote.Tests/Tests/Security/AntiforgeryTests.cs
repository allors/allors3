// <copyright file="AntiforgeryTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Allors.Protocol.Json.Api.Read;
    using Allors.Protocol.Json.SystemTextJson;
    using Database.Domain;
    using Xunit;

    // Core's antiforgery protects the Allors API against the cookie that the Identity plug-in signs a
    // browser in with.
    [Collection("Api")]
    public class AntiforgeryTests : ApiTest
    {
        private const string ReadRequestJson = "{\"v\":1,\"i\":\"named-route-security\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}";

        public AntiforgeryTests()
        {
            var user = new PersonBuilder(this.Transaction).WithUserName("xsrfuser").Build();
            user.SetPassword("xsrf-password");

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        [Fact]
        public async Task CookiePostWithoutXsrfHeaderReturns400()
        {
            // The cookie client's base address is the site root (for /Identity), so /allors is explicit here.
            var client = await this.SignInWithCookieAsync("xsrfuser", "xsrf-password");
            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));

            var response = await client.PostAsync(new Uri("allors/read", UriKind.Relative), CreateReadContent());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            await AssertAntiforgeryFailureAsync(response);
        }

        [Fact]
        public async Task CookiePostWithXsrfHeaderSucceeds()
        {
            var client = await this.SignInWithCookieAsync("xsrfuser", "xsrf-password");

            var safeResponse = await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));
            var xsrf = ExtractSetCookieValue(safeResponse, "XSRF-TOKEN");

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("allors/read", UriKind.Relative)) { Content = CreateReadContent() };
            request.Headers.Add("X-XSRF-TOKEN", xsrf);
            var response = await client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode, $"Cookie POST with the XSRF header should succeed; was {(int)response.StatusCode}.");
            await this.AssertCompleteReadAsync(response);
        }

        [Fact]
        public async Task CookiePostAfterAnonymousFirstVisitSucceeds()
        {
            var (client, jar) = CreateCookieClient();

            // The SPA's bootstrap GET mints a token while still anonymous; the token the browser
            // holds after the login redirect must validate for the now-authenticated user.
            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));
            await LoginWithCookieAsync(client, "xsrfuser", "xsrf-password");
            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));

            var xsrf = JarXsrfToken(jar);
            Assert.False(string.IsNullOrEmpty(xsrf), "A readable XSRF-TOKEN cookie should be present after login.");

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("allors/read", UriKind.Relative)) { Content = CreateReadContent() };
            request.Headers.Add("X-XSRF-TOKEN", xsrf);
            var response = await client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode, $"Cookie POST after an anonymous first visit should succeed; was {(int)response.StatusCode}.");
            await this.AssertCompleteReadAsync(response);
        }

        [Fact]
        public async Task LogoutAndReloginRotateTheXsrfToken()
        {
            var (client, jar) = CreateCookieClient();

            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));
            await LoginWithCookieAsync(client, "xsrfuser", "xsrf-password");
            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));

            // The Angular LogoutService shape: a POST with the XSRF header and no form body.
            var logout = new HttpRequestMessage(HttpMethod.Post, new Uri("Identity/Account/Logout", UriKind.Relative));
            logout.Headers.Add("X-XSRF-TOKEN", JarXsrfToken(jar));
            var logoutResponse = await client.SendAsync(logout);
            Assert.Equal(HttpStatusCode.Redirect, logoutResponse.StatusCode);

            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));
            await LoginWithCookieAsync(client, "xsrfuser", "xsrf-password");
            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("allors/read", UriKind.Relative)) { Content = CreateReadContent() };
            request.Headers.Add("X-XSRF-TOKEN", JarXsrfToken(jar));
            var response = await client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode, $"Cookie POST after logout and re-login should succeed; was {(int)response.StatusCode}.");
            await this.AssertCompleteReadAsync(response);
        }

        [Fact]
        public async Task CorruptedXsrfTokenSelfHeals()
        {
            var (client, jar) = CreateCookieClient();

            await LoginWithCookieAsync(client, "xsrfuser", "xsrf-password");
            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));

            // A token that no longer validates (corruption, data-protection key loss) must not
            // wedge the session: the rejecting 400 drops it and the next safe GET re-mints.
            jar.Add(new Cookie("XSRF-TOKEN", "garbage", "/", "localhost"));
            var poisoned = new HttpRequestMessage(HttpMethod.Post, new Uri("allors/read", UriKind.Relative)) { Content = CreateReadContent() };
            poisoned.Headers.Add("X-XSRF-TOKEN", "garbage");
            var poisonedResponse = await client.SendAsync(poisoned);
            Assert.Equal(HttpStatusCode.BadRequest, poisonedResponse.StatusCode);
            await AssertAntiforgeryFailureAsync(poisonedResponse);

            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("allors/read", UriKind.Relative)) { Content = CreateReadContent() };
            request.Headers.Add("X-XSRF-TOKEN", JarXsrfToken(jar));
            var response = await client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode, $"Cookie POST after antiforgery self-heal should succeed; was {(int)response.StatusCode}.");
            await this.AssertCompleteReadAsync(response);
        }

        [Fact]
        public async Task NamedWorkspaceQueryRouteIsGone()
        {
            var client = await this.SignInWithCookieAsync("xsrfuser", "xsrf-password");
            var safeResponse = await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));
            var xsrf = ExtractSetCookieValue(safeResponse, "XSRF-TOKEN");

            var readRequest = new HttpRequestMessage(HttpMethod.Post, new Uri("allors/read", UriKind.Relative)) { Content = CreateReadContent() };
            readRequest.Headers.Add("X-XSRF-TOKEN", xsrf);
            var readResponse = await client.SendAsync(readRequest);
            await this.AssertCompleteReadAsync(readResponse);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("allors/Organisations/Pull", UriKind.Relative));
            request.Headers.Add("X-XSRF-TOKEN", xsrf);
            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task CookieInvokeRequiresAntiforgeryAndRunsProtectedBatch(bool includeHeader)
        {
            using var client = await this.SignInWithCookieAsync("xsrfuser", "xsrf-password");
            using var safeResponse = await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));
            var xsrf = ExtractSetCookieValue(safeResponse, "XSRF-TOKEN");
            using var bootstrap = new HttpRequestMessage(HttpMethod.Post, new Uri("allors/read", UriKind.Relative)) { Content = CreateReadContent() };
            bootstrap.Headers.Add("X-XSRF-TOKEN", xsrf);
            using var bound = await client.SendAsync(bootstrap);
            await this.AssertCompleteReadAsync(bound);
            var scope = Assert.IsType<ReadSuccess>(ReadJson.DeserializeResponse(await bound.Content.ReadAsStringAsync(), ReadJson.DeserializeRequest(ReadRequestJson))).sc;
            var invocation = new Allors.Protocol.Json.Api.InvokeRead.InvokeReadRequest
            {
                v = 1, i = "cookie-invoke", sc = scope, l = Array.Empty<Allors.Protocol.Json.Api.Invoke.Invocation>(), refresh = true,
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("allors/invoke-read", UriKind.Relative))
            {
                Content = new StringContent(JsonSerializer.Serialize(invocation), Encoding.UTF8, "application/json"),
            };
            if (includeHeader) request.Headers.Add("X-XSRF-TOKEN", xsrf);

            using var response = await client.SendAsync(request);

            Assert.Equal(includeHeader ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.StatusCode);
            if (!includeHeader) await AssertAntiforgeryFailureAsync(response);
            else
            {
                var result = InvokeReadJson.DeserializeResponse(await response.Content.ReadAsStringAsync(), invocation);
                Assert.Equal("committed", result.invoke.k);
                Assert.Equal(scope.u, result.sc.u);
                var changed = Assert.IsType<Allors.Protocol.Json.Api.PushRead.PushReadChanged>(result.changed);
                Assert.Empty(changed.p);
                Assert.Empty(changed.u);
                Assert.Empty(changed.s);
                Assert.Empty(changed.pm);
            }
        }

        private static StringContent CreateReadContent() => new StringContent(ReadRequestJson, Encoding.UTF8, "application/json");

        private static async Task AssertAntiforgeryFailureAsync(HttpResponseMessage response)
        {
            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("antiforgery", body.RootElement.GetProperty("error").GetString());
        }

        private async Task AssertCompleteReadAsync(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = Assert.IsType<ReadSuccess>(ReadJson.DeserializeResponse(await response.Content.ReadAsStringAsync(), ReadJson.DeserializeRequest(ReadRequestJson)));
            Assert.Equal("named-route-security", result.i);
            Assert.Equal("complete", result.k);
            Assert.Equal("Default", result.sc.w);
            Assert.Equal("identity-read-v1", result.sc.r);
            Assert.Equal(new Users(this.Transaction).FindBy(this.M.User.UserName, "xsrfuser").Id, result.sc.u);
            Assert.Empty(result.l);
            Assert.Empty(result.o);
            Assert.Empty(result.c);
            Assert.Empty(result.p);
        }

        private static (HttpClient Client, CookieContainer Jar) CreateCookieClient()
        {
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer(),
                AllowAutoRedirect = false,
            };
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5000/") };
            return (client, handler.CookieContainer);
        }

        // The jar is what Angular's XSRF interceptor sees (document.cookie), unlike a single
        // response's Set-Cookie header.
        private static string JarXsrfToken(CookieContainer jar) =>
            jar.GetCookies(new Uri("http://localhost:5000/"))["XSRF-TOKEN"]?.Value;

        private static string ExtractSetCookieValue(HttpResponseMessage response, string name)
        {
            var setCookies = response.Headers.TryGetValues("Set-Cookie", out var values) ? values : Enumerable.Empty<string>();
            var cookie = setCookies.FirstOrDefault(v => v.StartsWith(name + "=", StringComparison.Ordinal));
            return cookie == null ? null : Regex.Match(cookie, name + "=([^;]+)").Groups[1].Value;
        }
    }
}
