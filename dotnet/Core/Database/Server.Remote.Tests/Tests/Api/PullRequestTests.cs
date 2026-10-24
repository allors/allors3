// <copyright file="PullRequestTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("Api")]
    public class PullRequestTests : ApiTest
    {
        [Theory]
        [InlineData("{\"p\":null}", "p")]
        [InlineData("{\"p\":{}}", "p")]
        [InlineData("{\"extra\":false}", "extra")]
        [InlineData("{\"extra\":[]}", "extra")]
        [InlineData("{\"l\":[{\"extra\":1}]}", "extra")]
        [InlineData("{\"d\":[{\"extra\":null}]}", "extra")]
        public async Task UnknownFieldsReturnBadRequest(string json, string field)
        {
            await this.SignIn(this.Administrator);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await this.HttpClient.PostAsync(new Uri("pull", UriKind.Relative), content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Contains(field, problem.RootElement.GetProperty("errors").ToString());
        }

        [Theory]
        [InlineData("{}")]
        [InlineData("{\"l\":[],\"d\":[],\"x\":\"request-id\"}")]
        public async Task OrdinaryRequestsRemainValid(string json)
        {
            await this.SignIn(this.Administrator);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await this.HttpClient.PostAsync(new Uri("pull", UriKind.Relative), content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(JsonValueKind.Array, result.RootElement.GetProperty("p").ValueKind);
        }
    }
}
