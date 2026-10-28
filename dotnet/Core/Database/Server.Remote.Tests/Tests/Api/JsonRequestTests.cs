// <copyright file="JsonRequestTests.cs" company="Allors bv">
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
    public class JsonRequestTests : ApiTest
    {
        [Theory]
        [InlineData("pull", "{\"p\":null}", "p")]
        [InlineData("pull", "{\"p\":{}}", "p")]
        [InlineData("pull", "{\"extra\":false}", "extra")]
        [InlineData("pull", "{\"extra\":[]}", "extra")]
        [InlineData("pull", "{\"l\":[{\"extra\":1}]}", "extra")]
        [InlineData("pull", "{\"d\":[{\"extra\":null}]}", "extra")]
        [InlineData("push", "{\"n\":[],\"o\":[],\"extra\":false}", "extra")]
        [InlineData("push", "{\"o\":[{\"r\":[{\"extra\":true}]}]}", "extra")]
        [InlineData("invoke", "{\"l\":[],\"extra\":[]}", "extra")]
        [InlineData("invoke", "{\"l\":[],\"o\":{\"extra\":true}}", "extra")]
        [InlineData("sync", "{\"o\":[],\"extra\":null}", "extra")]
        [InlineData("access", "{\"g\":[],\"r\":[],\"extra\":{}}", "extra")]
        [InlineData("permission", "{\"p\":[],\"extra\":1}", "extra")]
        public async Task UnknownFieldsReturnBadRequest(string endpoint, string json, string field)
        {
            await this.SignIn(this.Administrator);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await this.HttpClient.PostAsync(new Uri(endpoint, UriKind.Relative), content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Contains(field, problem.RootElement.GetProperty("errors").ToString());
        }

        [Theory]
        [InlineData("g", "[null]", 0)]
        [InlineData("r", "[null]", 0)]
        [InlineData("g", "[[]]", 0)]
        [InlineData("r", "[[]]", 0)]
        [InlineData("g", "[[1]]", 0)]
        [InlineData("r", "[[1]]", 0)]
        [InlineData("g", "[[1,2,3]]", 0)]
        [InlineData("r", "[[1,2,3]]", 0)]
        [InlineData("g", "[[0,2]]", 0)]
        [InlineData("r", "[[0,2]]", 0)]
        [InlineData("g", "[[-1,2]]", 0)]
        [InlineData("r", "[[-1,2]]", 0)]
        [InlineData("g", "[[1,-1]]", 0)]
        [InlineData("r", "[[1,-1]]", 0)]
        [InlineData("g", "[[1,0],[2]]", 1)]
        [InlineData("r", "[[1,0],[2]]", 1)]
        public async Task InvalidSyncCacheEntriesReturnBadRequest(string field, string entries, int index)
        {
            await this.SignIn(this.Administrator);
            using var content = new StringContent($"{{\"o\":[],\"{field}\":{entries}}}", Encoding.UTF8, "application/json");
            using var response = await this.HttpClient.PostAsync(new Uri("sync", UriKind.Relative), content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var errors = problem.RootElement.GetProperty("errors").GetProperty($"{field}[{index}]").ToString();
            Assert.Contains("[id, version]", errors);
            Assert.Contains("positive ID", errors);
            Assert.Contains("non-negative version", errors);
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

        [Theory]
        [InlineData("push", "{\"n\":[],\"o\":[]}", "hasErrors", JsonValueKind.False)]
        [InlineData("invoke", "{\"l\":[],\"o\":{\"i\":false,\"c\":false}}", "hasErrors", JsonValueKind.False)]
        [InlineData("sync", "{\"o\":[]}", "o", JsonValueKind.Array)]
        [InlineData("sync", "{\"o\":[],\"g\":null,\"r\":null}", "o", JsonValueKind.Array)]
        [InlineData("sync", "{\"o\":[],\"g\":[],\"r\":[]}", "o", JsonValueKind.Array)]
        [InlineData("sync", "{\"o\":[],\"g\":[[1,0],[1,0]],\"r\":[[2,0],[2,0]]}", "o", JsonValueKind.Array)]
        [InlineData("sync", "{\"o\":[],\"g\":[[1,0],[1,1],[1,2]],\"r\":[[2,0],[2,1],[2,2]]}", "o", JsonValueKind.Array)]
        [InlineData("access", "{\"g\":[],\"r\":[]}", "g", JsonValueKind.Null)]
        [InlineData("permission", "{\"p\":[]}", "p", JsonValueKind.Null)]
        public async Task EmptyProtocolOperationsRemainValid(string endpoint, string json, string responseField, JsonValueKind responseKind)
        {
            await this.SignIn(this.Administrator);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await this.HttpClient.PostAsync(new Uri(endpoint, UriKind.Relative), content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var field = result.RootElement.GetProperty(responseField);
            Assert.Equal(responseKind, field.ValueKind);
            if (responseKind == JsonValueKind.Array)
            {
                Assert.Empty(field.EnumerateArray());
            }
        }
    }
}
