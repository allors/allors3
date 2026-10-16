// <copyright file="ReadValidation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Validates raw read-message trees before serializers discard unknown or missing fields.
    /// This checks wire structure and advertised references, not authentication, cache possession,
    /// metadata compatibility, or the contents of reused security bodies.
    /// </summary>
    /// <remarks>
    /// Trees contain IDictionary&lt;string, object&gt;, IList&lt;object&gt;, string, bool, long,
    /// double, and null values. Serializer adapters must preserve member presence and reject
    /// duplicate JSON properties before constructing a tree. Validation does not modify it.
    /// </remarks>
    public static partial class ReadValidation
    {
        private const long MaximumInteger = 9007199254740991;

        /// <summary>
        /// Validates a raw request represented by dictionaries, lists and JSON scalar values.
        /// </summary>
        public static void ValidateRequest(object value) => Request(value, "$");

        /// <summary>
        /// Validates a raw response against the original request and its advertised inventory.
        /// </summary>
        public static void ValidateResponse(object value, object request)
        {
            var original = Request(request, "$request");
            var response = Map(value, "$", null);
            var kind = Text(Required(response, "k", "$"), "$.k");
            var allowed = kind == "discovery" ? "i sc k p s" : kind == "complete" ? "i sc k l c o p s pm" : kind == "error" ? "i sc k e" : null;
            Check(allowed != null, "$.k", "Use discovery, complete, or error as the response kind.");
            Map(value, "$", allowed);
            var id = Text(Required(response, "i", "$"), "$.i");
            Check(id == Text(original.Root["i"], "$request.i"), "$.i", "Echo the original request correlation ID.");

            if (kind == "error")
            {
                Optional(response, "sc", "$", (scopeValue, scopePath) => Scope(scopeValue, scopePath));
                Failure(response, original);
                return;
            }

            Check((string)original.Root["k"] == (kind == "discovery" ? "discover" : "read"), "$.k", "Return the response kind for the original request phase.");
            var scope = Scope(Required(response, "sc", "$"), "$.sc");
            Check(Equals(Value(scope, "x"), Value(original.Root, "x")), "$.sc.x", "Echo the captured request context in the response scope.");
            if (original.Scope != null)
            {
                foreach (var field in Fields("p u w r x"))
                {
                    var matches = field == "u"
                        ? Id(scope[field], "$.sc.u") == Id(original.Scope[field], "$request.sc.u")
                        : Equals(Value(scope, field), Value(original.Scope, field));
                    Check(matches, "$.sc." + field, "Use the expected request scope, or return an error response for a scope mismatch.");
                }
            }

            var security = List(Required(response, "s", "$"), "$.s");
            var securityKeys = SecurityTable(security, "$.s", kind == "complete" ? "k i v p" : "k i v", false);
            var pool = List(Required(response, "p", "$"), "$.p");
            var poolIds = new HashSet<long>();
            var usedSecurity = new HashSet<int>();
            for (var index = 0; index < pool.Count; index++)
            {
                var path = At("$.p", index);
                var record = Map(pool[index], path, kind == "complete" ? "i c v g r p" : "i c v g r");
                var key = RecordHeader(record, path, security, usedSecurity);
                Check(poolIds.Add(Id(record["i"], path + ".i")), path + ".i", "Include each pool object ID only once.");
                if (kind == "complete")
                {
                    RecordPayload(Required(record, "p", path), path + ".p", key, original);
                }
            }

            Check(usedSecurity.Count == security.Count, "$.s", "Return only security rows referenced by the final pool.");
            if (kind == "discovery")
            {
                return;
            }

            Inputs(response, original);
            Outputs(response, poolIds);
            Permissions(response, original, security, securityKeys);
        }

        private static RequestData Request(object value, string path)
        {
            var request = Map(value, path, "v i sc x k p h");
            Check(Integer(Required(request, "v", path), path + ".v", 0, int.MaxValue) == 1, path + ".v", "Use supported read protocol revision 1.");
            Text(Required(request, "i", path), path + ".i");
            Choice(Required(request, "k", path), path + ".k", "discover", "read");
            Optional(request, "x", path, Context);
            var scope = Value(request, "sc") == null ? null : Scope(request["sc"], path + ".sc");
            if (scope != null)
            {
                Check(Equals(Value(scope, "x"), Value(request, "x")), path + ".sc.x", "Use the captured request context in the expected scope.");
            }

            var pull = Map(Required(request, "p", path), path + ".p", "l d");
            var selections = List(Required(pull, "l", path + ".p"), path + ".p.l");
            Each(selections, path + ".p.l", Selection);
            Each(List(Required(pull, "d", path + ".p"), path + ".p.d"), path + ".p.d", Dependency);
            var result = new RequestData { Root = request, Scope = scope, Selections = selections };
            if (Value(request, "h") == null)
            {
                return result;
            }

            var cachePath = path + ".h";
            var cache = Map(request["h"], cachePath, "s r p");
            result.Security = List(Required(cache, "s", cachePath), cachePath + ".s");
            result.Records = List(Required(cache, "r", cachePath), cachePath + ".r");
            result.Permissions = List(Required(cache, "p", cachePath), cachePath + ".p");
            Check(scope != null || result.Security.Count + result.Records.Count + result.Permissions.Count == 0, path + ".sc", "Supply the expected scope when advertising cache hints.");
            SecurityTable(result.Security, cachePath + ".s", "k i v h", true);
            var candidates = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < result.Records.Count; index++)
            {
                var recordPath = At(cachePath + ".r", index);
                var record = Map(result.Records[index], recordPath, "i c v g r");
                var key = RecordHeader(record, recordPath, result.Security, null);
                Check(candidates.Add(key), recordPath, "Advertise each exact record candidate only once.");
                result.RecordKeys.Add(key);
            }

            IdSet(result.Permissions, cachePath + ".p", false);
            return result;
        }

        private static IDictionary<string, object> Scope(object value, string path)
        {
            var scope = Map(value, path, "p u w r x");
            foreach (var field in Fields("p w r"))
            {
                Text(Required(scope, field, path), path + "." + field);
            }

            Id(Required(scope, "u", path), path + ".u");
            Optional(scope, "x", path, Context);
            return scope;
        }

        private static List<string> SecurityTable(IList<object> table, string path, string fields, bool hints)
        {
            var unique = new HashSet<string>(StringComparer.Ordinal);
            var keys = new List<string>();
            for (var index = 0; index < table.Count; index++)
            {
                var itemPath = At(path, index);
                var row = Map(table[index], itemPath, fields);
                var key = SecurityKey(row, itemPath);
                Check(unique.Add(key), itemPath, "Include each security kind, ID and version only once.");
                if (hints)
                {
                    Check(Required(row, "h", itemPath) is bool, itemPath + ".h", "Supply a boolean indicating whether this exact security body is retained.");
                }

                keys.Add(key);
            }

            return keys;
        }

        private static string SecurityKey(IDictionary<string, object> row, string path)
        {
            var kind = Choice(Required(row, "k", path), path + ".k", "grant", "revocation");
            var id = Id(Required(row, "i", path), path + ".i");
            var version = Version(Required(row, "v", path), path + ".v");
            return kind + ":" + Number(id) + ":" + Number(version);
        }

        private static string RecordHeader(IDictionary<string, object> row, string path, IList<object> security, ISet<int> used)
        {
            var id = Id(Required(row, "i", path), path + ".i");
            var tag = Text(Required(row, "c", path), path + ".c");
            var version = Version(Required(row, "v", path), path + ".v");
            var grants = Provenance(Required(row, "g", path), path + ".g", "grant", security, used);
            var revocations = Provenance(Required(row, "r", path), path + ".r", "revocation", security, used);
            return Number(id) + ":" + tag.Length + ":" + tag + ":" + Number(version) + ":" + grants + ":" + revocations;
        }

        private static string Provenance(object value, string path, string kind, IList<object> security, ISet<int> used)
        {
            var references = List(value, path);
            var indexes = new HashSet<int>();
            var keys = new List<string>();
            for (var index = 0; index < references.Count; index++)
            {
                var itemPath = At(path, index);
                var reference = Index(references[index], itemPath, security.Count);
                Check(indexes.Add(reference), itemPath, "Include each security reference only once in this set.");
                var row = (IDictionary<string, object>)security[reference];
                Check((string)row["k"] == kind, itemPath, "Reference a security row of kind " + kind + ".");
                keys.Add(SecurityKey(row, itemPath));
                used?.Add(reference);
            }

            keys.Sort(StringComparer.Ordinal);
            return string.Join(",", keys);
        }

        private static void RecordPayload(object value, string path, string recordKey, RequestData request)
        {
            var payload = Map(value, path, "ro r");
            Alternative(payload, "ro", "r", path);
            if (Value(payload, "ro") != null)
            {
                var tags = new HashSet<string>(StringComparer.Ordinal);
                Each(List(payload["ro"], path + ".ro"), path + ".ro", (roleValue, rolePath) =>
                {
                    var role = Map(roleValue, rolePath, "t c o v");
                    Check(tags.Add(Text(Required(role, "t", rolePath), rolePath + ".t")), rolePath + ".t", "Include each role tag only once in a full body.");
                    Check(Fields("c o v").Count(field => Value(role, field) != null) <= 1, rolePath, "Supply at most one non-null collection, object, or encoded unit value.");
                    Optional(role, "c", rolePath, (items, itemsPath) => IdSet(List(items, itemsPath), itemsPath, true));
                    Optional(role, "o", rolePath, (item, itemPath) => Id(item, itemPath));
                    Optional(role, "v", rolePath, Encoded);
                });
                return;
            }

            var reference = Index(payload["r"], path + ".r", request.RecordKeys.Count);
            Check(recordKey == request.RecordKeys[reference], path + ".r", "Reuse only the exact advertised record ID, class, version, and complete security provenance; otherwise supply a full body.");
        }

        private static void Inputs(IDictionary<string, object> response, RequestData request)
        {
            var inputs = List(Required(response, "l", "$"), "$.l");
            Check(inputs.Count == request.Selections.Count, "$.l", "Return one input outcome for every original selection.");
            var indexes = new HashSet<int>();
            Each(inputs, "$.l", (value, path) =>
            {
                var input = Map(value, path, "i o s");
                var index = Index(Required(input, "i", path), path + ".i", request.Selections.Count);
                Check(indexes.Add(index), path + ".i", "Return each original selection index only once.");
                var status = Choice(Required(input, "s", path), path + ".s", "resolved", "unavailable");
                var target = Value((IDictionary<string, object>)request.Selections[index], "o");
                var returned = Value(input, "o");
                if (returned != null)
                {
                    Id(returned, path + ".o");
                }

                Check(target == null ? returned == null : returned != null && Id(target, path + ".o") == Id(returned, path + ".o"), path + ".o", "Echo the original selection's explicit target, or null when it has none.");
                Check(status != "unavailable" || target != null, path + ".s", "Use unavailable only for a selection with an explicit target.");
            });
        }

        private static void Outputs(IDictionary<string, object> response, ISet<long> pool)
        {
            foreach (var entry in Map(Required(response, "c", "$"), "$.c", null))
            {
                var path = "$.c." + entry.Key;
                foreach (var id in IdSet(List(entry.Value, path), path, false))
                {
                    Check(pool.Contains(id), path, "Include every named collection object in the response pool.");
                }
            }

            foreach (var entry in Map(Required(response, "o", "$"), "$.o", null))
            {
                if (entry.Value != null)
                {
                    var path = "$.o." + entry.Key;
                    Check(pool.Contains(Id(entry.Value, path)), path, "Include every named object in the response pool, or return explicit null.");
                }
            }
        }

        private static void Permissions(IDictionary<string, object> response, RequestData request, IList<object> security, IList<string> securityKeys)
        {
            var permissions = List(Required(response, "pm", "$"), "$.pm");
            var ids = new HashSet<long>();
            Each(permissions, "$.pm", (value, path) =>
            {
                var permission = Map(value, path, "i p");
                var id = Id(Required(permission, "i", path), path + ".i");
                Check(ids.Add(id), path + ".i", "Include each permission ID only once.");
                PermissionPayload(Required(permission, "p", path), path + ".p", id, request);
            });

            var required = new HashSet<long>();
            var hasReuse = false;
            for (var index = 0; index < security.Count; index++)
            {
                var path = At("$.s", index);
                var row = (IDictionary<string, object>)security[index];
                var payload = Map(Required(row, "p", path), path + ".p", "p r");
                Alternative(payload, "p", "r", path + ".p");
                if (Value(payload, "p") != null)
                {
                    foreach (var id in IdSet(List(payload["p"], path + ".p.p"), path + ".p.p", false))
                    {
                        Check(ids.Contains(id), path + ".p.p", "Supply or reuse every permission descriptor referenced by a full security body.");
                        required.Add(id);
                    }
                }
                else
                {
                    hasReuse = true;
                    var reference = Index(payload["r"], path + ".p.r", request.Security.Count);
                    var advertised = (IDictionary<string, object>)request.Security[reference];
                    Check((bool)advertised["h"] && securityKeys[index] == SecurityKey(advertised, path + ".p.r"), path + ".p.r", "Reuse only the exact advertised security kind, ID and version with h true; otherwise supply its full body.");
                }
            }

            Check(hasReuse || required.SetEquals(ids), "$.pm", "Return only permission descriptors required by the security bodies.");
        }

        private static void PermissionPayload(object value, string path, long id, RequestData request)
        {
            var payload = Map(value, path, "c o t r");
            if (Value(payload, "r") != null)
            {
                Check(Fields("c o t").All(field => Value(payload, field) == null), path, "Supply either a full permission descriptor or its reuse index, with other fields null or omitted.");
                var reference = Index(payload["r"], path + ".r", request.Permissions.Count);
                Check(Id(request.Permissions[reference], path + ".r") == id, path + ".r", "Reuse the request permission entry with the same permission ID.");
                return;
            }

            Text(Required(payload, "c", path), path + ".c");
            var operation = Integer(Required(payload, "o", path), path + ".o", 0, int.MaxValue);
            Check(operation == 1 || operation == 2 || operation == 4 || operation == 8, path + ".o", "Use operation 1 (Create), 2 (Read), 4 (Write), or 8 (Execute).");
            if (operation == 1)
            {
                Check(Value(payload, "t") == null, path + ".t", "Omit the operand or use null for class-only Create permission.");
            }
            else
            {
                Text(Required(payload, "t", path), path + ".t");
            }
        }

        private static void Failure(IDictionary<string, object> response, RequestData request)
        {
            var errors = List(Required(response, "e", "$"), "$.e");
            Check(errors.Count != 0, "$.e", "Return at least one actionable error.");
            Each(errors, "$.e", (value, path) =>
            {
                var error = Map(value, path, "c m o i");
                Text(Required(error, "c", path), path + ".c");
                Text(Required(error, "m", path), path + ".m");
                Optional(error, "o", path, (items, itemsPath) => IdSet(List(items, itemsPath), itemsPath, false));
                Optional(error, "i", path, (index, indexPath) => Index(index, indexPath, request.Selections.Count));
            });
        }

        private static void Selection(object value, string path)
        {
            var selection = Map(value, path, "e t o r a");
            Optional(selection, "e", path, Extent);
            Optional(selection, "t", path, (item, itemPath) => Text(item, itemPath));
            Optional(selection, "o", path, (item, itemPath) => Id(item, itemPath));
            Optional(selection, "r", path, (items, itemsPath) => Each(List(items, itemsPath), itemsPath, Result));
            Optional(selection, "a", path, (arguments, argumentsPath) =>
            {
                foreach (var entry in Map(arguments, argumentsPath, null))
                {
                    JsonData(entry.Value, argumentsPath + "." + entry.Key);
                }
            });
        }

        private static void Dependency(object value, string path)
        {
            var dependency = Map(value, path, "o a r");
            Text(Required(dependency, "o", path), path + ".o");
            Alternative(dependency, "a", "r", path);
            Text(Value(dependency, "a") ?? Value(dependency, "r"), path);
        }

        private static void Extent(object value, string path)
        {
            var extent = Map(value, path, "k o t p s");
            Integer(Required(extent, "k", path), path + ".k", 1, 4);
            OptionalTexts(extent, path, "t");
            Optional(extent, "o", path, (items, itemsPath) => Each(List(items, itemsPath), itemsPath, Extent));
            Optional(extent, "p", path, Predicate);
            Optional(extent, "s", path, (items, itemsPath) => Each(List(items, itemsPath), itemsPath, Sort));
        }

        private static void Predicate(object value, string path)
        {
            var predicate = Map(value, path, "k a r o p d op ops ob obs v vs pa pas e");
            Integer(Required(predicate, "k", path), path + ".k", 1, 12);
            OptionalTexts(predicate, path, "a r o p pa");
            foreach (var field in Fields("d pas"))
            {
                Optional(predicate, field, path, (items, itemsPath) => Each(List(items, itemsPath), itemsPath, (item, itemPath) => Text(item, itemPath)));
            }

            Optional(predicate, "op", path, Predicate);
            Optional(predicate, "ops", path, (items, itemsPath) => Each(List(items, itemsPath), itemsPath, Predicate));
            Optional(predicate, "ob", path, (item, itemPath) => Id(item, itemPath));
            Optional(predicate, "obs", path, (items, itemsPath) => Each(List(items, itemsPath), itemsPath, (item, itemPath) => Id(item, itemPath)));
            Optional(predicate, "v", path, Encoded);
            Optional(predicate, "vs", path, (items, itemsPath) => Each(List(items, itemsPath), itemsPath, Encoded));
            Optional(predicate, "e", path, Extent);
        }

        private static void Result(object value, string path)
        {
            var result = Map(value, path, "s i n k t");
            Optional(result, "s", path, Select);
            Optional(result, "i", path, (items, itemsPath) => Each(List(items, itemsPath), itemsPath, Node));
            OptionalTexts(result, path, "n");
            foreach (var field in Fields("k t"))
            {
                Optional(result, field, path, (item, itemPath) => Integer(item, itemPath, 0, int.MaxValue));
            }
        }

        private static void Select(object value, string path)
        {
            var select = Map(value, path, "a r o n i");
            OptionalTexts(select, path, "a r o");
            Optional(select, "n", path, Select);
            Optional(select, "i", path, (items, itemsPath) => Each(List(items, itemsPath), itemsPath, Node));
        }

        private static void Node(object value, string path)
        {
            var node = Map(value, path, "a r n");
            OptionalTexts(node, path, "a r");
            Optional(node, "n", path, (items, itemsPath) => Each(List(items, itemsPath), itemsPath, Node));
        }

        private static void Sort(object value, string path)
        {
            var sort = Map(value, path, "r d");
            OptionalTexts(sort, path, "r");
            Optional(sort, "d", path, (item, itemPath) => Integer(item, itemPath, 1, 2));
        }

        private static void JsonData(object value, string path)
        {
            if (value is IDictionary<string, object> map)
            {
                foreach (var entry in map)
                {
                    JsonData(entry.Value, path + "." + entry.Key);
                }
            }
            else if (value is IList<object> list)
            {
                Each(list, path, JsonData);
            }
            else
            {
                Encoded(value, path);
            }
        }

        private static void Encoded(object value, string path) => Check(value == null || value is string || value is bool || value is long || value is double number && !double.IsInfinity(number) && !double.IsNaN(number), path, "Supply a null, string, boolean, or finite numeric encoded value.");

        private static void Context(object value, string path) => Check(value is string, path, "Supply a string context, or omit it or use null.");

        private static void OptionalTexts(IDictionary<string, object> map, string path, string fields)
        {
            foreach (var field in Fields(fields))
            {
                Optional(map, field, path, (value, valuePath) => Text(value, valuePath));
            }
        }

        private static void Optional(IDictionary<string, object> map, string field, string path, Action<object, string> validate)
        {
            if (Value(map, field) != null)
            {
                validate(map[field], path + "." + field);
            }
        }

        private static void Alternative(IDictionary<string, object> map, string full, string reuse, string path) => Check((Value(map, full) != null) != (Value(map, reuse) != null), path, "Supply exactly one non-null " + full + " or " + reuse + " alternative; empty arrays and index zero are present values.");

        private static HashSet<long> IdSet(IList<object> items, string path, bool sorted)
        {
            var result = new HashSet<long>();
            long previous = 0;
            for (var index = 0; index < items.Count; index++)
            {
                var itemPath = At(path, index);
                var id = Id(items[index], itemPath);
                Check(result.Add(id), itemPath, "Include each object ID only once in this set.");
                Check(!sorted || id > previous, itemPath, "Encode relation collections as strictly ascending positive object IDs.");
                previous = id;
            }

            return result;
        }

        private static long Id(object value, string path) => Integer(value, path, 1, MaximumInteger);

        private static long Version(object value, string path) => Integer(value, path, 0, MaximumInteger);

        private static int Index(object value, string path, int count)
        {
            var index = Integer(value, path, 0, int.MaxValue);
            Check(index < count, path, "Use an index within the referenced table, or supply the full body instead of reusing it.");
            return (int)index;
        }

        private static long Integer(object value, string path, long minimum, long maximum)
        {
            if (value is long integer)
            {
                Check(integer >= minimum && integer <= maximum, path, "Supply an integer between " + Number(minimum) + " and " + Number(maximum) + ".");
                return integer;
            }

            if (value is double number)
            {
                Check(!double.IsNaN(number) && !double.IsInfinity(number) && Math.Truncate(number) == number && number >= minimum && number <= maximum, path, "Supply an exactly representable integer between " + Number(minimum) + " and " + Number(maximum) + ".");
                return (long)number;
            }

            throw Invalid(path, "Supply a JSON integer, not a string, boolean, or null.");
        }

        private static string Choice(object value, string path, params string[] choices)
        {
            var text = Text(value, path);
            Check(Array.IndexOf(choices, text) >= 0, path, "Use one of: " + string.Join(", ", choices) + ".");
            return text;
        }

        private static string Text(object value, string path)
        {
            // Include the Unicode BOM in blank text, matching JavaScript's whitespace handling.
            Check(value is string text && text.Any(character => !char.IsWhiteSpace(character) && character != '\uFEFF'), path, "Supply a nonempty string.");
            return (string)value;
        }

        private static object Required(IDictionary<string, object> map, string field, string path)
        {
            Check(map.TryGetValue(field, out var value) && value != null, path + "." + field, "Supply this required field; use an explicit empty array or object when appropriate.");
            return value;
        }

        private static object Value(IDictionary<string, object> map, string field) => map.TryGetValue(field, out var value) ? value : null;

        private static IDictionary<string, object> Map(object value, string path, string allowed)
        {
            Check(value is IDictionary<string, object>, path, "Supply a JSON object.");
            var map = (IDictionary<string, object>)value;
            if (allowed != null)
            {
                var fields = Fields(allowed);
                foreach (var field in map.Keys)
                {
                    Check(Array.IndexOf(fields, field) >= 0, path + "." + field, "Remove this unsupported field and use supported inline query declarations.");
                }
            }

            return map;
        }

        private static IList<object> List(object value, string path)
        {
            Check(value is IList<object>, path, "Supply a JSON array, including an explicit empty array when there are no entries.");
            return (IList<object>)value;
        }

        private static void Each(IList<object> values, string path, Action<object, string> validate)
        {
            for (var index = 0; index < values.Count; index++)
            {
                validate(values[index], At(path, index));
            }
        }

        private static string[] Fields(string fields) => fields.Split(' ');

        private static string At(string path, int index) => path + "[" + index + "]";

        private static string Number(long value) => value.ToString(CultureInfo.InvariantCulture);

        private static void Check(bool valid, string path, string remedy)
        {
            if (!valid)
            {
                throw Invalid(path, remedy);
            }
        }

        private static FormatException Invalid(string path, string remedy) => new FormatException(path + ": " + remedy);

        private sealed class RequestData
        {
            internal IDictionary<string, object> Root { get; set; }

            internal IDictionary<string, object> Scope { get; set; }

            internal IList<object> Selections { get; set; }

            internal IList<object> Security { get; set; } = new List<object>();

            internal IList<object> Records { get; set; } = new List<object>();

            internal IList<object> Permissions { get; set; } = new List<object>();

            internal List<string> RecordKeys { get; } = new List<string>();
        }
    }
}
