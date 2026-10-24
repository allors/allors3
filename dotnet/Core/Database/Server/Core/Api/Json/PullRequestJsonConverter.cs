// <copyright file="PullRequestJsonConverter.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Allors.Protocol.Json.Api.Pull;

    public sealed class PullRequestJsonConverter : JsonConverter<PullRequest>
    {
        private readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> strictOptions = new();

        public override PullRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            JsonSerializer.Deserialize<PullRequest>(ref reader, this.Options(options));

        public override void Write(Utf8JsonWriter writer, PullRequest value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, value, this.Options(options));

        private JsonSerializerOptions Options(JsonSerializerOptions options) => this.strictOptions.GetValue(options, source =>
        {
            // Keep the application's JSON settings while rejecting unknown members throughout the
            // typed Pull request. Dictionary keys and untyped argument values remain application data.
            var strict = new JsonSerializerOptions(source)
            {
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            };

            for (var i = strict.Converters.Count - 1; i >= 0; --i)
            {
                if (strict.Converters[i] is PullRequestJsonConverter)
                {
                    strict.Converters.RemoveAt(i);
                }
            }

            return strict;
        });
    }
}
