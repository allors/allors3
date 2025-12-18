// <copyright file="AdminApiClient.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands.Services
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    public class AdminApiClient : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;

        public AdminApiClient(string baseUrl)
        {
            this.baseUrl = baseUrl.TrimEnd('/');
            this.httpClient = new HttpClient();
            this.httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<Stream> SaveAsync()
        {
            var response = await this.httpClient.GetAsync($"{this.baseUrl}/allors/Admin/Save");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }

        public async Task LoadAsync(Stream xmlStream, string fileName)
        {
            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(xmlStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            content.Add(streamContent, "file", fileName);

            var response = await this.httpClient.PostAsync($"{this.baseUrl}/allors/Admin/Load", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task PopulateAsync()
        {
            var response = await this.httpClient.PostAsync($"{this.baseUrl}/allors/Admin/Populate", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<UpgradeResponse> UpgradeAsync(Stream xmlStream, string fileName)
        {
            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(xmlStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            content.Add(streamContent, "file", fileName);

            var response = await this.httpClient.PostAsync($"{this.baseUrl}/allors/Admin/Upgrade", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<UpgradeResponse>();
                return errorResponse ?? new UpgradeResponse
                {
                    Success = false,
                    ErrorMessage = response.ReasonPhrase,
                };
            }

            return await response.Content.ReadFromJsonAsync<UpgradeResponse>() ?? new UpgradeResponse { Success = true };
        }

        public void Dispose()
        {
            this.httpClient?.Dispose();
        }
    }
}
