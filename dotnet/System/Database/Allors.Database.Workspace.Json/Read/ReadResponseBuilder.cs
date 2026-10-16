// <copyright file="ReadResponseBuilder.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Read;
    using Allors.Protocol.Json.Api.Sync;
    using Data;
    using Meta;
    using Security;
    using Services;

    internal sealed class ReadResponseBuilder
    {
        private readonly ReadContext context;
        private readonly ReadRequest request;
        private readonly ReadCompilation compilation;
        private readonly IUnitConvert unitConvert;
        private readonly ITransaction transaction;
        private readonly HashSet<IClass> classes;
        private readonly Dictionary<IClass, ISet<IRoleType>> roleTypes;
        private readonly Dictionary<long, IObject> pool = new Dictionary<long, IObject>();
        private readonly Dictionary<string, List<long>> collections = new Dictionary<string, List<long>>(StringComparer.Ordinal);
        private readonly Dictionary<string, long?> objects = new Dictionary<string, long?>(StringComparer.Ordinal);
        private readonly List<ReadInput> inputs = new List<ReadInput>();
        private readonly List<ReadSecurity> security = new List<ReadSecurity>();
        private readonly Dictionary<(string Kind, long Id, long Version), (int Index, long[] Body)> securityEntries = new Dictionary<(string, long, long), (int, long[])>();
        private int traversalSteps;
        private readonly HashSet<(long Id, Node Node)> included = new HashSet<(long, Node)>();
        private readonly HashSet<long> permissionIds = new HashSet<long>();

        internal ReadResponseBuilder(ReadContext context, ReadRequest request, ReadCompilation compilation, IUnitConvert unitConvert)
        {
            this.context = context;
            this.request = request;
            this.compilation = compilation;
            this.unitConvert = unitConvert;
            this.transaction = context.Transaction;
            var meta = this.transaction.Database.Services.Get<IMetaCache>();
            this.classes = new HashSet<IClass>(meta.GetWorkspaceClasses(context.Scope.w));
            this.roleTypes = meta.GetWorkspaceRoleTypesByClass(context.Scope.w)
                .ToDictionary(v => v.Key, v => (ISet<IRoleType>)new HashSet<IRoleType>(v.Value));
        }

        internal object Build()
        {
            for (var i = 0; i < this.compilation.Pulls.Length; i++)
            {
                this.context.EnsureValid();
                this.Select(this.compilation.Pulls[i], i);
            }

            this.AddDependencies();
            var records = this.pool.Values.OrderBy(v => v.Id).Select(this.Record).ToArray();
            this.context.EnsureValid();
            if (this.request.k == "discover")
            {
                return new DiscoverySuccess
                {
                    i = this.request.i, k = "discovery", sc = this.context.Scope,
                    p = records.Select(v => new ReadRecordHeader { i = v.i, c = v.c, v = v.v, g = v.g, r = v.r }).ToArray(),
                    s = this.security.Select(v => new ReadSecurityHeader { k = v.k, i = v.i, v = v.v }).ToArray(),
                };
            }

            var permissions = this.permissionIds.OrderBy(v => v).Select(this.Permission).ToArray();
            this.context.EnsureValid();
            return new ReadSuccess
            {
                i = this.request.i, k = "complete", sc = this.context.Scope, l = this.inputs.ToArray(),
                c = this.collections.ToDictionary(v => v.Key, v => v.Value.ToArray()), o = this.objects,
                p = records, s = this.security.ToArray(), pm = permissions,
            };
        }

        internal Allors.Protocol.Json.Api.PushRead.PushReadChanged BuildAffected(IEnumerable<Adapters.Sql.AffectedObject> affected)
        {
            var unavailable = new List<long>();
            var count = 0;
            foreach (var entry in affected.OrderBy(value => value.Id))
            {
                this.context.EnsureValid();
                // Captured class eligibility also governs identities deleted during derivation.
                if (!this.classes.Contains(entry.Class)) { continue; }
                if (++count > ReadExecutor.MaxPoolObjects)
                {
                    throw new ReadRequestException("The affected-object refresh exceeds 10000 objects. Read smaller selections after acknowledging the committed mutation.");
                }

                var value = this.transaction.Instantiate(entry.Id);
                if (value == null) { unavailable.Add(entry.Id); }
                else { this.Add(value); }
            }

            var records = this.pool.Values.OrderBy(value => value.Id).Select(this.Record).ToArray();
            var permissions = this.permissionIds.OrderBy(value => value).Select(this.Permission).ToArray();
            this.context.EnsureValid();
            return new Allors.Protocol.Json.Api.PushRead.PushReadChanged
            {
                i = this.request.i, sc = this.context.Scope, k = "changed", p = records,
                s = this.security.ToArray(), pm = permissions, u = unavailable.ToArray(),
            };
        }

        private void Visit()
        {
            this.context.EnsureValid();
            if (++this.traversalSteps > 1000000)
            {
                throw new ReadRequestException("Narrow the read selections, includes or dependencies: this attempt exceeds one million traversal steps.");
            }
        }

        private bool Eligible(IObject value) => value != null && this.classes.Contains(value.Strategy.Class);

        private void Select(Pull pull, int index)
        {
            var targetId = this.request.p.l[index].o;
            var explicitTarget = targetId.HasValue;
            var source = pull.Object;
            var available = !explicitTarget || (this.Eligible(source) && (pull.ObjectType == null || ((IComposite)pull.ObjectType).IsAssignableFrom(source.Strategy.Class)));
            this.inputs.Add(new ReadInput { i = index, o = targetId, s = available ? "resolved" : "unavailable" });
            var type = pull.ObjectType as IComposite ?? source?.Strategy.Class ?? pull.Extent?.ObjectType;
            var results = explicitTarget ? pull.Results ?? new[] { new Result() }
                : pull.Results?.Length > 0 ? pull.Results : new[] { new Result() };
            // Every result starts at the original target or extent. Includes and cache reuse never prune it.
            IEnumerable<IObject> baseObjects = explicitTarget
                ? (available ? new[] { source } : Array.Empty<IObject>())
                : pull.Extent.Build(this.transaction, pull.Arguments).Where(value => { this.Visit(); return this.Eligible(value); });
            foreach (var result in results)
            {
                this.context.EnsureValid();
                var select = result.Select;
                var isObject = explicitTarget && (select?.PropertyType == null || select.IsOne);
                var name = result.Name ?? (select?.PropertyType != null
                    ? (isObject ? select.End.PropertyType.SingularName : select.End.PropertyType.PluralName)
                    : (isObject ? type?.SingularName : type?.PluralName));
                // A missing untyped implicit target has no declared output name to recover.
                if (name == null)
                {
                    continue;
                }

                var paged = (IEnumerable<IObject>)baseObjects;
                if (!explicitTarget)
                {
                    if (result.Skip.HasValue) { paged = paged.Skip(result.Skip.Value); }
                    if (result.Take.HasValue) { paged = paged.Take(result.Take.Value); }
                }

                var selected = select?.PropertyType == null ? paged : paged.SelectMany(v => this.TraverseSelect(v, select));
                var values = this.Bounded(selected.Where(this.Eligible).Distinct()).ToArray();
                var include = select?.End.Include ?? result.Include;
                if (isObject)
                {
                    if (this.collections.ContainsKey(name)) { throw new ReadRequestException($"Output '{name}' is both an object and a collection. Give those results distinct names."); }
                    var value = values.SingleOrDefault();
                    this.objects[name] = value?.Id;
                    if (value != null) { this.Add(value); this.Include(value, include); }
                }
                else
                {
                    if (this.objects.ContainsKey(name)) { throw new ReadRequestException($"Output '{name}' is both an object and a collection. Give those results distinct names."); }
                    if (!this.collections.TryGetValue(name, out var collection)) { collection = new List<long>(); this.collections.Add(name, collection); }
                    var held = collection.ToHashSet();
                    foreach (var value in values)
                    {
                        if (held.Add(value.Id)) { collection.Add(value.Id); }
                        this.Add(value);
                        this.Include(value, include);
                    }
                }
            }
        }

        private IEnumerable<IObject> Bounded(IEnumerable<IObject> values)
        {
            var count = 0;
            foreach (var value in values)
            {
                this.Visit();
                if (++count > ReadExecutor.MaxPoolObjects) { throw new ReadRequestException("Page or narrow the read selection to at most 10000 objects."); }
                yield return value;
            }
        }

        private IEnumerable<IObject> TraverseSelect(IObject source, Select select)
        {
            this.Visit();
            if (select.PropertyType is IRoleType role && !this.context.AccessControl[source].CanRead(role)) { yield break; }
            foreach (var value in this.Related(source, select.PropertyType))
            {
                if (select.OfType != null && !select.OfType.IsAssignableFrom(value.Strategy.Class)) { continue; }
                if (select.Next == null) { yield return value; }
                else { foreach (var next in this.TraverseSelect(value, select.Next)) { yield return next; } }
            }
        }

        private IEnumerable<IObject> Related(IObject source, IPropertyType property)
        {
            this.Visit();
            if (property is IRoleType role)
            {
                if (role.ObjectType.IsUnit) { yield break; }
                if (role.IsOne)
                {
                    var value = source.Strategy.GetCompositeRole(role);
                    if (value != null) { yield return value; }
                }
                else { foreach (var value in this.Bounded(source.Strategy.GetCompositesRole<IObject>(role))) { yield return value; } }
            }
            else
            {
                var association = (IAssociationType)property;
                if (association.IsOne)
                {
                    var value = source.Strategy.GetCompositeAssociation(association);
                    if (value != null) { yield return value; }
                }
                else { foreach (var value in this.Bounded(source.Strategy.GetCompositesAssociation<IObject>(association))) { yield return value; } }
            }
        }

        private void Include(IObject source, Node[] nodes)
        {
            if (nodes == null) { return; }
            foreach (var node in nodes)
            {
                this.Visit();
                if (!this.included.Add((source.Id, node))) { continue; }
                if (node.PropertyType is IRoleType role && !this.context.AccessControl[source].CanRead(role)) { continue; }
                foreach (var value in this.Related(source, node.PropertyType))
                {
                    // Existing includes apply class eligibility to the pool, and traverse readable ends.
                    this.Add(value);
                    this.Include(value, node.Nodes);
                }
            }
        }

        private void Add(IObject value)
        {
            this.context.EnsureValid();
            if (this.Eligible(value) && this.pool.TryAdd(value.Id, value) && this.pool.Count > ReadExecutor.MaxPoolObjects)
            {
                throw new ReadRequestException("Narrow the includes/dependencies so the complete read pool has at most 10000 objects.");
            }
        }

        private void AddDependencies()
        {
            if (this.compilation.Dependencies == null) { return; }
            var pending = new Queue<IObject>(this.pool.Values);
            while (pending.TryDequeue(out var source))
            {
                this.context.EnsureValid();
                if (!this.compilation.Dependencies.TryGetValue(source.Strategy.Class, out var properties)) { continue; }
                // Configured dependencies intentionally do not use the forward include CanRead rule.
                foreach (var property in properties)
                {
                    foreach (var value in this.Related(source, property).Where(this.Eligible))
                    {
                        // Only new pool members enter the queue, so cycles cannot revisit objects.
                        if (!this.pool.ContainsKey(value.Id)) { this.Add(value); pending.Enqueue(value); }
                    }
                }
            }
        }

        private ReadRecord Record(IObject value)
        {
            this.context.EnsureValid();
            var acl = this.context.AccessControl[value];
            var record = new ReadRecord
            {
                i = value.Id, c = value.Strategy.Class.Tag, v = value.Strategy.ObjectVersion,
                g = acl.Grants.OrderBy(v => v.Id).Select(v => this.Security("grant", v.Id, v.Version, v.PermissionSet)).ToArray(),
                r = acl.Revocations.OrderBy(v => v.Id).Select(v => this.Security("revocation", v.Id, v.Version, v.PermissionSet)).ToArray(),
            };
            if (this.request.k == "discover") { return record; }
            var reusable = Array.FindIndex(this.request.h?.r ?? Array.Empty<ReadRecordHeader>(), candidate =>
                candidate.i == record.i && candidate.c == record.c && candidate.v == record.v
                && this.SameSecurity(candidate.g, record.g) && this.SameSecurity(candidate.r, record.r));
            record.p = reusable >= 0 ? new ReadRecordPayload { r = reusable } : new ReadRecordPayload
            {
                ro = this.roleTypes[value.Strategy.Class].OrderBy(v => v.RelationType.Tag, StringComparer.Ordinal)
                    .Where(v => acl.CanRead(v) && value.Strategy.ExistRole(v)).Select(v => this.Role(value, v)).ToArray(),
            };
            this.context.EnsureValid();
            return record;
        }

        private bool SameSecurity(int[] candidate, int[] current)
        {
            var advertised = candidate.Select(v => this.request.h.s[v]).Select(v => (v.k, v.i, v.v)).ToHashSet();
            return advertised.SetEquals(current.Select(v => this.security[v]).Select(v => (v.k, v.i, v.v)));
        }

        private int Security(string kind, long id, long version, ISet<long> permissions)
        {
            this.context.EnsureValid();
            var key = (kind, id, version);
            var body = permissions.OrderBy(v => v).ToArray();
            if (this.securityEntries.TryGetValue(key, out var entry))
            {
                if (!body.SequenceEqual(entry.Body)) { throw new InvalidOperationException("Security changed within one committed read version. Correct security derivation before reading."); }
                return entry.Index;
            }

            var index = this.security.Count;
            if (index >= ReadExecutor.MaxSecurityEntries) { throw new ReadRequestException("Narrow the read selection to reduce its security closure."); }
            this.permissionIds.UnionWith(body);
            if (this.permissionIds.Count > ReadExecutor.MaxPermissionEntries) { throw new ReadRequestException("Narrow the read selection to reduce its permission closure."); }
            this.securityEntries.Add(key, (index, body));
            var reuse = Array.FindIndex(this.request.h?.s ?? Array.Empty<ReadSecurityHint>(), v => v.k == kind && v.i == id && v.v == version && v.h);
            this.security.Add(new ReadSecurity { k = kind, i = id, v = version, p = reuse >= 0 ? new ReadSecurityPayload { r = reuse } : new ReadSecurityPayload { p = body } });
            return index;
        }

        private ReadPermission Permission(long id)
        {
            this.context.EnsureValid();
            if (this.transaction.Instantiate(id) is not IPermission permission || !this.classes.Contains(permission.Class))
            {
                throw new InvalidOperationException("A read security body contains a missing or workspace-ineligible permission. Correct security derivation before reading.");
            }

            var descriptor = permission switch
            {
                IReadPermission read => new ReadPermissionPayload { c = read.Class.Tag, o = (long)Operations.Read, t = read.RelationType.Tag },
                IWritePermission write => new ReadPermissionPayload { c = write.Class.Tag, o = (long)Operations.Write, t = write.RelationType.Tag },
                IExecutePermission execute => new ReadPermissionPayload { c = execute.Class.Tag, o = (long)Operations.Execute, t = execute.MethodType.Tag },
                ICreatePermission create => new ReadPermissionPayload { c = create.Class.Tag, o = (long)Operations.Create },
                _ => throw new InvalidOperationException("The permission operation cannot be encoded by the read protocol."),
            };
            var reuse = Array.IndexOf(this.request.h?.p ?? Array.Empty<long>(), id);
            return new ReadPermission { i = id, p = reuse >= 0 ? new ReadPermissionPayload { r = reuse } : descriptor };
        }

        private SyncResponseRole Role(IObject source, IRoleType role)
        {
            this.context.EnsureValid();
            var result = new SyncResponseRole { t = role.RelationType.Tag };
            if (role.ObjectType.IsUnit) { result.v = this.unitConvert.ToJson(source.Strategy.GetUnitRole(role)); }
            else if (role.IsOne)
            {
                var value = source.Strategy.GetCompositeRole(role);
                if (this.Eligible(value)) { result.o = value.Id; }
            }
            else { result.c = this.Bounded(source.Strategy.GetCompositesRole<IObject>(role).Where(value => { this.Visit(); return this.Eligible(value); })).Select(v => v.Id).OrderBy(v => v).ToArray(); }
            return result;
        }
    }
}
