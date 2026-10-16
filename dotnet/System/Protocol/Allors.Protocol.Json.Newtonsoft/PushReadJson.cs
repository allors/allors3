// <copyright file="PushReadJson.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Newtonsoft
{
    using System;
    using System.Collections.Generic;
    using Allors.Protocol.Json.Api.PushRead;
    using Allors.Protocol.Json.Api.Read;
    using global::Newtonsoft.Json;

    /// <summary>Separately decodes the mutation receipt so invalid read payloads cannot erase a known commit.</summary>
    public static class PushReadJson
    {
        public static PushReadRequest DeserializeRequest(string json)
        {
            var value = ReadJson.Parse(json);
            PushReadValidation.ValidateRequest(value);
            return ReadJson.Deserialize<PushReadRequest>(value);
        }

        public static PushReadReceipt DeserializeReceipt(string json, PushReadRequest request)
        {
            var value = ReadJson.ParseReceipt(json);
            PushReadValidation.ValidateReceipt(value, Original(request));
            return ReadJson.Deserialize<PushReadReceipt>(Receipt(value));
        }

        public static PushReadResponse DeserializeResponse(string json, PushReadRequest request)
        {
            var value = ReadJson.Parse(json);
            PushReadValidation.ValidateResponse(value, Original(request));
            var receipt = ReadJson.Deserialize<PushReadReceipt>(Receipt(value));
            var map = (IDictionary<string, object>)value;
            return new PushReadResponse
            {
                i = receipt.i, sc = receipt.sc, push = receipt.push,
                changed = Part(map, "changed"), discovery = Part(map, "discovery"),
            };
        }

        /// <summary>Decode one independent read without allowing the other payload to erase its outcome.</summary>
        public static object DeserializePart(string json, PushReadRequest request, bool changed)
        {
            var name = changed ? "changed" : "discovery";
            var response = (IDictionary<string, object>)ReadJson.ParseReceipt(json, name);
            var original = (IDictionary<string, object>)Original(request);
            PushReadValidation.ValidateReceipt(response, original);
            response.Remove(changed ? "discovery" : "changed");
            original[changed ? "discover" : "returnChanged"] = null;
            PushReadValidation.ValidateResponse(response, original);
            return Part(response, name);
        }

        private static object Original(PushReadRequest request)
        {
            try { return ReadJson.Parse(ReadJson.Serialize(request)); }
            catch (JsonException exception) { throw new FormatException("$request: Supply an acyclic PushRead request with supported JSON values.", exception); }
        }

        private static object Receipt(object value)
        {
            var map = (IDictionary<string, object>)value;
            return new Dictionary<string, object> { ["i"] = map["i"], ["sc"] = map.TryGetValue("sc", out var scope) ? scope : null, ["push"] = map["push"] };
        }

        private static object Part(IDictionary<string, object> response, string name)
        {
            if (!response.TryGetValue(name, out var value) || value == null) return null;
            var part = (IDictionary<string, object>)value;
            if ((string)part["k"] == "error") return ReadJson.Deserialize<ReadFailure>(part);
            return name == "changed" ? (object)ReadJson.Deserialize<PushReadChanged>(part) : ReadJson.Deserialize<DiscoverySuccess>(part);
        }
    }
}
