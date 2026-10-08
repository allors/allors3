// <copyright file="PullResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Meta;

    /// <summary>
    /// What a pull answered, in ids: the named objects, the named collections, the named values
    /// and the pool of every object the answer names, whose records the connection holds. The
    /// names compare without regard to case.
    /// </summary>
    public sealed class PullResult : Result
    {
        private readonly PullResponse response;
        private readonly IUnitConvert unitConvert;

        private IReadOnlyDictionary<string, long> objects;
        private IReadOnlyDictionary<string, long[]> collections;
        private IReadOnlyDictionary<string, object> values;
        private IReadOnlyList<long> pool;

        internal PullResult(IMetaPopulation metaPopulation, IUnitConvert unitConvert, PullResponse response) : base(metaPopulation, response)
        {
            this.response = response;
            this.unitConvert = unitConvert;
        }

        public IReadOnlyDictionary<string, long> Objects => this.objects ??= ByName(this.response.o);

        public IReadOnlyDictionary<string, long[]> Collections => this.collections ??= ByName(this.response.c);

        /// <summary>
        /// The values as the wire delivered them; <see cref="GetValue{T}"/> converts one.
        /// </summary>
        public IReadOnlyDictionary<string, object> Values => this.values ??= ByName(this.response.v);

        /// <summary>
        /// The ids of every object the answer names, in objects, collections and includes.
        /// </summary>
        public IReadOnlyList<long> Pool => this.pool ??= this.response.p?.Select(v => v.i).ToArray() ?? Array.Empty<long>();

        /// <summary>
        /// The named value as a unit of the type asked for; default when the answer has no such
        /// value.
        /// </summary>
        public T GetValue<T>(string name)
        {
            if (!this.Values.TryGetValue(name, out var value))
            {
                return default;
            }

            return value switch
            {
                null => default,
                T typed => typed,
                _ => (T)this.unitConvert.UnitFromJson(UnitTagForType(typeof(T)), value),
            };
        }

        private static IReadOnlyDictionary<string, T> ByName<T>(IDictionary<string, T> byName)
        {
            var dictionary = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            if (byName != null)
            {
                foreach (var kvp in byName)
                {
                    dictionary[kvp.Key] = kvp.Value;
                }
            }

            return new ReadOnlyDictionary<string, T>(dictionary);
        }

        private static string UnitTagForType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type == typeof(int))
            {
                return UnitTags.Integer;
            }

            if (type == typeof(string))
            {
                return UnitTags.String;
            }

            if (type == typeof(bool))
            {
                return UnitTags.Boolean;
            }

            if (type == typeof(decimal))
            {
                return UnitTags.Decimal;
            }

            if (type == typeof(double))
            {
                return UnitTags.Float;
            }

            if (type == typeof(DateTime))
            {
                return UnitTags.DateTime;
            }

            if (type == typeof(Guid))
            {
                return UnitTags.Unique;
            }

            if (type == typeof(byte[]))
            {
                return UnitTags.Binary;
            }

            throw new ArgumentException($"Unit type not supported: {type}");
        }
    }
}
