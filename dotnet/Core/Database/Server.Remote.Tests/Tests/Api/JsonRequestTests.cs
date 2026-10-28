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
        [InlineData("sync", "{\"pull\":{},\"o\":[],\"extra\":null}", "extra")]
        [InlineData("sync", "{\"pull\":{\"extra\":null}}", "extra")]
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
        [InlineData("sync", "{\"pull\":{},\"o\":[]}", "o", JsonValueKind.Array)]
        [InlineData("sync", "{\"pull\":{},\"o\":[]}", "g", JsonValueKind.Array)]
        [InlineData("sync", "{\"pull\":{},\"o\":[]}", "r", JsonValueKind.Array)]
        [InlineData("sync", "{\"pull\":{},\"o\":[]}", "p", JsonValueKind.Array)]
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

        [Theory]
        [InlineData("{}", "original Pull")]
        [InlineData("{\"pull\":{},\"g\":[[1,1],[1,2]]}", "one entry per ID")]
        [InlineData("{\"pull\":{},\"r\":[[1]]}", "[id, version]")]
        [InlineData("{\"pull\":{},\"o\":[{\"i\":1,\"v\":1,\"g\":[2]}]}", "security cache table")]
        public async Task InvalidSyncCacheHintsReturnBadRequest(string json, string message)
        {
            await this.SignIn(this.Administrator);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await this.HttpClient.PostAsync(new Uri("sync", UriKind.Relative), content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Contains(message, result.RootElement.GetProperty("_e").GetString());
        }
    }
}
