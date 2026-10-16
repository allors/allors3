// <copyright file="ReadCompiler.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Read;
    using Allors.Protocol.Json.SystemTextJson;
    using Meta;
    using Services;

    internal static class ReadCompiler
    {
        internal static ReadCompilation Compile(ITransaction transaction, string workspaceName,
            ReadPull pull, IUnitConvert unitConvert)
        {
            var meta = transaction.Database.MetaPopulation;
            var workspaceClasses = new HashSet<IClass>(transaction.Database.Services.Get<IMetaCache>().GetWorkspaceClasses(workspaceName));
            var owned = Capture(pull);
            var grammar = new ReadGrammar(meta);
            foreach (var selection in owned.l)
            {
                grammar.Pull(selection);
            }

            var dependencies = new Dictionary<IClass, ISet<IPropertyType>>();
            foreach (var dependency in owned.d)
            {
                var composite = grammar.Composite(dependency.o, "$pull.dependency.o");
                var property = grammar.Property(dependency.a, dependency.r, composite, "$pull.dependency");
                ReadGrammar.Check(property.ObjectType.IsComposite, "$pull.dependency", "Use a composite association or role for a dependency.");
                foreach (var @class in composite.DatabaseClasses)
                {
                    if (!dependencies.TryGetValue(@class, out var properties))
                    {
                        properties = new HashSet<IPropertyType>();
                        dependencies.Add(@class, properties);
                    }

                    properties.Add(property);
                }
            }

            // Every declaration is admitted before resolving even the first explicit object.
            var fromJson = new FromJson(transaction, unitConvert);
            var pulls = new Data.Pull[owned.l.Length];
            var targetIds = new long?[owned.l.Length];
            for (var i = 0; i < owned.l.Length; ++i)
            {
                var selection = owned.l[i];
                targetIds[i] = selection.o;
                selection.o = null;
                var visitor = new FromJsonVisitor(fromJson);
                selection.Accept(visitor);
                pulls[i] = visitor.Pull;
                if (selection.r?.Length == 0)
                {
                    // An explicit empty result list suppresses explicit-object outputs.
                    pulls[i].Results = Array.Empty<Data.Result>();
                }
                pulls[i].Arguments = selection.a == null ? null : new ReadArguments(selection.a, unitConvert);
            }

            try
            {
                fromJson.Resolve();
            }
            catch (KeyNotFoundException exception)
            {
                throw new ReadRequestException("A predicate object operand is unavailable. Refresh its committed object ID before requesting this selection again.", exception);
            }
            for (var i = 0; i < pulls.Length; ++i)
            {
                if (targetIds[i].HasValue)
                {
                    var target = transaction.Instantiate(targetIds[i].Value);
                    // Ineligible and absent explicit targets have the same unavailable outcome.
                    // Do not let runtime class metadata influence validation or inferred names.
                    pulls[i].Object = target != null && workspaceClasses.Contains(target.Strategy.Class) ? target : null;
                    if (pulls[i].Object != null && owned.l[i].t == null)
                    {
                        owned.l[i].o = targetIds[i];
                        grammar.Pull(owned.l[i], pulls[i].Object.Strategy.Class);
                    }
                }
            }

            return new ReadCompilation { Pulls = pulls, Dependencies = dependencies };
        }

        internal static ReadPull Capture(ReadPull pull)
        {
            try
            {
                return ReadJson.DeserializeRequest(JsonSerializer.Serialize(new ReadRequest
                {
                    v = 1, i = "read-capture", k = "read", p = pull,
                })).p;
            }
            catch (Exception exception) when (exception is JsonException || exception is NotSupportedException)
            {
                throw new ReadRequestException("Supply an acyclic declarative read tree containing only supported JSON data.", exception);
            }
        }

        private sealed class ReadArguments : Data.IArguments
        {
            private readonly IDictionary<string, object> arguments;
            private readonly IUnitConvert convert;

            internal ReadArguments(IDictionary<string, object> arguments, IUnitConvert convert)
            {
                this.arguments = arguments;
                this.convert = convert;
            }

            public bool HasArgument(string name) => this.arguments.ContainsKey(name);

            public object ResolveUnit(string tag, string name) => this.convert.UnitFromJson(tag, this.arguments[name]);

            public object[] ResolveUnits(string tag, string name) => ((JsonElement)this.arguments[name]).EnumerateArray()
                .Select(value => this.convert.UnitFromJson(tag, value)).ToArray();

            public long? ResolveObject(string name) => this.convert.LongFromJson(this.arguments[name]);

            public long[] ResolveObjects(string name) => this.convert.LongArrayFromJson(this.arguments[name]);

            public string ResolveMetaObject(string name) => this.convert.StringFromJson(this.arguments[name]);
        }
    }

    internal sealed class ReadCompilation
    {
        internal Data.Pull[] Pulls { get; init; }

        internal IDictionary<IClass, ISet<IPropertyType>> Dependencies { get; init; }
    }
}
