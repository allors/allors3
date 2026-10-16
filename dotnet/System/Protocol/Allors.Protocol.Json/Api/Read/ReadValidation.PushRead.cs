// <copyright file="ReadValidation.PushRead.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    using System;
    using System.Collections.Generic;

    public static partial class ReadValidation
    {
        internal static void PushRequest(object value)
        {
            var request = Map(value, "$request", "v i sc x push returnChanged discover");
            Scope(Required(request, "sc", "$request"), "$request.sc");
            Request(ReadRequestForPush(request, "discover", Value(request, "discover")), "$request");
            Optional(request, "returnChanged", "$request", (item, path) => Check(item is bool, path, "Supply a boolean, null, or omit this option."));
            var push = Map(Required(request, "push", "$request"), "$request.push", "n o");
            var newIds = new HashSet<long>();
            Optional(push, "n", "$request.push", (items, path) => Each(List(items, path), path, (item, itemPath) =>
            {
                var entry = Map(item, itemPath, "w t r");
                Check(newIds.Add(Integer(Required(entry, "w", itemPath), itemPath + ".w", -MaximumInteger, -1)), itemPath + ".w", "Submit each negative workspace ID once.");
                Text(Required(entry, "t", itemPath), itemPath + ".t");
            }));
            var existing = new HashSet<long>();
            Optional(push, "o", "$request.push", (items, path) => Each(List(items, path), path, (item, itemPath) =>
            {
                var entry = Map(item, itemPath, "d v r");
                Check(existing.Add(Id(Required(entry, "d", itemPath), itemPath + ".d")), itemPath + ".d", "Submit each database ID once.");
                Version(Required(entry, "v", itemPath), itemPath + ".v");
            }));
            foreach (var field in Fields("n o"))
            {
                Optional(push, field, "$request.push", (items, path) => Each(List(items, path), path, (item, itemPath) =>
                {
                    var entry = (IDictionary<string, object>)item;
                    var tags = new HashSet<string>(StringComparer.Ordinal);
                    Optional(entry, "r", itemPath, (roles, rolesPath) => Each(List(roles, rolesPath), rolesPath, (roleValue, rolePath) =>
                    {
                        var role = Map(roleValue, rolePath, "t u c a r");
                        Check(tags.Add(Text(Required(role, "t", rolePath), rolePath + ".t")), rolePath, "Submit each role tag once per object.");
                        var alternativeCount = (Value(role, "u") != null ? 1 : 0) + (Value(role, "c") != null ? 1 : 0) + (Value(role, "a") != null || Value(role, "r") != null ? 1 : 0);
                        Check(alternativeCount <= 1, rolePath, "Supply one unit, composite, or collection-update alternative.");
                        Optional(role, "u", rolePath, Encoded);
                        Optional(role, "c", rolePath, (reference, referencePath) => PushReference(reference, referencePath, newIds));
                        foreach (var relation in Fields("a r"))
                        {
                            Optional(role, relation, rolePath, (references, referencesPath) =>
                            {
                                var seen = new HashSet<long>();
                                Each(List(references, referencesPath), referencesPath, (reference, referencePath) => Check(seen.Add(PushReference(reference, referencePath, newIds)), referencePath, "Include each relation ID once."));
                            });
                        }
                    }));
                }));
            }
        }

        internal static void PushReceipt(object value, object original)
        {
            PushRequest(original);
            var request = (IDictionary<string, object>)original;
            var submittedNewIds = new HashSet<long>();
            var submittedPush = (IDictionary<string, object>)request["push"];
            Optional(submittedPush, "n", "$request.push", (items, path) => Each(List(items, path), path, (item, itemPath) => submittedNewIds.Add(Integer(((IDictionary<string, object>)item)["w"], itemPath + ".w", -MaximumInteger, -1))));
            var response = Map(value, "$response", "i sc push changed discovery");
            Check(Text(Required(response, "i", "$response"), "$response.i") == (string)request["i"], "$response.i", "Echo the original request correlation ID.");
            var outcome = Map(Required(response, "push", "$response"), "$response.push", "k n e _e _v _a _m _d");
            var kind = Choice(Required(outcome, "k", "$response.push"), "$response.push.k", "committed", "rejected", "unknown");
            Optional(response, "sc", "$response", (item, path) => Scope(item, path));
            if (kind == "committed")
            {
                var scope = Scope(Required(response, "sc", "$response"), "$response.sc");
                var expected = (IDictionary<string, object>)request["sc"];
                foreach (var field in Fields("p u w r x"))
                {
                    Check(field == "u" ? Id(scope[field], "$response.sc.u") == Id(expected[field], "$request.sc.u") : Equals(Value(scope, field), Value(expected, field)), "$response.sc." + field, "Keep the scope admitted for the write in the committed receipt.");
                }

                var push = (IDictionary<string, object>)request["push"];
                var expectedIds = new HashSet<long>();
                Optional(push, "n", "$request.push", (items, path) => Each(List(items, path), path, (item, itemPath) => expectedIds.Add(Integer(((IDictionary<string, object>)item)["w"], itemPath + ".w", -MaximumInteger, -1))));
                var workspaceIds = new HashSet<long>();
                var databaseIds = new HashSet<long>();
                Each(List(Required(outcome, "n", "$response.push"), "$response.push.n"), "$response.push.n", (item, path) =>
                {
                    var mapping = Map(item, path, "w d");
                    Check(workspaceIds.Add(Integer(Required(mapping, "w", path), path + ".w", -MaximumInteger, -1)), path, "Map each workspace ID once.");
                    Check(databaseIds.Add(Id(Required(mapping, "d", path), path + ".d")), path, "Map each new object to a distinct positive database ID.");
                });
                Check(expectedIds.SetEquals(workspaceIds), "$response.push.n", "Return exactly one mapping for every submitted new object.");
            }
            else
            {
                Check(Value(outcome, "n") == null, "$response.push.n", "Return new-ID mappings only after a known commit.");
            }

            var hasDiagnostics = false;
            Optional(outcome, "e", "$response.push", (items, path) =>
            {
                var errors = List(items, path);
                Check(errors.Count > 0, path, "Supply nonempty diagnostics or omit them.");
                hasDiagnostics = true;
                Each(errors, path, (item, itemPath) =>
                {
                    var error = Map(item, itemPath, "c m o i");
                    Text(Required(error, "c", itemPath), itemPath + ".c");
                    Text(Required(error, "m", itemPath), itemPath + ".m");
                    Optional(error, "o", itemPath, (ids, idsPath) => PushDiagnosticIds(ids, idsPath, submittedNewIds));
                    Check(Value(error, "i") == null, itemPath + ".i", "Mutation diagnostics have no query selection index.");
                });
            });
            Optional(outcome, "_e", "$response.push", (item, path) => { Text(item, path); Check(kind != "committed", path, "A committed outcome cannot also report legacy Push failures."); hasDiagnostics = true; });
            foreach (var field in Fields("_v _a _m"))
            {
                Optional(outcome, field, "$response.push", (items, path) =>
                {
                    hasDiagnostics |= List(items, path).Count > 0;
                    Check(kind != "committed" || List(items, path).Count == 0, path, "A committed outcome cannot also report legacy Push failures.");
                    if (field == "_v") IdSet(List(items, path), path, false);
                    else PushDiagnosticIds(items, path, submittedNewIds);
                });
            }
            Optional(outcome, "_d", "$response.push", (items, path) =>
            {
                hasDiagnostics |= List(items, path).Count > 0;
                Check(kind != "committed" || List(items, path).Count == 0, path, "A committed outcome cannot also report legacy Push failures.");
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
            Check(kind == "committed" || hasDiagnostics, "$response.push", "Explain a rejected or unknown mutation outcome with actionable diagnostics.");
        }

        internal static void PushResponse(object value, object original)
        {
            PushReceipt(value, original);
            var response = (IDictionary<string, object>)value;
            var request = (IDictionary<string, object>)original;
            var kind = (string)((IDictionary<string, object>)response["push"])["k"];
            foreach (var part in Fields("changed discovery"))
            {
                var requested = kind == "committed" && (part == "changed" ? Equals(Value(request, "returnChanged"), true) : Value(request, "discover") != null);
                Check(requested == (Value(response, part) != null), "$response." + part, "Return each requested read outcome exactly once after commit, and no other read parts.");
                if (!requested) continue;
                var read = Map(response[part], "$response." + part, null);
                var readRequest = ReadRequestForPush(request, part == "changed" ? "read" : "discover", part == "changed" ? null : request["discover"]);
                if (part != "changed" || Equals(Value(read, "k"), "error"))
                {
                    ValidateResponse(read, readRequest);
                    continue;
                }
                AffectedResponse(read, readRequest);
            }
        }

        private static void AffectedResponse(IDictionary<string, object> read, object readRequest)
        {
            Map(read, "$response.changed", "i sc k p s pm u");
            Choice(Required(read, "k", "$response.changed"), "$response.changed.k", "changed");
            var unavailable = IdSet(List(Required(read, "u", "$response.changed"), "$response.changed.u"), "$response.changed.u", false);
            var complete = new Dictionary<string, object>(read);
            complete.Remove("u");
            complete["k"] = "complete";
            complete["l"] = new List<object>();
            complete["c"] = new Dictionary<string, object>();
            complete["o"] = new Dictionary<string, object>();
            ValidateResponse(complete, readRequest);
            foreach (var record in List(read["p"], "$response.changed.p"))
            {
                Check(!unavailable.Contains(Id(((IDictionary<string, object>)record)["i"], "$response.changed.p.i")), "$response.changed.u", "An affected ID cannot have both a full record and an unavailable outcome.");
            }
        }

        private static void PushDiagnosticIds(object value, string path, ISet<long> newIds)
        {
            var seen = new HashSet<long>();
            Each(List(value, path), path, (item, itemPath) => Check(seen.Add(PushReference(item, itemPath, newIds)), itemPath, "Include each diagnostic object ID once."));
        }

        private static long PushReference(object value, string path, ISet<long> newIds)
        {
            var id = Integer(value, path, -MaximumInteger, MaximumInteger);
            Check(id != 0 && (id > 0 || newIds.Contains(id)), path, "Reference a positive database ID or a negative workspace ID declared by this push.");
            return id;
        }

        private static IDictionary<string, object> ReadRequestForPush(IDictionary<string, object> request, string kind, object pull) => new Dictionary<string, object>
        {
            ["v"] = Required(request, "v", "$request"), ["i"] = Required(request, "i", "$request"),
            ["sc"] = Required(request, "sc", "$request"), ["x"] = Value(request, "x"), ["k"] = kind,
            ["p"] = pull ?? new Dictionary<string, object> { ["l"] = new List<object>(), ["d"] = new List<object>() },
        };
    }
}
