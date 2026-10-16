// <copyright file="ReadValidation.InvokeRead.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    using System;
    using System.Collections.Generic;

    public static partial class ReadValidation
    {
        internal static void InvokeRequest(object value)
        {
            var request = Map(value, "$request", "v i sc x l refresh");
            Scope(Required(request, "sc", "$request"), "$request.sc");
            Request(ReadRequestForPush(request, "read", null), "$request");
            Check(Required(request, "refresh", "$request") is bool, "$request.refresh", "Supply the required refresh boolean.");
            var versions = new Dictionary<long, long>();
            Each(List(Required(request, "l", "$request"), "$request.l"), "$request.l", (item, path) =>
            {
                var invocation = Map(item, path, "i v m");
                var id = Id(Required(invocation, "i", path), path + ".i");
                var version = Integer(Required(invocation, "v", path), path + ".v", 1, MaximumInteger);
                Text(Required(invocation, "m", path), path + ".m");
                Check(!versions.TryGetValue(id, out var previous) || previous == version, path + ".v", "Use the same original version for every invocation of one target.");
                versions[id] = version;
            });
        }

        internal static void InvokeReceipt(object value, object original)
        {
            InvokeRequest(original);
            var request = (IDictionary<string, object>)original;
            var response = Map(value, "$response", "i sc invoke changed");
            Check(Text(Required(response, "i", "$response"), "$response.i") == (string)request["i"], "$response.i", "Echo the original request correlation ID.");
            var outcome = Map(Required(response, "invoke", "$response"), "$response.invoke", "k e _e _v _a _m _d");
            var kind = Choice(Required(outcome, "k", "$response.invoke"), "$response.invoke.k", "committed", "rejected", "unknown");
            if (kind == "committed") Required(response, "sc", "$response");
            Optional(response, "sc", "$response", (item, path) =>
            {
                var scope = Scope(item, path);
                var expected = (IDictionary<string, object>)request["sc"];
                foreach (var field in Fields("p u w r x"))
                {
                    Check(field == "u" ? Id(scope[field], path + ".u") == Id(expected[field], "$request.sc.u") : Equals(Value(scope, field), Value(expected, field)), path + "." + field, "Keep the exact scope admitted for this write in every supplied receipt scope.");
                }
            });
            var hasDiagnostics = false;
            Optional(outcome, "e", "$response.invoke", (items, path) =>
            {
                var errors = List(items, path);
                Check(errors.Count > 0, path, "Supply nonempty diagnostics or omit them.");
                hasDiagnostics = true;
                Each(errors, path, (item, itemPath) =>
                {
                    var error = Map(item, itemPath, "c m o i");
                    Text(Required(error, "c", itemPath), itemPath + ".c");
                    Text(Required(error, "m", itemPath), itemPath + ".m");
                    Optional(error, "o", itemPath, (ids, idsPath) => IdSet(List(ids, idsPath), idsPath, false));
                    Check(Value(error, "i") == null, itemPath + ".i", "Mutation diagnostics have no query selection index.");
                });
            });
            Optional(outcome, "_e", "$response.invoke", (item, path) => { Text(item, path); Check(kind != "committed", path, "A committed outcome cannot also report legacy Invoke failures."); hasDiagnostics = true; });
            foreach (var field in Fields("_v _a _m"))
            {
                Optional(outcome, field, "$response.invoke", (items, path) =>
                {
                    hasDiagnostics |= List(items, path).Count > 0;
                    Check(kind != "committed" || List(items, path).Count == 0, path, "A committed outcome cannot also report legacy Invoke failures.");
                    IdSet(List(items, path), path, false);
                });
            }
            Optional(outcome, "_d", "$response.invoke", (items, path) =>
            {
                hasDiagnostics |= List(items, path).Count > 0;
                Check(kind != "committed" || List(items, path).Count == 0, path, "A committed outcome cannot also report legacy Invoke failures.");
                Each(List(items, path), path, (item, itemPath) =>
                {
                    var error = Map(item, itemPath, "m r");
                    Optional(error, "m", itemPath, (message, messagePath) => Check(message is string, messagePath, "Supply a derivation message string, null, or omit it."));
                    var relationsPath = itemPath + ".r";
                    Each(List(Required(error, "r", itemPath), relationsPath), relationsPath, (relation, relationPath) =>
                    {
                        var entry = Map(relation, relationPath, "i r");
                        Integer(Required(entry, "i", relationPath), relationPath + ".i", -MaximumInteger, MaximumInteger);
                        Check(Integer(entry["i"], relationPath + ".i", -MaximumInteger, MaximumInteger) != 0, relationPath + ".i", "Use a database or workspace ID, never zero.");
                        Text(Required(entry, "r", relationPath), relationPath + ".r");
                    });
                });
            });
            Check(kind == "committed" || hasDiagnostics, "$response.invoke", "Explain a rejected or unknown mutation outcome with actionable diagnostics.");
        }

        internal static void InvokeResponse(object value, object original)
        {
            InvokeReceipt(value, original);
            var response = (IDictionary<string, object>)value;
            var request = (IDictionary<string, object>)original;
            var kind = (string)((IDictionary<string, object>)response["invoke"])["k"];
            var requested = kind == "committed" && Equals(request["refresh"], true);
            Check(requested == (Value(response, "changed") != null), "$response.changed", "Return the requested affected refresh exactly once after commitment, and no other read payload.");
            if (!requested) return;
            var read = Map(response["changed"], "$response.changed", null);
            var readRequest = ReadRequestForPush(request, "read", null);
            if (Equals(Value(read, "k"), "error")) ValidateResponse(read, readRequest);
            else AffectedResponse(read, readRequest);
        }
    }
}
