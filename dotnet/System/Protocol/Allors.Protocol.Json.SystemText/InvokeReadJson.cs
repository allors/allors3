// <copyright file="InvokeReadJson.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.SystemTextJson
{
    using System;
    using System.Collections.Generic;
    using Allors.Protocol.Json.Api.InvokeRead;
    using Allors.Protocol.Json.Api.PushRead;
    using Allors.Protocol.Json.Api.Read;
    using System.Text.Json;

    /// <summary>Decodes the receipt independently so a malformed refresh cannot erase a known commit.</summary>
    public static class InvokeReadJson
    {
        public static InvokeReadRequest DeserializeRequest(string json)
        {
            var value = ReadJson.Parse(json);
            InvokeReadValidation.ValidateRequest(value);
            return ReadJson.Deserialize<InvokeReadRequest>(value);
        }

        public static InvokeReadReceipt DeserializeReceipt(string json, InvokeReadRequest request)
        {
            var value = ReadJson.ParseReceipt(json, mutationField: "invoke");
            InvokeReadValidation.ValidateReceipt(value, Original(request));
            return ReadJson.Deserialize<InvokeReadReceipt>(Receipt(value));
        }

        public static InvokeReadResponse DeserializeResponse(string json, InvokeReadRequest request)
        {
            var value = ReadJson.Parse(json);
            InvokeReadValidation.ValidateResponse(value, Original(request));
            var receipt = ReadJson.Deserialize<InvokeReadReceipt>(Receipt(value));
            return new InvokeReadResponse { i = receipt.i, sc = receipt.sc, invoke = receipt.invoke, changed = Changed((IDictionary<string, object>)value) };
        }

        public static object DeserializeChanged(string json, InvokeReadRequest request)
        {
            var value = (IDictionary<string, object>)ReadJson.ParseReceipt(json, "changed", "invoke");
            InvokeReadValidation.ValidateResponse(value, Original(request));
            return Changed(value);
        }

        private static object Original(InvokeReadRequest request)
        {
            try { return ReadJson.Parse(JsonSerializer.Serialize(request)); }
            catch (JsonException exception) { throw new FormatException("$request: Supply an acyclic InvokeRead request with supported JSON values.", exception); }
        }

        private static object Receipt(object value)
        {
            var map = (IDictionary<string, object>)value;
            return new Dictionary<string, object> { ["i"] = map["i"], ["sc"] = map.TryGetValue("sc", out var scope) ? scope : null, ["invoke"] = map["invoke"] };
        }

        private static object Changed(IDictionary<string, object> response)
        {
            if (!response.TryGetValue("changed", out var value) || value == null) return null;
            var part = (IDictionary<string, object>)value;
            return (string)part["k"] == "error" ? (object)ReadJson.Deserialize<ReadFailure>(part) : ReadJson.Deserialize<PushReadChanged>(part);
        }
    }
}
